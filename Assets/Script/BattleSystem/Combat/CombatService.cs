using System;
using UnityEngine;

namespace BattleSystem
{
    public class CombatService : ICombatService
    {
        private const int MageEnemyAttackBaseInterval = 2;

        private readonly GameConfig _gameConfig;
        private readonly CombatConfig _combatConfig;
        private readonly EnemyLevelProgression _enemyLevelProgression;
        private readonly PlayerCharacter _playerCharacter;
        private bool _isPlayerDead;
        private bool _isAwaitingEnemySpawn;
        private int _playerShield;
        private int _playerMaxHP;
        private int _turnsAgainstCurrentEnemy;

        public CombatService(
            GameConfig gameConfig,
            PlayerCharacter playerCharacter,
            EnemyLevelProgression enemyLevelProgression)
        {
            _gameConfig = gameConfig;
            _combatConfig = gameConfig.CombatConfig;
            _enemyLevelProgression = enemyLevelProgression;
            _playerCharacter = playerCharacter;

            _playerMaxHP = Mathf.Max(1, playerCharacter.BaseHp);
            PlayerCurrentHp = PlayerMaxHp;

            SpawnEnemy();
        }

        public EnemyInstance CurrentEnemy { get; private set; }
        public int PlayerCurrentHp { get; private set; }
        public int PlayerMaxHp => _playerMaxHP;
        public PlayerCharacter PlayerCharacter => _playerCharacter;
        public bool IsPlayerDead => _isPlayerDead;
        public bool IsAwaitingEnemySpawn => _isAwaitingEnemySpawn;
        public int TurnsUntilEnemyAttack
        {
            get
            {
                if (_isPlayerDead || CurrentEnemy == null || _isAwaitingEnemySpawn || CurrentEnemy.IsDead)
                    return 0;
                if (_playerCharacter.PlayerClass == PlayerClass.Warrior)
                    return 0;

                var attackInterval = GetEnemyAttackInterval();
                var completedSwipesInCycle = _turnsAgainstCurrentEnemy % attackInterval;
                return completedSwipesInCycle == 0
                    ? attackInterval
                    : attackInterval - completedSwipesInCycle;
            }
        }

        public event Action<EnemyInstance> EnemySpawned;
        public event Action<EnemyInstance, int> EnemyDamaged;
        public event Action<EnemyInstance> EnemyKilled;
        public event Action<int, int, int, int> PlayerDamaged;
        public event Action PlayerKilled;
        public event Action TurnResolved;

        public void ApplyDamageToEnemy(int rawDamage)
        {
            if (_isPlayerDead || CurrentEnemy == null || CurrentEnemy.IsDead)
                return;

            var classModifier = _combatConfig.GetClassModifier(_playerCharacter.PlayerClass);
            var playerDamage = Mathf.Max(0, Mathf.RoundToInt(rawDamage * classModifier.OutgoingDamageMultiplier));

            CurrentEnemy.TakeDamage(playerDamage);
            EnemyDamaged?.Invoke(CurrentEnemy, playerDamage);

            if (!CurrentEnemy.IsDead)
                return;

            EnemyKilled?.Invoke(CurrentEnemy);
            _enemyLevelProgression.AdvanceOnKill();
            _isAwaitingEnemySpawn = true;
        }

        public void HealPlayer(int amount)
        {
            if (_isPlayerDead || amount <= 0)
                return;

            PlayerCurrentHp += amount;
            if (PlayerCurrentHp > PlayerMaxHp)
                PlayerCurrentHp = PlayerMaxHp;
        }

        public void AddPlayerShield(int amount)
        {
            if (_isPlayerDead || amount <= 0)
                return;

            _playerShield += amount;
        }
        
        public void AddMaxHp(float percent)
        {
            if (_isPlayerDead || percent <= 0)
                return;

            _playerMaxHP += (int)(_playerMaxHP*percent);
        }
        public void AddStep(int amount)
        {
            if (_isPlayerDead || amount <= 0)
                return;

            _turnsAgainstCurrentEnemy=Mathf.Max(0, _turnsAgainstCurrentEnemy-amount);
        }

