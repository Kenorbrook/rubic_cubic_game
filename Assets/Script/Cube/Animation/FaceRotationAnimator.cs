using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Компонент для анимации вращения грани кубика
/// </summary>
public class FaceRotationAnimator : MonoBehaviour
{
    public bool IsAnimating => _isAnimating;
    
    [SerializeField] private float _rotationDuration = 0.3f;
    [SerializeField] private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _isAnimating;
    private Coroutine _currentAnimation;


    public void AnimateRotation(Transform faceTransform, RotationDirection direction, Action onComplete)
    {
        if (_isAnimating)
        {
            Debug.LogWarning("Animation already in progress");
            return;
        }

        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(RotateCoroutine(faceTransform, direction, onComplete));
    }

    public void StopAnimation()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }
        _isAnimating = false;
    }
    
    private IEnumerator RotateCoroutine(Transform faceTransform, RotationDirection direction, Action onComplete)
    {
        _isAnimating = true;

        float targetAngle = direction == RotationDirection.Clockwise ? -90f : 90f;
        Quaternion startRotation = faceTransform.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, targetAngle);

        float elapsedTime = 0f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _rotationDuration;
            float curveValue = _rotationCurve.Evaluate(t);

            faceTransform.localRotation = Quaternion.Lerp(startRotation, endRotation, curveValue);

            yield return null;
        }

        faceTransform.localRotation = endRotation;

        _isAnimating = false;
        _currentAnimation = null;

        onComplete?.Invoke();
    }

}
