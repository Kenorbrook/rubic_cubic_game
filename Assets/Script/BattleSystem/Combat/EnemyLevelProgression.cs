using UnityEngine;

namespace BattleSystem
{
    public class EnemyLevelProgression
    {
        private readonly CombatConfig _combatConfig;
        private int _currentLevel = 1;

        public EnemyLevelProgression(CombatConfig combatConfig)
        {
            _combatConfig = combatConfig;
        }

        public int CurrentLevel => _currentLevel;

        public EnemyLevelMultiplier GetCurrentMultiplier()
        {
            return _combatConfig.GetEnemyMultiplierForLevel(_currentLevel);
        }

        public void AdvanceOnKill()
        {
            _currentLevel = Mathf.Clamp(_currentLevel + 1, 1, 100);
        }
    }
}
