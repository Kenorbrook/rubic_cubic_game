using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для анимации изменения цвета ячейки
/// </summary>
public class CellColorAnimator : MonoBehaviour
{
    private Image _cellImage;
    [SerializeField] private float _colorTransitionDuration = 0.2f;

    private Coroutine _colorTransition;

    /// <summary>
    /// Инициализация аниматора с ссылкой на Image
    /// </summary>
    public void Initialize(Image cellImage)
    {
        _cellImage = cellImage;
    }

    /// <summary>
    /// Плавно изменить цвет ячейки
    /// </summary>
    public void AnimateColorChange(Color targetColor)
    {
        if (_colorTransition != null)
        {
            StopCoroutine(_colorTransition);
        }

        _colorTransition = StartCoroutine(ColorTransitionCoroutine(targetColor));
    }

    /// <summary>
    /// Установить цвет мгновенно
    /// </summary>
    public void SetColorImmediate(Color color)
    {
        if (_colorTransition != null)
        {
            StopCoroutine(_colorTransition);
            _colorTransition = null;
        }

        if (_cellImage != null)
        {
            _cellImage.color = color;
        }
    }

    /// <summary>
    /// Корутина для плавного перехода цвета
    /// </summary>
    private IEnumerator ColorTransitionCoroutine(Color targetColor)
    {
        if (_cellImage == null)
        {
            yield break;
        }

        Color startColor = _cellImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < _colorTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _colorTransitionDuration;

            _cellImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        _cellImage.color = targetColor;
        _colorTransition = null;
    }
}
