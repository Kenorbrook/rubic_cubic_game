using System;
using UnityEngine;
using UnityEngine.InputSystem;


public enum SwipeDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}


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
        if (MenuPanel.IsOpen)
            return;

        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                StartSwipe(touch.position.ReadValue());
            }
            else if (touch.press.wasReleasedThisFrame && _isSwiping)
            {
                EndSwipe(touch.position.ReadValue());
            }
        }
        if (Mouse.current != null)
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

    private void StartSwipe(Vector2 position)
    {
        _startTouchPosition = position;
        _startTime = Time.time;
        _isSwiping = true;
    }

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

    private SwipeDirection GetSwipeDirection()
    {
        Vector2 swipeVector = _endTouchPosition - _startTouchPosition;

        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            return swipeVector.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }

        return swipeVector.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
    }

    public Vector2 GetSwipeStartPosition()
    {
        return _startTouchPosition;
    }
}

