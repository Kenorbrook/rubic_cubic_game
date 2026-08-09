using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RubikCubeView : MonoBehaviour
{
    [SerializeField] float _normalSpeed = 1f;
    [SerializeField] float _shuffleSpeed = 3f;
    
    
    [Header("Face Views")]
    
    [SerializeField]
    private CubeFaceView _frontFaceView;

    [SerializeField]
    private CubeFaceView _backFaceView;

    [SerializeField]
    private CubeFaceView _rightFaceView;

    [SerializeField]
    private CubeFaceView _leftFaceView;

    [SerializeField]
    private CubeFaceView _topFaceView;

    [SerializeField]
    private CubeFaceView _bottomFaceView;

    [Header("Input")]
    
    [SerializeField]
    private List<InputController> _inputControllers;

    [Header("UI")]
    
    [SerializeField]
    private Button _shuffleButton;

    [SerializeField]
    private Button _resetButton;

    private RubikCubeViewModel _viewModel;
    private Dictionary<CubeSide, CubeFaceView> _faceViews;


    private int _pendingAnimations;
    private RubikCube3DPresenter _threeDPresenter;
    private bool _use3D;
    private bool _previewHorizontal;
    private int _previewIndex = -1;
    private float _previewOffset;
    private Transform[] _previewCells;
    private Vector3[] _previewBasePositions;

  

    public void Initialize(RubikCubeViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeFaceViews();
        _threeDPresenter = GetComponent<RubikCube3DPresenter>();
        if (_threeDPresenter == null)
            _threeDPresenter = gameObject.AddComponent<RubikCube3DPresenter>();
        _threeDPresenter.Initialize(this, viewModel);
        Set3DMode(CubeViewPreferences.Use3D);
        SetupUI();
        StartCoroutine(InitializeViewModel());
    }
    
    
    #region Init

    
    private void InitializeFaceViews()
    {
        _faceViews = new Dictionary<CubeSide, CubeFaceView>
        {
            {CubeSide.Front, _frontFaceView},
            {CubeSide.Right, _rightFaceView},
            {CubeSide.Left, _leftFaceView},
            {CubeSide.Top, _topFaceView},
            {CubeSide.Bottom, _bottomFaceView}
        };

        foreach (var face in _faceViews.Values)
        {
            face?.Initialize();
        }
    }

    private System.Collections.IEnumerator InitializeViewModel()
    {
        _viewModel.OnInputBlockRequested += BlockInput;
        _viewModel.OnInputUnblockRequested += UnblockInput;
        _viewModel.OnLayerRotationStarted += HandleLayerRotationStarted;

        _viewModel.OnCubeDataChanged += HandleFaceUpdated;
        _viewModel.OnCubeSolved += HandleCubeSolved;

        UpdateAllFaces(false);

        yield return null;
        OnShuffleClicked();
    }

    private void SetupUI()
    {
        if (_shuffleButton != null)
            _shuffleButton.onClick.AddListener(OnShuffleClicked);

        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetClicked);
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.Dispose();
            _viewModel.OnInputBlockRequested -= BlockInput;
            _viewModel.OnInputUnblockRequested -= UnblockInput;
            _viewModel.OnLayerRotationStarted -= HandleLayerRotationStarted;
            _viewModel.OnCubeDataChanged -= HandleFaceUpdated;
            _viewModel.OnCubeSolved -= HandleCubeSolved;
        }
    }

    #endregion

    public bool RequestRowRotation(int rowIndex, RotationDirection direction, MoveSource source)
    {
        return _viewModel.TryRotateLayer(CubeAxis.Y, rowIndex, direction, source);
    }

    public bool RequestColumnRotation(int colIndex, RotationDirection direction, MoveSource source)
    {
        return _viewModel.TryRotateLayer(CubeAxis.X, colIndex, direction, source);
    }
    
    private void HandleLayerRotationStarted(CubeAxis axis, int index, RotationDirection dir, MoveSource source)
    {
        if (_use3D)
        {
            _threeDPresenter.AnimateLayer(axis, index, dir, source);
            return;
        }
        // View decides animation speed based on move metadata from VM.
        SetAnimationSpeed(source == MoveSource.Shuffle ? _shuffleSpeed : _normalSpeed);

        _pendingAnimations = 0;
        // The 2D net animates the layer the player actually touched. Moving all
        // neighbouring mini-faces at different scales caused cells to scatter
        // and made the final direction look reversed on mobile.
        if (axis == CubeAxis.Y)
            AnimateRow(_frontFaceView, index, dir);
        else if (axis == CubeAxis.X)
            AnimateColumn(_frontFaceView, index, dir);
        else
        {
            _viewModel.CompleteRotation();
            return;
        }
        
    }
    
    private void AnimateLayerY(int row, RotationDirection dir)
    {
        AnimateRow(_frontFaceView, row, dir);

        AnimateRow(_leftFaceView, row, dir);

        AnimateRow(_rightFaceView, row, dir);

        if (row == 0)
            AnimateFace(_topFaceView, Invert(dir));
        else if (row == 2)
            AnimateFace(_bottomFaceView, dir);
    }
    
    private void AnimateLayerX(int col, RotationDirection dir)
    {
        AnimateColumn(_frontFaceView, col, dir);

        AnimateColumn(_topFaceView, col, dir);

        AnimateColumn(_bottomFaceView, col, dir);

        if (col == 0)
            AnimateFace(_leftFaceView, dir);
        else if (col == 2)
            AnimateFace(_rightFaceView, Invert(dir));
    }
    
    private void AnimateLayerZ(int col, RotationDirection dir)
    {
        AnimateRow(_topFaceView, col, dir);
        AnimateRow(_bottomFaceView, col, dir);

        

        AnimateColumn(_rightFaceView, col, dir);
        
        AnimateColumn(_leftFaceView, col, dir);

         if (col == 2)
            AnimateFace(_frontFaceView, dir);
    }
    private void AnimateRow(CubeFaceView face, int row, RotationDirection dir)
    {
        _pendingAnimations++;
        var cells = face.GetRow(row);
        if (face == _frontFaceView && _previewHorizontal && _previewIndex == row)
        {
            RestorePreviewPositions();
            face.AnimateRow(cells, dir, _previewOffset, OnSingleAnimationCompleted);
            ClearPreviewState();
        }
        else
            face.AnimateRow(cells, dir, OnSingleAnimationCompleted);
    }

    private void AnimateColumn(CubeFaceView face, int col, RotationDirection dir)
    {
        _pendingAnimations++;
        var cells = face.GetColumn(col);
        if (face == _frontFaceView && !_previewHorizontal && _previewIndex == col)
        {
            RestorePreviewPositions();
            face.AnimateColumn(cells, dir, _previewOffset, OnSingleAnimationCompleted);
            ClearPreviewState();
        }
        else
            face.AnimateColumn(cells, dir, OnSingleAnimationCompleted);
    }

    private void AnimateFace(CubeFaceView face, RotationDirection dir)
    {
        _pendingAnimations++;
        var cells = face.GetAll();
        face.AnimateFace(cells, dir, OnSingleAnimationCompleted);
    }

    private RotationDirection Invert(RotationDirection d)
    {
        return d == RotationDirection.Clockwise
            ? RotationDirection.CounterClockwise
            : RotationDirection.Clockwise;
    }


    private void OnSingleAnimationCompleted()
    {
        _pendingAnimations--;
        if (_pendingAnimations <= 0)
        {
            _viewModel.CompleteRotation();
        }
    }


    private void HandleFaceUpdated(Dictionary<CubeSide, CubeFaceModel> models)
    {
        _threeDPresenter?.RefreshColors();
        foreach (var face in _faceViews)
        {
            if (_faceViews.TryGetValue(face.Key, out var view))
            {
                var model = models[face.Key];
                view.UpdateDisplay(model, false);
            }
        }
    }

    private void SetAnimationSpeed(float multiplier)
    {
        foreach (var face in _faceViews.Values)
            face.SetSpeed(multiplier);
    }

    private void HandleCubeSolved()
    {
        Debug.Log("Cube solved!");
    }


    private void UpdateAllFaces(bool animate)
    {
        foreach (var kv in _faceViews)
        {
            var model = _viewModel.GetFaceData(kv.Key);
            kv.Value.UpdateDisplay(model, animate);
        }
    }

    private void OnShuffleClicked()
    {
        _viewModel.ShuffleCube(20);
    }

    public void PreviewFrontLayer(bool horizontal, int index, float offset)
    {
        if (_use3D || _viewModel == null || _viewModel.IsAnimating) return;
        if (_previewCells == null || _previewHorizontal != horizontal || _previewIndex != index)
        {
            CancelFrontLayerPreview();
            _previewHorizontal = horizontal;
            _previewIndex = index;
            _previewCells = horizontal ? _frontFaceView.GetRow(index) : _frontFaceView.GetColumn(index);
            _previewBasePositions = new Vector3[_previewCells.Length];
            for (var i = 0; i < _previewCells.Length; i++) _previewBasePositions[i] = _previewCells[i].localPosition;
        }
        _previewOffset = offset;
        RowColumnAnimator.ApplyWrappedOffset(_previewCells, _previewBasePositions, offset, horizontal);
    }

    public void CancelFrontLayerPreview()
    {
        RestorePreviewPositions();
        ClearPreviewState();
    }

    private void RestorePreviewPositions()
    {
        if (_previewCells == null || _previewBasePositions == null) return;
        for (var i = 0; i < _previewCells.Length; i++)
            if (_previewCells[i] != null) _previewCells[i].localPosition = _previewBasePositions[i];
    }

    private void ClearPreviewState()
    {
        _previewCells = null;
        _previewBasePositions = null;
        _previewIndex = -1;
        _previewOffset = 0f;
    }

    public void CompleteVisualRotation()
    {
        _viewModel.CompleteRotation();
    }

    public void Set3DMode(bool enabled)
    {
        _use3D = enabled;
        _threeDPresenter?.SetVisible(enabled);
        foreach (var face in _faceViews.Values)
        {
            if (face == null)
                continue;

            face.gameObject.SetActive(!enabled);
            if (!enabled)
                face.FreezeGridLayout();
        }
    }

    public bool Is3DMode => _use3D;

    public void Notify3DUserSwipe(SwipeDirection direction)
    {
        if (!_use3D || _inputControllers == null)
            return;

        foreach (var controller in _inputControllers)
        {
            if (controller is FrontFaceInputController frontFaceInput)
            {
                frontFaceInput.NotifyExternalSwipe(direction);
                return;
            }
        }
    }

    private void OnResetClicked()
    {
        _viewModel.ResetCube();
        _threeDPresenter?.RecenterView();
        UpdateAllFaces(false);
        _threeDPresenter?.RefreshColors();
    }


    private void BlockInput()
    {
        if (_inputControllers == null) return;

        foreach (var c in _inputControllers)
            c.BlockInput();
    }

    private void UnblockInput()
    {
        if (_inputControllers == null) return;

        foreach (var c in _inputControllers)
            c.UnblockInput();
    }
}
