using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Определяет направление свайпа на мобильных устройствах
/// </summary>
public enum SwipeDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Детектор свайпов для мобильных устройств (New Input System)
/// </summary>
public class SwipeDetector : MonoBehaviour
{
    [SerializeField] private float _minSwipeDistance = 50f;
    [SerializeField] private float _maxSwipeTime = 1f;

    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;
    private float _startTime;
    private bool _isSwiping;

    public event Action<SwipeDirection, Vector2> OnSwipe;

    private void Update()
    {
        DetectSwipe();
    }

    /// <summary>
    /// Определение свайпа (New Input System)
    /// </summary>
    private void DetectSwipe()
    {
        // Проверяем тач-ввод (мобильные устройства)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;

            // Начало касания
            if (touch.press.wasPressedThisFrame)
            {
                StartSwipe(touch.position.ReadValue());
            }
            // Конец касания
            else if (touch.press.wasReleasedThisFrame && _isSwiping)
            {
                EndSwipe(touch.position.ReadValue());
            }
        }
        // Проверяем мышь (для тестирования в редакторе)
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSwipe(Mouse.current.position.ReadValue());
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && _isSwiping)
            {
                EndSwipe(Mouse.current.position.ReadValue());
            }
        }
    }

    /// <summary>
    /// Начало свайпа
    /// </summary>
    private void StartSwipe(Vector2 position)
    {
        _startTouchPosition = position;
        _startTime = Time.time;
        _isSwiping = true;
    }

    /// <summary>
    /// Конец свайпа
    /// </summary>
    private void EndSwipe(Vector2 position)
    {
        if (!_isSwiping)
            return;

        _endTouchPosition = position;
        _isSwiping = false;

        float swipeTime = Time.time - _startTime;
        float swipeDistance = Vector2.Distance(_startTouchPosition, _endTouchPosition);


        if (swipeTime <= _maxSwipeTime && swipeDistance >= _minSwipeDistance)
        {
            SwipeDirection direction = GetSwipeDirection();

            if (direction != SwipeDirection.None)
            {
                OnSwipe?.Invoke(direction, _startTouchPosition);
            }
        }
        else
        {
            Debug.Log($"Swipe too slow or short. Required: <{_maxSwipeTime}s and >{_minSwipeDistance}px");
        }
    }

    /// <summary>
    /// Определить направление свайпа
    /// </summary>
    private SwipeDirection GetSwipeDirection()
    {
        Vector2 swipeVector = _endTouchPosition - _startTouchPosition;

        // Определяем, какая ось доминирует
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            // Горизонтальный свайп
            return swipeVector.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }
        else
        {
            // Вертикальный свайп
            return swipeVector.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    /// <summary>
    /// Получить позицию начала свайпа
    /// </summary>
    public Vector2 GetSwipeStartPosition()
    {
        return _startTouchPosition;
    }
}
