namespace BattleSystem
{
    public class BattleStatistics
    {
        public int DefeatedEnemies { get; private set; }
        public int TurnsMade { get; private set; }
        public int CollectedPatterns { get; private set; }

        public void RegisterEnemyDefeat()
        {
            DefeatedEnemies++;
        }

        public void RegisterTurn()
        {
            TurnsMade++;
        }

        public void RegisterPattern()
        {
            CollectedPatterns++;
        }
    }
}
