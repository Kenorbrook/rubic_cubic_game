using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер ввода для кубика Рубика
/// Обрабатывает свайпы и определяет, какую грань нужно повернуть
/// Блокирует одновременное вращение нескольких граней
/// </summary>
public class CubeInputController : InputController
{
    [SerializeField] private SwipeDetector _swipeDetector;
    [SerializeField] private RubikCubeView _cubeView;

    private bool _isInputBlocked;

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
    /// Обработка свайпа
    /// </summary>
    private void HandleSwipe(SwipeDirection direction, Vector2 startPosition)
    {
        /*Debug.Log($" TRY Swipe detected: {direction} at position {startPosition}");
        // Блокируем ввод, если уже идет анимация
        if (_isInputBlocked)
        {
            Debug.Log("Input blocked: animation in progress");
            return;
        }

        // Проверяем, что свайп не начался на UI элементе
      

        // Определяем, на какой грани был свайп
        CubeSide? targetSide = GetSwipedFace(startPosition);
        if (!targetSide.HasValue)
        {
            return;
        }

        // Определяем направление вращения на основе свайпа
        RotationDirection? rotationDirection = GetRotationDirection(targetSide.Value, direction);
        if (!rotationDirection.HasValue)
        {
            return;
        }

        // Отправляем команду на вращение
        if (_cubeView != null)
        {
            bool success;
            if(direction== SwipeDirection.Down || direction== SwipeDirection.Up)
                success = _cubeView.RequestColumnRotation(targetSide.Value, rotationDirection.Value);
            else 
                success = _cubeView.RequestRowRotation(targetSide.Value, rotationDirection.Value);
            
            if (success)
            {
                BlockInput();
            }
        }*/
    }

    /// <summary>
    /// Определить, на какой грани был свайп
    /// </summary>
    private CubeSide? GetSwipedFace(Vector2 screenPosition)
    {
        // Используем Raycast для определения, на какой грани был тап
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            // Проверяем, есть ли у объекта компонент CubeFaceView
            CubeFaceView faceView = hit.collider.GetComponent<CubeFaceView>();
            if (faceView != null)
            {
                return faceView.Side;
            }
        }

        return null;
    }

    /// <summary>
    /// Определить направление вращения на основе свайпа
    /// </summary>
    private RotationDirection? GetRotationDirection(CubeSide side, SwipeDirection swipe)
    {
        // Логика зависит от того, на какой грани был свайп
        switch (side)
        {
            case CubeSide.Front:
                if (swipe == SwipeDirection.Right) return RotationDirection.Clockwise;
                if (swipe == SwipeDirection.Left) return RotationDirection.CounterClockwise;
                break;

            case CubeSide.Right:
                if (swipe == SwipeDirection.Up) return RotationDirection.Clockwise;
                if (swipe == SwipeDirection.Down) return RotationDirection.CounterClockwise;
                break;

            case CubeSide.Left:
                if (swipe == SwipeDirection.Up) return RotationDirection.CounterClockwise;
                if (swipe == SwipeDirection.Down) return RotationDirection.Clockwise;
                break;

            case CubeSide.Top:
                if (swipe == SwipeDirection.Right) return RotationDirection.Clockwise;
                if (swipe == SwipeDirection.Left) return RotationDirection.CounterClockwise;
                break;

            case CubeSide.Bottom:
                if (swipe == SwipeDirection.Right) return RotationDirection.CounterClockwise;
                if (swipe == SwipeDirection.Left) return RotationDirection.Clockwise;
                break;
        }

        return null;
    }

    /// <summary>
    /// Проверить, находится ли указатель над UI элементом
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
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