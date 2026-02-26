using System;

namespace BattleSystem
{
    public interface ICombatService
    {
        EnemyInstance CurrentEnemy { get; }
        int PlayerCurrentHp { get; }
        int PlayerMaxHp { get; }
        PlayerCharacter PlayerCharacter { get; }
        bool IsPlayerDead { get; }
        bool IsAwaitingEnemySpawn { get; }
        int TurnsUntilEnemyAttack { get; }

        event Action<EnemyInstance> EnemySpawned;
        event Action<EnemyInstance, int> EnemyDamaged;
        event Action<EnemyInstance> EnemyKilled;
        event Action<int, int, int, int> PlayerDamaged;
        event Action PlayerKilled;
        event Action TurnResolved;
        public void AddStep(int amount);
        void ApplyDamageToEnemy(int rawDamage);
        void HealPlayer(int amount);
        public void AddMaxHp(float percent);
        void AddPlayerShield(int amount);
        void RegisterPlayerSwipe();
        void ResolveWarriorTimingMiss();
        void SpawnPendingEnemy();
    }
}
