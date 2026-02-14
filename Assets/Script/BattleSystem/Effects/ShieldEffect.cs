using BattleSystem;
using UnityEngine;


namespace Effects
{
    [System.Serializable]
    public class ShieldEffect : Effect
    {
        public int amount;

        public override void Apply(PatternContext context)
        {
            //context.player.AddShield(amount);
        }
    }
}