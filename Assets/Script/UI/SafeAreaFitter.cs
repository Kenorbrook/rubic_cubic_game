using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        ApplyIfChanged();
    }

    private void OnEnable()
    {
        _rectTransform ??= (RectTransform)transform;
        ApplyIfChanged();
    }

    private void Update()
    {
        ApplyIfChanged();
    }

    private void ApplyIfChanged()
    {
        var screenSize = new Vector2Int(Screen.width, Screen.height);
        var safeArea = Screen.safeArea;
        if (screenSize == _lastScreenSize && safeArea == _lastSafeArea)
            return;

        _lastScreenSize = screenSize;
        _lastSafeArea = safeArea;
        if (screenSize.x <= 0 || screenSize.y <= 0)
            return;

        _rectTransform.anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
        _rectTransform.anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}
