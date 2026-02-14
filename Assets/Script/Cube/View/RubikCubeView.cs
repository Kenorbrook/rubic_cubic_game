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

  

    public void Initialize(RubikCubeViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeFaceViews();
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
        _viewModel.OnInputUnblockRequested += HandleShuffleCompleted;
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
            _viewModel.OnInputUnblockRequested -= HandleShuffleCompleted;
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
    
    private void HandleShuffleCompleted()
    {
        SetAnimationSpeed(_normalSpeed); 
    }
    
    private void HandleLayerRotationStarted(CubeAxis axis, int index, RotationDirection dir)
    {
        _pendingAnimations = 0;
        if (axis == CubeAxis.Y)
            AnimateLayerY(index, dir);
        else if (axis == CubeAxis.X)
            AnimateLayerX(index, dir);
        else if (axis == CubeAxis.Z)
            AnimateLayerZ(index, dir);
        
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
        face.AnimateRow(cells, dir, OnSingleAnimationCompleted);
    }

    private void AnimateColumn(CubeFaceView face, int col, RotationDirection dir)
    {
        _pendingAnimations++;
        var cells = face.GetColumn(col);
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
        SetAnimationSpeed(_shuffleSpeed); 
        _viewModel.ShuffleCube(20);
    }

    private void OnResetClicked()
    {
        _viewModel.ResetCube();
        UpdateAllFaces(false);
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