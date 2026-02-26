namespace BattleSystem
{
    public class EnemyInstance
    {
        public EnemyInstance(EnemyCharacter character, int level, int maxHp, int attack)
        {
            Character = character;
            Level = level;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attack = attack;
        }

        public EnemyCharacter Character { get; }
        public int Level { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int Attack { get; }

        public bool IsDead => CurrentHp <= 0;

        public void TakeDamage(int damage)
        {
            if (damage <= 0)
                return;

            CurrentHp -= damage;
            if (CurrentHp < 0)
                CurrentHp = 0;
        }
    }
}
