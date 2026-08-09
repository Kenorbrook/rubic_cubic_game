using BattleSystem;
using System.Collections.Generic;
using UnityEngine;

public class WarriorTimingMiniGame : MonoBehaviour
{
    [SerializeField] private RectTransform _laneRect;
    [SerializeField] private RectTransform _barsContainer;
    [SerializeField] private WarriorTimingBar _barPrefab;
    [SerializeField] private RectTransform _targetZone;
    [SerializeField] private UnityEngine.UI.Image _targetZoneImage;

    [Header("Timing")]
    [SerializeField] private float _baseSpeed = 95f;
    [SerializeField] private float _speedIncreasePerLevel = 22.77778f;
    [SerializeField] private float _maxSpeed = 300f;
    [SerializeField] private Vector2 _firstLevelSpawnIntervalRange = new Vector2(4.5f, 6f);
    [SerializeField] private Vector2 _spawnIntervalRange = new Vector2(1.5f, 2.4f);
    [SerializeField, Min(2)] private int _maxDifficultyLevel = 10;
    [SerializeField] private float _rightSwipeStartOffsetFromCenter = 42f;
    [SerializeField] private float _leftSwipeEndOffsetFromCenter = -42f;
    [SerializeField, Min(24f)] private float _targetZoneWidth = 180f;
    [SerializeField] private float _shuffleSpeedMultiplier = 0.05f;

