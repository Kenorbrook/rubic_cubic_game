using System.Collections;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _deathDuration = 0.35f;
    [SerializeField] private float _hitDuration = 0.12f;
    [SerializeField] private float _hitShakeOffset = 0.06f;
    [SerializeField] private float _targetWorldHeight = 3.25f;

    private Coroutine _hitCoroutine;
    private Coroutine _attackCoroutine;
    private Vector3 _restScale;
    private bool _facesRight;

    public void Setup(Sprite sprite)
    {
        Setup(sprite, true);
    }

    public void Setup(Sprite sprite, bool faceRight)
    {
        _spriteRenderer.sprite = sprite;
        var spriteHeight = sprite != null ? sprite.bounds.size.y : 1f;
        var scale = spriteHeight > 0.001f ? _targetWorldHeight / spriteHeight : 1f;
        _facesRight = faceRight;
        transform.localScale = new Vector3(faceRight ? scale : -scale, scale, 1f);
        _restScale = transform.localScale;
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

    public void PlayAttackAnimation()
    {
        if (_spriteRenderer == null)
            return;
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(PlayAttackAnimationRoutine());
    }

    private IEnumerator PlayAttackAnimationRoutine()
    {
        var origin = transform.localPosition;
        var originRotation = transform.localRotation;
        var direction = _facesRight ? 1f : -1f;
        const float anticipation = 0.10f;
        const float strike = 0.13f;
        const float recovery = 0.16f;

        yield return AnimatePose(anticipation, t =>
        {
            transform.localPosition = origin + Vector3.right * (-direction * 0.08f * t);
            transform.localRotation = originRotation * Quaternion.Euler(0f, 0f, direction * 4f * t);
            transform.localScale = Vector3.Scale(_restScale, new Vector3(0.94f, 1.06f, 1f));
        });
        yield return AnimatePose(strike, t =>
        {
            var eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = origin + Vector3.right * (direction * 0.34f * eased);
            transform.localRotation = originRotation * Quaternion.Euler(0f, 0f, -direction * 7f * eased);
            transform.localScale = Vector3.Scale(_restScale, new Vector3(1.07f, 0.95f, 1f));
        });
        yield return AnimatePose(recovery, t =>
        {
            transform.localPosition = Vector3.Lerp(origin + Vector3.right * direction * 0.34f, origin, Mathf.SmoothStep(0f, 1f, t));
            transform.localRotation = Quaternion.Slerp(originRotation * Quaternion.Euler(0f, 0f, -direction * 7f), originRotation, t);
            transform.localScale = Vector3.Lerp(Vector3.Scale(_restScale, new Vector3(1.07f, 0.95f, 1f)), _restScale, t);
        });
        transform.localPosition = origin;
        transform.localRotation = originRotation;
        transform.localScale = _restScale;
        _attackCoroutine = null;
    }

    private static IEnumerator AnimatePose(float duration, System.Action<float> apply)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            apply(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
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
            var recoil = (_facesRight ? -1f : 1f) * _hitShakeOffset * 2.2f * pulse;
            transform.localPosition = startLocalPosition + new Vector3(recoil, _hitShakeOffset * 0.35f * pulse, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, (_facesRight ? 1f : -1f) * 5f * pulse);
            yield return null;
        }

        _spriteRenderer.color = startColor;
        transform.localPosition = startLocalPosition;
        transform.localRotation = Quaternion.identity;
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
