using BattleSystem;

namespace Effects
{
    
    [System.Serializable]
    public class HealEffect : Effect
    {
        public int value;
        public override void Apply(PatternContext context)
        {
            context.CombatService?.HealPlayer(value);
        }
    }
}
