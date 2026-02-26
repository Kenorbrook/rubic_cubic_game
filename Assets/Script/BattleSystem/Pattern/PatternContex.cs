namespace BattleSystem
{
    public class PatternContext
    {
        public ICombatService CombatService;
        public System.Action<Pattern> PatternApplied;
        public System.Action PlayerSwiped;
        public System.Action<bool> ShuffleStateChanged;
    }
}
