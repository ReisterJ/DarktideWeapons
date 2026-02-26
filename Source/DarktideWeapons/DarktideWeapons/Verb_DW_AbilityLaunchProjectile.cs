using RimWorld;
using Verse;

namespace DarktideWeapons
{
    /// <summary>
    /// A verb that launches a projectile like Verb_LaunchProjectileStatic,
    /// but also calls Ability.Activate() so that charges are properly decremented.
    /// Use this as the verbClass in AbilityDefs that need charges + projectile launching.
    /// </summary>
    public class Verb_DW_AbilityLaunchProjectile : Verb_LaunchProjectileStatic
    {
        protected override bool TryCastShot()
        {
            bool result = base.TryCastShot();
            if (result && verbTracker?.directOwner is Ability ability)
            {
                if (ability.def.charges > 0)
                {
                    ability.RemainingCharges--;
                }
                if (ability.def.cooldownTicksRange.max > 0)
                {
                    ability.StartCooldown(ability.def.cooldownTicksRange.max);
                }
            }
            return result;
        }
    }


}
