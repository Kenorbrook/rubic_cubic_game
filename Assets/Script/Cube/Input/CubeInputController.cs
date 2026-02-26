using UnityEngine;

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
    public override void BlockInput()
    {
        _isInputBlocked = true;
    }

    public override void UnblockInput()
    {
        _isInputBlocked = false;
    }
    
    private void HandleSwipe(SwipeDirection direction, Vector2 startPosition)
    {
        if (_isInputBlocked)
        {
            Debug.Log("Input blocked: animation in progress");
            return;
        }
        
        if (MenuPanel.IsOpen)
            return;

        CubeSide? targetSide = GetSwipedFace(startPosition);
        if (!targetSide.HasValue)
        {
            return;
        }

        RotationDirection? rotationDirection = GetRotationDirection(targetSide.Value, direction);
        if (!rotationDirection.HasValue)
        {
            return;
        }

        if (_cubeView != null)
        {
            bool success;
            if(direction== SwipeDirection.Down || direction== SwipeDirection.Up)
                success = _cubeView.RequestColumnRotation((int)targetSide.Value, rotationDirection.Value, MoveSource.User);
            else 
                success = _cubeView.RequestRowRotation((int)targetSide.Value, rotationDirection.Value, MoveSource.User);
            
            if (success)
            {
                BlockInput();
            }
        }
    }

    private CubeSide? GetSwipedFace(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            CubeFaceView faceView = hit.collider.GetComponent<CubeFaceView>();
            if (faceView != null)
            {
                return faceView.Side;
            }
        }

        return null;
    }

    private RotationDirection? GetRotationDirection(CubeSide side, SwipeDirection swipe)
    {
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

}