    [Header("Visual")]
    [SerializeField] private Color _defaultColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color _successColor = new Color(0.15f, 0.85f, 0.25f, 1f);
    [SerializeField] private Color _missColor = new Color(0.9f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color _targetZoneIdleColor = new Color(0.91f, 0.67f, 0.29f, 0.18f);
    [SerializeField] private Color _targetZoneActiveColor = new Color(1f, 0.82f, 0.35f, 0.58f);
    [SerializeField] private ArrowSprites _arrowSprites;

    private ICombatService _combatService;
    private readonly List<ActiveBarState> _activeBars = new List<ActiveBarState>(8);
    private float _spawnCooldown;
    private float _speedMultiplier = 1f;
    private float _currentSpeed;
    private Vector2 _currentSpawnIntervalRange;
    private bool _isConfigured;
    private bool _missingRefsLogged;
    private FrontFaceInputController _inputSource;

    private void OnEnable()
    {
        SubscribeInput();
    }

    private void OnDisable()
    {
        UnsubscribeInput();
    }

    private void Update()
    {
        if (!_isConfigured || MenuPanel.IsOpen)
            return;

        var scaledDt = Time.deltaTime * _speedMultiplier;
        if (scaledDt > 0f)
            UpdateBars(scaledDt);

        RefreshTargetZone();

        if (_speedMultiplier <= 0f)
            return;

        _spawnCooldown -= scaledDt;
        if (_spawnCooldown <= 0f)
            SpawnBar();
    }

    public void Configure(ICombatService combatService, FrontFaceInputController inputSource)
    {
        _combatService = combatService;
        UnsubscribeInput();
        _inputSource = inputSource;
        SubscribeInput();
        _speedMultiplier = 1f;
        _currentSpeed = Mathf.Min(_baseSpeed, _maxSpeed);
        _currentSpawnIntervalRange = _firstLevelSpawnIntervalRange;
        SetTargetZoneWidth(_targetZoneWidth);
        ClearBars();
        _missingRefsLogged = false;
        _isConfigured = ValidateReferences();
        if (!_isConfigured)
            return;

        _spawnCooldown = Random.Range(_currentSpawnIntervalRange.x, _currentSpawnIntervalRange.y);
    }

    public void SetShuffleState(bool isShuffling)
    {
        _speedMultiplier = isShuffling ? Mathf.Clamp01(_shuffleSpeedMultiplier) : 1f;
    }

    public void SetBattleLevel(int level)
    {
        var completedLevels = Mathf.Clamp(level - 1, 0, _maxDifficultyLevel - 1);
        var difficulty = completedLevels / (float)(_maxDifficultyLevel - 1);
        _currentSpeed = Mathf.Min(_maxSpeed, _baseSpeed + completedLevels * _speedIncreasePerLevel);
        _currentSpawnIntervalRange = Vector2.Lerp(_firstLevelSpawnIntervalRange, _spawnIntervalRange, difficulty);
        if (_spawnCooldown > 0f)
            _spawnCooldown = Mathf.Min(_spawnCooldown, _currentSpawnIntervalRange.y);
    }

    private void SpawnBar()
    {
        if (!_isConfigured)
            return;

        var parent = _barsContainer != null ? _barsContainer : _laneRect;
        var bar = Instantiate(_barPrefab, parent);
        var rect = bar.Rect;
        if (rect == null)
        {
            Destroy(bar.gameObject);
            return;
        }

        var promptType = (PromptType)Random.Range(0, 6);
        bar.SetPrompt(GetPromptSprite(promptType), _defaultColor);

        var laneRect = _laneRect.rect;
        var barHalfWidth = rect.rect.width * 0.5f;
        var spawnX = laneRect.xMax + barHalfWidth;
        rect.anchoredPosition = new Vector2(spawnX, 0f);

        _activeBars.Add(new ActiveBarState(bar, promptType));
        _spawnCooldown = Random.Range(_currentSpawnIntervalRange.x, _currentSpawnIntervalRange.y);
    }

    private void UpdateBars(float dt)
    {
        if (dt <= 0f || _activeBars.Count == 0)
            return;

        var lane = _laneRect.rect;
        var centerX = (lane.xMin + lane.xMax) * 0.5f;
        GetSwipeWindowX(centerX, out _, out var leftSwipeEndX);
        for (var i = _activeBars.Count - 1; i >= 0; i--)
        {
            var barState = _activeBars[i];
            if (!barState.IsAlive)
            {
                _activeBars.RemoveAt(i);
                continue;
            }

            var pos = barState.Rect.anchoredPosition;
            pos.x -= _currentSpeed * dt;
            barState.Rect.anchoredPosition = pos;

            if (!barState.Resolved && pos.x < leftSwipeEndX)
            {
                barState.MarkMissedCenter();
                barState.Bar.SetFillColor(_missColor);
            }

            var leftEdgeToDestroy = lane.xMin - (barState.Rect.rect.width * 0.5f);
            if (pos.x > leftEdgeToDestroy)
                continue;

            if (barState.ShouldDealDamageOnExit)
                _combatService?.ResolveWarriorTimingMiss();

            Destroy(barState.Bar.gameObject);
            _activeBars.RemoveAt(i);
        }
    }

    private void OnFrontFaceSwipe(SwipeDirection direction)
    {
        if (!_isConfigured || _activeBars.Count == 0)
            return;

        var candidate = FindSwipeCandidate(direction);
        if (candidate == null)
            return;

        candidate.MarkSuccess();
        candidate.Bar.SetFillColor(_successColor);
        RefreshTargetZone();
    }

    public void SetTargetZoneWidth(float width)
    {
        _targetZoneWidth = Mathf.Max(24f, width);
        _rightSwipeStartOffsetFromCenter = _targetZoneWidth * 0.5f;
        _leftSwipeEndOffsetFromCenter = -_targetZoneWidth * 0.5f;
        if (_targetZone != null)
        {
            var size = _targetZone.sizeDelta;
            size.x = _targetZoneWidth;
            _targetZone.sizeDelta = size;
        }
    }

    private void RefreshTargetZone()
    {
        if (_targetZoneImage == null || _laneRect == null)
            return;

        var lane = _laneRect.rect;
        var centerX = (lane.xMin + lane.xMax) * 0.5f;
        GetSwipeWindowX(centerX, out var rightEdge, out var leftEdge);
        var hasUnresolvedPrompt = false;
        for (var i = 0; i < _activeBars.Count; i++)
        {
            var state = _activeBars[i];
            if (!state.IsAlive || state.Resolved)
                continue;
            var x = state.Rect.anchoredPosition.x;
            if (x >= leftEdge && x <= rightEdge)
            {
                hasUnresolvedPrompt = true;
                break;
            }
        }

        _targetZoneImage.color = hasUnresolvedPrompt ? _targetZoneActiveColor : _targetZoneIdleColor;
    }

    private ActiveBarState FindSwipeCandidate(SwipeDirection direction)
    {
        ActiveBarState best = null;
        var bestX = float.MaxValue;
        var lane = _laneRect.rect;
        var centerX = (lane.xMin + lane.xMax) * 0.5f;
        GetSwipeWindowX(centerX, out var rightSwipeStartX, out var leftSwipeEndX);
        for (var i = 0; i < _activeBars.Count; i++)
        {
            var barState = _activeBars[i];
            if (!barState.IsAlive || barState.Resolved)
                continue;

            if (!DirectionMatches(direction, barState.Type))
                continue;

            var x = barState.Rect.anchoredPosition.x;
            if (x > rightSwipeStartX || x < leftSwipeEndX)
                continue;

            if (x >= bestX)
                continue;

            best = barState;
            bestX = x;
        }

        return best;
    }

    private bool DirectionMatches(SwipeDirection swipe, PromptType prompt)
    {
        switch (prompt)
        {
            case PromptType.Up:
                return swipe == SwipeDirection.Up;
            case PromptType.Down:
                return swipe == SwipeDirection.Down;
            case PromptType.Left:
                return swipe == SwipeDirection.Left;
            case PromptType.Right:
                return swipe == SwipeDirection.Right;
            case PromptType.UpOrDown:
                return swipe == SwipeDirection.Up || swipe == SwipeDirection.Down;
            case PromptType.LeftOrRight:
                return swipe == SwipeDirection.Left || swipe == SwipeDirection.Right;
            default:
                return false;
        }
    }

    private Sprite GetPromptSprite(PromptType promptType)
    {
        switch (promptType)
        {
            case PromptType.Up:
                return _arrowSprites.Up;
            case PromptType.Down:
                return _arrowSprites.Down;
            case PromptType.Left:
                return _arrowSprites.Left;
            case PromptType.Right:
                return _arrowSprites.Right;
            case PromptType.UpOrDown:
                return _arrowSprites.UpOrDown;
            case PromptType.LeftOrRight:
                return _arrowSprites.LeftOrRight;
            default:
                return null;
        }
    }

    private void ClearBars()
    {
        for (var i = 0; i < _activeBars.Count; i++)
        {
            var barState = _activeBars[i];
            if (barState != null && barState.Bar != null)
                Destroy(barState.Bar.gameObject);
        }

        _activeBars.Clear();
        RefreshTargetZone();
    }

    private bool ValidateReferences()
    {
        var ok = _laneRect != null &&
                 _barPrefab != null &&
                 _targetZone != null &&
                 _targetZoneImage != null &&
                 _inputSource != null;
        if (ok || _missingRefsLogged)
            return ok;

        Debug.LogWarning("WarriorTimingMiniGame is not configured: lane, target zone, bar prefab and input source are required.");
        _missingRefsLogged = true;
        return false;
    }

    private void SubscribeInput()
    {
        if (_inputSource != null)
            _inputSource.FrontFaceSwipePerformed += OnFrontFaceSwipe;
    }

    private void UnsubscribeInput()
    {
        if (_inputSource != null)
            _inputSource.FrontFaceSwipePerformed -= OnFrontFaceSwipe;
    }

    private void GetSwipeWindowX(float centerX, out float rightSwipeStartX, out float leftSwipeEndX)
    {
        rightSwipeStartX = centerX + _rightSwipeStartOffsetFromCenter;
        leftSwipeEndX = centerX + _leftSwipeEndOffsetFromCenter;
        if (rightSwipeStartX >= leftSwipeEndX)
            return;

        var tmp = rightSwipeStartX;
        rightSwipeStartX = leftSwipeEndX;
        leftSwipeEndX = tmp;
    }

    private enum PromptType
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        UpOrDown = 4,
        LeftOrRight = 5
    }

    [System.Serializable]
    private struct ArrowSprites
    {
        public Sprite Up;
        public Sprite Down;
        public Sprite Left;
        public Sprite Right;
        public Sprite UpOrDown;
        public Sprite LeftOrRight;
    }

    private sealed class ActiveBarState
    {
        public readonly WarriorTimingBar Bar;
        public readonly PromptType Type;

        public bool Resolved { get; private set; }
        public bool Success { get; private set; }
        public bool PassedCenterWithoutSuccess { get; private set; }

        public bool IsAlive => Bar != null && Bar.Rect != null;
        public bool ShouldDealDamageOnExit => PassedCenterWithoutSuccess && !Success;
        public RectTransform Rect => Bar.Rect;

        public ActiveBarState(WarriorTimingBar bar, PromptType type)
        {
            Bar = bar;
            Type = type;
            Resolved = false;
            Success = false;
            PassedCenterWithoutSuccess = false;
        }

        public void MarkSuccess()
        {
            Resolved = true;
            Success = true;
        }

        public void MarkMissedCenter()
        {
            Resolved = true;
            PassedCenterWithoutSuccess = true;
        }
    }
}