        public void RegisterPlayerSwipe()
        {
            if (_isPlayerDead || CurrentEnemy == null || CurrentEnemy.IsDead || _isAwaitingEnemySpawn)
                return;

            if (_playerCharacter.PlayerClass == PlayerClass.Warrior)
            {
                PlayerDamaged?.Invoke(PlayerCurrentHp, PlayerMaxHp, _playerShield, 0);
                TurnResolved?.Invoke();
                return;
            }

            _turnsAgainstCurrentEnemy++;
            var attackInterval = GetEnemyAttackInterval();
            if (_turnsAgainstCurrentEnemy % attackInterval != 0)
            {
                PlayerDamaged?.Invoke(PlayerCurrentHp, PlayerMaxHp,_playerShield, 0 );
                TurnResolved?.Invoke();
                return;
            }

            var classModifier = _combatConfig.GetClassModifier(_playerCharacter.PlayerClass);
            var enemyDamage = Mathf.Max(0, Mathf.RoundToInt(CurrentEnemy.Attack * classModifier.IncomingDamageMultiplier));
            var blockedByShield = Mathf.Min(_playerShield, enemyDamage);
            _playerShield -= blockedByShield;
            var finalDamage = enemyDamage - blockedByShield;

            PlayerCurrentHp -= finalDamage;
            if (PlayerCurrentHp < 0)
                PlayerCurrentHp = 0;

            PlayerDamaged?.Invoke(PlayerCurrentHp, PlayerMaxHp,_playerShield, finalDamage);
            if (PlayerCurrentHp <= 0)
            {
                _isPlayerDead = true;
                PlayerKilled?.Invoke();
            }

            TurnResolved?.Invoke();
        }

        public void ResolveWarriorTimingMiss()
        {
            if (_playerCharacter.PlayerClass != PlayerClass.Warrior)
                return;

            if (_isPlayerDead || CurrentEnemy == null || CurrentEnemy.IsDead || _isAwaitingEnemySpawn)
                return;

            var classModifier = _combatConfig.GetClassModifier(_playerCharacter.PlayerClass);
            var enemyDamage = Mathf.Max(0, Mathf.RoundToInt(CurrentEnemy.Attack * classModifier.IncomingDamageMultiplier));
            var blockedByShield = Mathf.Min(_playerShield, enemyDamage);
            _playerShield -= blockedByShield;
            var finalDamage = enemyDamage - blockedByShield;

            PlayerCurrentHp -= finalDamage;
            if (PlayerCurrentHp < 0)
                PlayerCurrentHp = 0;

            PlayerDamaged?.Invoke(PlayerCurrentHp, PlayerMaxHp, _playerShield, finalDamage);
            if (PlayerCurrentHp <= 0)
            {
                _isPlayerDead = true;
                PlayerKilled?.Invoke();
            }
        }

        public void SpawnPendingEnemy()
        {
            if (!_isAwaitingEnemySpawn || _isPlayerDead)
                return;

            _isAwaitingEnemySpawn = false;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            var enemyCharacter = _gameConfig.GetRandomEnemy();
            if (enemyCharacter == null)
                throw new InvalidOperationException("Enemy roster is empty in GameConfig.");

            var level = _enemyLevelProgression.CurrentLevel;
            var multiplier = _enemyLevelProgression.GetCurrentMultiplier();

            var maxHp = Mathf.Max(1, Mathf.RoundToInt(enemyCharacter.BaseHp * multiplier.HpMultiplier));
            var attack = Mathf.Max(1, Mathf.RoundToInt(enemyCharacter.BaseAttack * multiplier.AttackMultiplier));
            CurrentEnemy = new EnemyInstance(enemyCharacter, level, maxHp, attack);
            _turnsAgainstCurrentEnemy = 0;

            EnemySpawned?.Invoke(CurrentEnemy);
        }

        private int GetEnemyAttackInterval()
        {
            var enemyFrequency = Mathf.Max(1, CurrentEnemy.Character.AttackFrequencyTurns);
            var playerModifier = _playerCharacter.PlayerClass == PlayerClass.Mage
                ? MageEnemyAttackBaseInterval
                : 1;
            return enemyFrequency * playerModifier;
        }
    }
}
