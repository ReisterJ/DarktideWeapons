using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_ChargeRequired : CompAbilityEffect
    {
        Comp_DWChargeWeapon CompCharge => this.parent.pawn.equipment.Primary.TryGetComp<Comp_DWChargeWeapon>();

        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            if(CompCharge != null && CompCharge.isCharged == true) 
            {
                return false;
            }
            reason = "DW_AbilityChargeRequired".Translate();
            return true;
        }
    }
}
