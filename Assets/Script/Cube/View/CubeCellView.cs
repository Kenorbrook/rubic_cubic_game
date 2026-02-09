using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View: Представление одной ячейки грани кубика
/// </summary>
[RequireComponent(typeof(Image))]
public class CubeCellView : MonoBehaviour
{
    [SerializeField] private Image _cellImage;
    [SerializeField] private CellColorAnimator _colorAnimator;

    /// <summary>
    /// Инициализация ячейки (вызывается вручную для контроля порядка)
    /// </summary>
    public void Initialize()
    {
        // Если не назначено в инспекторе, получаем компоненты
        if (_cellImage == null)
        {
            _cellImage = GetComponent<Image>();
        }

        if (_colorAnimator == null)
        {
            _colorAnimator = GetComponent<CellColorAnimator>();
        }

        // Инициализируем аниматор
        if (_colorAnimator != null)
        {
            _colorAnimator.Initialize(_cellImage);
        }
    }

    /// <summary>
    /// Установить цвет ячейки с анимацией
    /// </summary>
    public void SetColor(CubeColor color, bool animate = true)
    {
        Color unityColor = color.ToUnityColor();

        if (animate && _colorAnimator != null)
        {
            _colorAnimator.AnimateColorChange(unityColor);
        }
        else
        {
            if (_colorAnimator != null)
            {
                _colorAnimator.SetColorImmediate(unityColor);
            }
            else
            {
                _cellImage.color = unityColor;
            }
        }
    }

    /// <summary>
    /// Получить текущий цвет
    /// </summary>
    public Color GetCurrentColor()
    {
        return _cellImage.color;
    }
}
