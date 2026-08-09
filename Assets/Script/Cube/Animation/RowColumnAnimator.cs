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
        => AnimateRowRotation(cells, direction, 0f, onComplete);

    public void AnimateRowRotation(Transform[] cells, RotationDirection direction, float initialOffset, Action onComplete)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(RowRotationCoroutine(cells, direction, initialOffset, onComplete));
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
        => AnimateColumnRotation(cells, direction, 0f, onComplete);

    public void AnimateColumnRotation(Transform[] cells, RotationDirection direction, float initialOffset, Action onComplete)
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(ColumnRotationCoroutine(cells, direction, initialOffset, onComplete));
    }

    public void StopAnimation()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }

    }
    
    private IEnumerator RowRotationCoroutine(Transform[] cells, RotationDirection direction, float initialOffset, Action onComplete)
    {
        Vector3[] startPositions = new Vector3[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            startPositions[i] = cells[i].localPosition;
        }

        float moveDistance = GetCellStep(startPositions, true);
        float targetOffset = direction == RotationDirection.Clockwise ? moveDistance : -moveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime * SpeedMultiplier;

            float t = elapsedTime / _rotationDuration;
            float curveValue = _rotationCurve.Evaluate(Mathf.Clamp01(t));
            float offset = Mathf.Lerp(initialOffset, targetOffset, curveValue);
            ApplyWrappedOffset(cells, startPositions, offset, true, moveDistance);

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

    private IEnumerator ColumnRotationCoroutine(Transform[] cells, RotationDirection direction, float initialOffset, Action onComplete)
    {
        Vector3[] startPositions = new Vector3[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            startPositions[i] = cells[i].localPosition;
        }

        float moveDistance = GetCellStep(startPositions, false);
        float targetOffset = direction == RotationDirection.Clockwise ? -moveDistance : moveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < _rotationDuration)
        {
            elapsedTime += Time.deltaTime * SpeedMultiplier;

            float t = elapsedTime / _rotationDuration;
            float curveValue = _rotationCurve.Evaluate(Mathf.Clamp01(t));
            float offset = Mathf.Lerp(initialOffset, targetOffset, curveValue);
            ApplyWrappedOffset(cells, startPositions, offset, false, moveDistance);

            yield return null;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].localPosition = startPositions[i];
        }

        _currentAnimation = null;

        onComplete?.Invoke();
    }

    public static void ApplyWrappedOffset(Transform[] cells, Vector3[] basePositions, float offset, bool horizontal)
    {
        ApplyWrappedOffset(cells, basePositions, offset, horizontal, GetCellStep(basePositions, horizontal));
    }

    private static void ApplyWrappedOffset(Transform[] cells, Vector3[] basePositions, float offset, bool horizontal, float step)
    {
        if (cells == null || basePositions == null || cells.Length != basePositions.Length || step <= 0f)
            return;

        var min = float.MaxValue;
        var max = float.MinValue;
        for (var i = 0; i < basePositions.Length; i++)
        {
            var value = horizontal ? basePositions[i].x : basePositions[i].y;
            min = Mathf.Min(min, value);
            max = Mathf.Max(max, value);
        }

        var cycle = step * cells.Length;
        var lower = min - step * 0.5f;
        var upper = max + step * 0.5f;
        for (var i = 0; i < cells.Length; i++)
        {
            var position = basePositions[i];
            var value = (horizontal ? position.x : position.y) + offset;
            while (value > upper) value -= cycle;
            while (value < lower) value += cycle;
            if (horizontal) position.x = value;
            else position.y = value;
            cells[i].localPosition = position;
        }
    }

    private static float GetCellStep(Vector3[] positions, bool horizontal)
    {
        if (positions == null || positions.Length < 2)
            return 100f;

        var values = new float[positions.Length];
        for (var i = 0; i < positions.Length; i++)
            values[i] = horizontal ? positions[i].x : positions[i].y;
        Array.Sort(values);

        var total = 0f;
        for (var i = 1; i < values.Length; i++)
            total += Mathf.Abs(values[i] - values[i - 1]);
        return Mathf.Max(1f, total / (values.Length - 1));
    }

}
