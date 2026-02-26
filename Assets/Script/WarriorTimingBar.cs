using UnityEngine;
using UnityEngine.UI;

public class WarriorTimingBar : MonoBehaviour
{
    [SerializeField] private RectTransform _rect;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _arrowImage;

    public RectTransform Rect => _rect;

    private void Awake()
    {
        if (_rect == null)
            _rect = transform as RectTransform;
    }

    public void SetPrompt(Sprite arrowSprite, Color fillColor)
    {
        if (_arrowImage != null)
            _arrowImage.sprite = arrowSprite;

        if (_fillImage != null)
            _fillImage.color = fillColor;
    }

    public void SetFillColor(Color color)
    {
        if (_fillImage != null)
            _fillImage.color = color;
    }
}
