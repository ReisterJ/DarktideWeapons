using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_ChargeWeapon : CompAbilityEffect
    {
        Comp_DWChargeWeapon CompCharge => this.parent.pawn.equipment.Primary.TryGetComp<Comp_DWChargeWeapon>();
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            CompCharge.ChargeWeapon();
        }
    }

    public class CompProperties_AbilityEffect_ChargeWeapon : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_ChargeWeapon()
        {
            this.compClass = typeof(CompAbilityEffect_ChargeWeapon);
        }
    }
}
