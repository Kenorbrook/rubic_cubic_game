using BattleSystem;

namespace Effects
{
    [System.Serializable]
    public abstract class Effect
    {
        public abstract void Apply(PatternContext context);
    }
}