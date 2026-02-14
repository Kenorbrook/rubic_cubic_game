using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для анимации изменения цвета ячейки
/// </summary>
public class CellColorAnimator : MonoBehaviour
{
    [SerializeField] private float _colorTransitionDuration = 0.2f;
    
    private Image _cellImage;

    private Coroutine _colorTransition;

    public void Initialize(Image cellImage)
    {
        _cellImage = cellImage;
    }

    public void AnimateColorChange(Color targetColor)
    {
        if (_colorTransition != null)
        {
            StopCoroutine(_colorTransition);
        }

        _colorTransition = StartCoroutine(ColorTransitionCoroutine(targetColor));
    }

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
