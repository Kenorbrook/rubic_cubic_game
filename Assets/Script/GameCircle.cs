using System.Collections;
using BattleSystem;
using UnityEngine;

// TODO: Replace with explicit game states when state machine is introduced.
public class GameCircle : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private GameView _gameView;
    [SerializeField] private BootstrapBattle _battleBootstrap;
    [SerializeField] private WarriorTimingMiniGame _warriorTimingMiniGamePrefab;
    [SerializeField] private RectTransform _warriorMiniGameParent;
    [SerializeField] private float _enemyDeathAnimDelay = 0.35f;
    [SerializeField] private float _playerDeathAnimDelay = 0.4f;

    private IGameSaveService _saveService;
    private ICombatService _combatService;
    private BattleStatistics _stats;
    private WarriorTimingMiniGame _warriorTimingMiniGameInstance;

    private void Awake()
    {
        if (_gameConfig == null || _gameConfig.CombatConfig == null || _gameView == null || _battleBootstrap == null)
        {
            Debug.LogError("GameCircle dependencies are not assigned.");
            enabled = false;
            return;
        }

        _saveService = new SaveManager();
        var saves = _saveService.Load() ?? _saveService.Create(0);
        var playerCharacter = _gameConfig.GetPlayerCharacter(saves.Character);
        if (playerCharacter == null)
        {
            Debug.LogError("Player roster is empty in GameConfig.");
            enabled = false;
            return;
        }

        _stats = new BattleStatistics();

        _gameView.SetupPlayer(playerCharacter.Sprite);
        _gameView.HideBattleResult();

        var enemyProgression = new EnemyLevelProgression(_gameConfig.CombatConfig);
        _combatService = new CombatService(_gameConfig, playerCharacter, enemyProgression);
        _combatService.EnemySpawned += OnEnemySpawned;
        _combatService.EnemyDamaged += OnEnemyDamaged;
        _combatService.EnemyKilled += OnEnemyKilled;
        _combatService.PlayerDamaged += OnPlayerDamaged;
        _combatService.PlayerKilled += OnPlayerKilled;

        _gameView.SetPlayerHp(_combatService.PlayerCurrentHp, _combatService.PlayerMaxHp, 0);

        _battleBootstrap.Initialize(new PatternContext
        {
            CombatService = _combatService,
            PatternApplied = OnPatternApplied,
            PlayerSwiped = OnPlayerSwiped,
            ShuffleStateChanged = OnShuffleStateChanged
        });

        if (playerCharacter.PlayerClass == PlayerClass.Warrior && _warriorTimingMiniGamePrefab != null)
        {
            var inputSource = _battleBootstrap.FrontFaceInputController;
            if (inputSource == null)
                Debug.LogError("BootstrapBattle.FrontFaceInputController is not assigned.");

            var parent = _warriorMiniGameParent != null ? _warriorMiniGameParent : _gameView.transform as RectTransform;
            _warriorTimingMiniGameInstance = Instantiate(_warriorTimingMiniGamePrefab, parent);
            _warriorTimingMiniGameInstance.Configure(_combatService, inputSource);
        }

        OnEnemySpawned(_combatService.CurrentEnemy);
    }

    private void OnDestroy()
    {
        if (_combatService == null)
            return;

        _combatService.EnemySpawned -= OnEnemySpawned;
        _combatService.EnemyDamaged -= OnEnemyDamaged;
        _combatService.EnemyKilled -= OnEnemyKilled;
        _combatService.PlayerDamaged -= OnPlayerDamaged;
        _combatService.PlayerKilled -= OnPlayerKilled;
    }

    private void OnEnemySpawned(EnemyInstance enemy)
    {
        _warriorTimingMiniGameInstance?.SetBattleLevel(enemy.Level);
        _gameView.SetupEnemy(enemy.Character.Sprite);
        _gameView.SetEnemyHp(enemy.CurrentHp, enemy.MaxHp, enemy.Level);
        _gameView.ClearEnemyDamage();
        UpdateEnemyAttackCountdown();
    }

    private void OnEnemyDamaged(EnemyInstance enemy, int damage)
    {
        _gameView.SetEnemyHp(enemy.CurrentHp, enemy.MaxHp, enemy.Level);
        if (damage > 0)
        {
            _gameView.PlayPlayerAttackFeedback();
            _gameView.PlayEnemyHitFeedback();
        }

        UpdateEnemyAttackCountdown();
    }

    private void OnEnemyKilled(EnemyInstance enemy)
    {
        _stats.RegisterEnemyDefeat();
        _gameView.SetEnemyAttackCountdown(0);
        StartCoroutine(HandleEnemyDeath());
    }

    private void OnPlayerDamaged(int currentHp, int maxHp, int shield, int enemyDamage)
    {
        _gameView.SetPlayerHp(currentHp, maxHp, shield);
        if (enemyDamage > 0)
        {
            _gameView.ShowEnemyDamage(enemyDamage);
            _gameView.PlayEnemyAttackFeedback();
            _gameView.PlayPlayerHitFeedback();
        }
        else
            _gameView.ClearEnemyDamage();

        UpdateEnemyAttackCountdown();
    }

    private void OnPlayerKilled()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private void OnPatternApplied(Pattern _)
    {
        _stats.RegisterPattern();
    }

    private void OnPlayerSwiped()
    {
        if (_combatService == null || _combatService.IsPlayerDead)
            return;

        _stats.RegisterTurn();
        _combatService.RegisterPlayerSwipe();
        UpdateEnemyAttackCountdown();
    }

    private void UpdateEnemyAttackCountdown()
    {
        if (_combatService == null)
            return;

        if (_combatService.PlayerCharacter != null &&
            _combatService.PlayerCharacter.PlayerClass == PlayerClass.Warrior)
        {
            _gameView.SetEnemyAttackCountdown(0);
            return;
        }

        _gameView.SetEnemyAttackCountdown(_combatService.TurnsUntilEnemyAttack);
    }

    private void OnShuffleStateChanged(bool isShuffling)
    {
        _warriorTimingMiniGameInstance?.SetShuffleState(isShuffling);
    }

    private IEnumerator HandleEnemyDeath()
    {
        if (_gameView.EnemyView != null)
            yield return _gameView.EnemyView.PlayDeathAnimation(_enemyDeathAnimDelay);
        else
            yield return new WaitForSeconds(_enemyDeathAnimDelay);

        _combatService.SpawnPendingEnemy();
    }

    private IEnumerator HandlePlayerDeath()
    {
        if (_gameView.PlayerView != null)
            yield return _gameView.PlayerView.PlayDeathAnimation(_playerDeathAnimDelay);
        else
            yield return new WaitForSeconds(_playerDeathAnimDelay);

        _gameView.ShowBattleResult(_stats);
    }
}
