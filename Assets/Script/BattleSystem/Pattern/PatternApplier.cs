namespace BattleSystem
{
    public class PatternApplier : IPatternApplier
    {

        private readonly PatternContext _context;

        public PatternApplier(PatternContext context)
        {
            _context = context;
        }

        public void Apply(Pattern pattern)
        {
            _context.PatternApplied?.Invoke(pattern);
            foreach (var effect in pattern.effects)
                effect.Apply(_context);
        }
    }
}
