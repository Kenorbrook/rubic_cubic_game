using UnityEngine;
using UnityEngine.UI;

public class CubeFaceView : MonoBehaviour
{
    [SerializeField] private CubeSide _side;
    [SerializeField] private CubeCellView[] _cells; // 9 штук
    [SerializeField] private RowColumnAnimator _animator;

    public CubeSide Side => _side;

    public void Initialize()
    {
        if (_cells == null || _cells.Length != 9)
        {
            Debug.LogError($"{_side} must have 9 cells");
            return;
        }

        FreezeGridLayout();

        foreach (var c in _cells)
            c?.Initialize();
    }

    public void FreezeGridLayout()
    {
        var grid = GetComponent<GridLayoutGroup>();
        if (grid == null || _cells == null || _cells.Length != 9)
            return;

        // Android can run Initialize before its first Canvas layout pass. In that
        // case disabling GridLayoutGroup used to leave all cells at an invalid
        // position/size and the complete 2D cube appeared missing. Position the
        // authored 3x3 grid explicitly so the result is identical on every device.
        var cellSize = grid.cellSize;
        var spacing = grid.spacing;
        var gridSize = new Vector2(
            cellSize.x * 3f + spacing.x * 2f,
            cellSize.y * 3f + spacing.y * 2f);

        for (var index = 0; index < _cells.Length; index++)
        {
            if (_cells[index] == null || _cells[index].transform is not RectTransform cellRect)
                continue;

            var column = index % 3;
            var row = index / 3;
            cellRect.anchorMin = new Vector2(0.5f, 0.5f);
            cellRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = cellSize;
            cellRect.anchoredPosition = new Vector2(
                -gridSize.x * 0.5f + cellSize.x * 0.5f + column * (cellSize.x + spacing.x),
                gridSize.y * 0.5f - cellSize.y * 0.5f - row * (cellSize.y + spacing.y));
            cellRect.localRotation = Quaternion.identity;
            cellRect.localScale = Vector3.one;
        }

        grid.enabled = false;
    }

    // ===== MODEL → VIEW =====

    public void UpdateDisplay(CubeFaceModel faceModel, bool animate)
    {
        int i = 0;
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = _cells[i++];
                cell.SetColor(faceModel.GetCell(r, c), animate);
                
            }
        }
        
    }

    // ===== CELL ACCESS =====
    public Color[,] GetFaceColor()
    {
        return new Color[3, 3]
        {
            {_cells[0].GetCurrentColor(), _cells[1].GetCurrentColor(), _cells[2].GetCurrentColor()},
            {_cells[3].GetCurrentColor(), _cells[4].GetCurrentColor(), _cells[5].GetCurrentColor()},
            {_cells[6].GetCurrentColor(), _cells[7].GetCurrentColor(), _cells[8].GetCurrentColor()}
        };
    }

    public Transform GetCell(int row, int col)
    {
        int index = row * 3 + col;
        if (index < 0 || index >= _cells.Length) return null;
        return _cells[index].transform;
    }

    public Transform[] GetRow(int row)
    {
        return new[]
        {
            GetCell(row,0),
            GetCell(row,1),
            GetCell(row,2)
        };
    }

    public Transform[] GetColumn(int col)
    {
        return new[]
        {
            GetCell(0,col),
            GetCell(1,col),
            GetCell(2,col)
        };
    }

    public Transform[] GetAll()
    {
        Transform[] t = new Transform[9];
        for (int i = 0; i < 9; i++)
            t[i] = _cells[i].transform;
        return t;
    }
    public void SetSpeed(float multiplier)
    {
        if (_animator != null)
            _animator.SpeedMultiplier = multiplier;
    }
    // ===== ANIMATION WRAPPERS =====

    public void AnimateRow(Transform[] cells, RotationDirection dir, System.Action onComplete)
    {
        _animator.AnimateRowRotation(cells, dir, onComplete);
    }

    public void AnimateRow(Transform[] cells, RotationDirection dir, float initialOffset, System.Action onComplete)
    {
        _animator.AnimateRowRotation(cells, dir, initialOffset, onComplete);
    }

    public void AnimateColumn(Transform[] cells, RotationDirection dir, System.Action onComplete)
    {
        _animator.AnimateColumnRotation(cells, dir, onComplete);
    }

    public void AnimateColumn(Transform[] cells, RotationDirection dir, float initialOffset, System.Action onComplete)
    {
        _animator.AnimateColumnRotation(cells, dir, initialOffset, onComplete);
    }

    public void AnimateFace(Transform[] cells, RotationDirection dir, System.Action onComplete)
    {
        _animator.AnimateAllRotation(cells, dir, onComplete);
    }
}
