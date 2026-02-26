using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер ввода для передней грани с поддержкой вращения строк и столбцов
/// Определяет, на какой строке/столбце был свайп и в каком направлении
/// </summary>
public class FrontFaceInputController : InputController
{
    [SerializeField] private SwipeDetector _swipeDetector;
    [SerializeField] private RubikCubeView _cubeView;
    [SerializeField] private RectTransform _frontFaceRect; // RectTransform передней грани

    private bool _isInputBlocked;
    public event System.Action<SwipeDirection> FrontFaceSwipePerformed;

    private void OnEnable()
    {
        if (_swipeDetector != null)
        {
            _swipeDetector.OnSwipe += HandleSwipe;
        }
    }

    private void OnDisable()
    {
        if (_swipeDetector != null)
        {
            _swipeDetector.OnSwipe -= HandleSwipe;
        }
    }

    /// <summary>
    /// Обработка свайпа на передней грани
    /// </summary>
    private void HandleSwipe(SwipeDirection direction, Vector2 startPosition)
    {
        // Блокируем ввод, если уже идет анимация
        if (_isInputBlocked)
        {
            Debug.Log("Input blocked: animation in progress");
            return;
        }
        
        if (MenuPanel.IsOpen)
            return;

        // Проверяем, что свайп был на передней грани
        if (!IsPointerOverFrontFace(startPosition))
        {
            Debug.Log("Swipe not over front face");
            return;
        }


        // Определяем локальную позицию на грани
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _frontFaceRect,
            startPosition,
            Camera.main,
            out localPoint
        );

        // Определяем, на какой строке/столбце был свайп
        GetCellPosition(localPoint, out var row, out var col);

        if (row < 0 || row > 2 || col < 0 || col > 2)
        {
            return;
        }

        // Определяем, что вращать: строку или столбец
        bool success = false;
        if (direction == SwipeDirection.Left || direction == SwipeDirection.Right)
        {
            // Горизонтальный свайп - вращаем строку
            RotationDirection rotDir = direction == SwipeDirection.Right
                ? RotationDirection.Clockwise
                : RotationDirection.CounterClockwise;

            success = _cubeView.RequestRowRotation(row, rotDir, MoveSource.User);
        }
        else if (direction == SwipeDirection.Up || direction == SwipeDirection.Down)
        {
            // Вертикальный свайп - вращаем столбец
            RotationDirection rotDir = direction == SwipeDirection.Down
                ? RotationDirection.Clockwise
                : RotationDirection.CounterClockwise;

            success = _cubeView.RequestColumnRotation(col, rotDir, MoveSource.User);
        }

        if (success)
        {
            FrontFaceSwipePerformed?.Invoke(direction);
            BlockInput();
        }
    }

    private void GetCellPosition(Vector2 localPoint, out int row, out int col)
    {
        Rect rect = _frontFaceRect.rect;

        float cellWidth  = rect.width / 3f;
        float cellHeight = rect.height / 3f;

        float x = localPoint.x - rect.xMin;
        float y = rect.yMax - localPoint.y;

        col = Mathf.FloorToInt(x / cellWidth);
        row = Mathf.FloorToInt(y / cellHeight);

        col = Mathf.Clamp(col, 0, 2);
        row = Mathf.Clamp(row, 0, 2);
    }



    /// <summary>
    /// Проверить, находится ли указатель над передней гранью
    /// </summary>
    private bool IsPointerOverFrontFace(Vector2 screenPosition)
    {
        if (_frontFaceRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(_frontFaceRect, screenPosition, Camera.main);
    }

    /// <summary>
    /// Проверить, находится ли указатель над UI элементом
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    /// <summary>
    /// Заблокировать ввод (вызывается при начале анимации)
    /// </summary>
    public override void BlockInput()
    {
        _isInputBlocked = true;
    }

    /// <summary>
    /// Разблокировать ввод (вызывается после окончания анимации)
    /// </summary>
    public override void UnblockInput()
    {
        _isInputBlocked = false;
    }
}
