using BattleSystem;
using UnityEngine;

namespace Effects
{

    [System.Serializable]
    public class DamageEffect : Effect
    {
        public int value;

        public override void Apply(PatternContext context)
        {
            //context.enemy.TakeDamage(value);
        }
    }
}