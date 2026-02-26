using BattleSystem;
using UnityEngine;

namespace Effects
{
    [System.Serializable]
    public class AddMaxHp : Effect
    {
        [Range(0,1)]
        public float percent;
        public override void Apply(PatternContext context)
        {
            context.CombatService.AddMaxHp(percent);
        }
    }
}