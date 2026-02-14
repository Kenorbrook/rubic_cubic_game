using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Компонент для анимации вращения строки или столбца
/// </summary>
public class RowColumnAnimator : MonoBehaviour
{
    public float SpeedMultiplier { get; set; } = 1f;
    
    [SerializeField]
    private float _rotationDuration = 0.3f;

    [SerializeField]
    private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _currentAnimation;


    public void AnimateRowRotation(Transform[] cells, RotationDirection direction, Action onComplete)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(RowRotationCoroutine(cells, direction, onComplete));
    }

    public void AnimateAllRotation(Transform[] cells, RotationDirection direction, Action onComplete)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AllRotationCoroutine(cells, direction, onComplete));
    }

    public void AnimateColumnRotation(Transform[] cells, RotationDirection direction, Action onComplete)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(ColumnRotationCoroutine(cells, direction, onComplete));
    }

    public void StopAnimation()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }

    }
    
    private IEnumerator RowRotationCoroutine(Transform[] cells, RotationDirection direction, Action onComplete)
    {

        float moveDistance = 100f;
        float targetOffset = direction == RotationDirection.Clockwise ? moveDistance : -moveDistance;

        Vector3[] startPositions = new Vector3[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            startPositions[i] = cells[i].localPosition;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime * SpeedMultiplier;

            float t = elapsedTime / _rotationDuration;
            float curveValue = _rotationCurve.Evaluate(t);

            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 offset = new Vector3(targetOffset * curveValue, 0, 0);
                cells[i].localPosition = startPositions[i] + offset;
            }

            yield return null;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].localPosition = startPositions[i];
        }

        _currentAnimation = null;
        onComplete?.Invoke();
    }

    private IEnumerator AllRotationCoroutine(Transform[] cells, RotationDirection direction, Action onComplete)
    {

        if (cells == null || cells.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        Transform first = cells[0];
        Transform parent = first.parent;

        GameObject pivotObj = new GameObject("FaceRotationPivot");
        Transform pivot = pivotObj.transform;
        pivot.SetParent(parent, false);

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var c in cells)
        {
            center += c.localPosition;
            count++;
        }

        center /= Mathf.Max(1, count);
        pivot.localPosition = center;

        Vector3[] startPositions = new Vector3[cells.Length];
        Quaternion[] startRotations = new Quaternion[cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            startPositions[i] = cells[i].localPosition;
            startRotations[i] = cells[i].localRotation;
            cells[i].SetParent(pivot, true);
        }
        float elapsedTime = 0f;
        float targetAngle = direction == RotationDirection.Clockwise ? -90f : 90f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime * SpeedMultiplier;

            float t = Mathf.Clamp01(elapsedTime / _rotationDuration);
            float curve = _rotationCurve.Evaluate(t);

            pivot.localRotation = Quaternion.Euler(0f, 0f, targetAngle * curve);

            yield return null;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].SetParent(parent, true);
            cells[i].localPosition = startPositions[i];
            cells[i].localRotation = startRotations[i];
        }

        Destroy(pivotObj);
        _currentAnimation = null;
        onComplete?.Invoke();
    }

    private IEnumerator ColumnRotationCoroutine(Transform[] cells, RotationDirection direction, Action onComplete)
    {

        float moveDistance = 100f; 
        float targetOffset = direction == RotationDirection.Clockwise ? -moveDistance : moveDistance;

        Vector3[] startPositions = new Vector3[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            startPositions[i] = cells[i].localPosition;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime * SpeedMultiplier;

            float t = elapsedTime / _rotationDuration;
            float curveValue = _rotationCurve.Evaluate(t);

            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 offset = new Vector3(0, targetOffset * curveValue, 0);
                cells[i].localPosition = startPositions[i] + offset;
            }

            yield return null;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].localPosition = startPositions[i];
        }

        _currentAnimation = null;

        onComplete?.Invoke();
    }

}