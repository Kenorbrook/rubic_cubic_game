using BattleSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    [SerializeField] private Button _pause;
    [SerializeField] private MenuPanel _pausePanel;
    [SerializeField] private CharacterView _player;
    [SerializeField] private CharacterView _enemy;

    [Header("HUD")]
    [SerializeField] private TMP_Text _playerHpText;
    [SerializeField] private TMP_Text _playerShield;
    [SerializeField] private TMP_Text _enemyHpText;
    [SerializeField] private TMP_Text _enemyDamageText;
    [SerializeField] private Image _playerHpFill;
    [SerializeField] private Image _enemyHpFill;
    [SerializeField] private GameObject _battleEndPanel;
    [SerializeField] private TMP_Text _battleEndTitleText;
    [SerializeField] private TMP_Text _battleEndStatsText;
    private string _enemyHpBaseText = string.Empty;
    private int _cachedTurnsUntilEnemyAttack;
    private void Awake()
    {
        if (_pause != null)
            _pause.onClick.AddListener(PauseGame);
    }

    public CharacterView PlayerView => _player;
    public CharacterView EnemyView => _enemy;

    public void SetupPlayer(Sprite sprite)
    {
        _player.Setup(sprite, true);
    }

    public void SetupEnemy(Sprite sprite)
    {
        _enemy.Setup(sprite, false);
    }

    public void SetPlayerHp(int currentHp, int maxHp, int shield)
    {
        if (_playerHpText != null)
            _playerHpText.text = $"{currentHp} / {maxHp}";
        if (_playerShield != null)
            _playerShield.text = shield > 0 ? $"ЩИТ  {shield}" : string.Empty;
        if (_playerHpFill != null)
            _playerHpFill.fillAmount = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
    }

    public void SetEnemyHp(int currentHp, int maxHp, int level)
    {
        _enemyHpBaseText = $"УР. {level}     {currentHp} / {maxHp}";
        RefreshEnemyHpText();
        if (_enemyHpFill != null)
            _enemyHpFill.fillAmount = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
    }

    public void ShowEnemyDamage(int damage)
    {
        if (_enemyDamageText != null)
            _enemyDamageText.text = $"УРОН  {damage}";
    }

    public void ClearEnemyDamage()
    {
        if (_enemyDamageText != null)
            _enemyDamageText.text = string.Empty;
    }

    public void SetEnemyAttackCountdown(int turnsUntilAttack)
    {
        _cachedTurnsUntilEnemyAttack = turnsUntilAttack;
        RefreshEnemyHpText();
    }

    public void PlayPlayerHitFeedback()
    {
        if (_player != null)
            _player.PlayHitAnimation();
    }

    public void PlayPlayerAttackFeedback()
    {
        if (_player != null)
            _player.PlayAttackAnimation();
    }

    public void PlayEnemyAttackFeedback()
    {
        if (_enemy != null)
            _enemy.PlayAttackAnimation();
    }

    public void PlayEnemyHitFeedback()
    {
        if (_enemy != null)
            _enemy.PlayHitAnimation();
    }

    public void ShowBattleResult(BattleStatistics stats)
    {
        if (_battleEndPanel == null)
            return;
        
        if (_pausePanel != null)
            _pausePanel.gameObject.SetActive(false);
        if (_pause != null)
            _pause.interactable = false;

        if (_battleEndTitleText != null)
            _battleEndTitleText.text = "Вы проиграли";

        if (_battleEndStatsText != null)
        {
            _battleEndStatsText.text =
                $"Побеждено врагов: {stats.DefeatedEnemies}\n" +
                $"Сделано ходов: {stats.TurnsMade}\n" +
                $"Собрано паттернов: {stats.CollectedPatterns}";
        }

        _battleEndPanel.SetActive(true);
    }

    public void HideBattleResult()
    {
        if (_battleEndPanel != null)
            _battleEndPanel.SetActive(false);
        if (_pause != null)
            _pause.interactable = true;
    }

    private void PauseGame()
    {
        if (_battleEndPanel != null && _battleEndPanel.activeSelf)
            return;

        _pausePanel.Open();
        // TODO Pause gameplay.
    }

    private void RefreshEnemyHpText()
    {
        if (_enemyHpText == null)
            return;


        _enemyHpText.text = _cachedTurnsUntilEnemyAttack > 0
            ? $"{_enemyHpBaseText}     УДАР: {_cachedTurnsUntilEnemyAttack}"
            : _enemyHpBaseText;
    }
}
