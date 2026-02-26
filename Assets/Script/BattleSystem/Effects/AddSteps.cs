using BattleSystem;

namespace Effects
{
    [System.Serializable]
    public class AddSteps : Effect
    {
        public int step;
        public override void Apply(PatternContext context)
        {
            context.CombatService.AddStep(step);
        }
    }
}