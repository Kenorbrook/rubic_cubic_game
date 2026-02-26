using System.Collections;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _deathDuration = 0.35f;
    [SerializeField] private float _hitDuration = 0.12f;
    [SerializeField] private float _hitShakeOffset = 0.06f;

    private Coroutine _hitCoroutine;

    public void Setup(Sprite sprite)
    {
        transform.localScale = new Vector3(-1, 1, 1);
        _spriteRenderer.sprite = sprite;
        ResetVisualState();
    }

    public IEnumerator PlayDeathAnimation(float durationOverride = -1f)
    {
        if (_spriteRenderer == null)
            yield break;

        var duration = durationOverride > 0f ? durationOverride : _deathDuration;
        var startColor = _spriteRenderer.color;
        var startScale = transform.localScale;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.85f, t);
            yield return null;
        }
    }

    public void PlayHitAnimation()
    {
        if (_spriteRenderer == null)
            return;

        if (_hitCoroutine != null)
            StopCoroutine(_hitCoroutine);

        _hitCoroutine = StartCoroutine(PlayHitAnimationRoutine());
    }

    private IEnumerator PlayHitAnimationRoutine()
    {
        var startColor = _spriteRenderer.color;
        var startLocalPosition = transform.localPosition;
        var elapsed = 0f;

        while (elapsed < _hitDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / _hitDuration);
            var pulse = Mathf.Sin(t * Mathf.PI);

            _spriteRenderer.color = Color.Lerp(startColor, new Color(1f, 0.5f, 0.5f, startColor.a), pulse);
            transform.localPosition = startLocalPosition + new Vector3(0f, _hitShakeOffset * pulse, 0f);
            yield return null;
        }

        _spriteRenderer.color = startColor;
        transform.localPosition = startLocalPosition;
        _hitCoroutine = null;
    }

    public void ResetVisualState()
    {
        if (_spriteRenderer == null)
            return;

        var color = _spriteRenderer.color;
        _spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
    }
}
