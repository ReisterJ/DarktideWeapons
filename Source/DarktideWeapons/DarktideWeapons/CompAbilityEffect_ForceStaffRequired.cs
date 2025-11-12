using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_ForceStaffRequired : CompAbilityEffect
    {
        
        Comp_DarktideForceStaff ForceStaff => parent.pawn.equipment.Primary.TryGetComp<Comp_DarktideForceStaff>();

        public new CompProperties_ForceStaffRequired Props
            {
                get
                {
                    return (CompProperties_ForceStaffRequired)props;
                }
            }
        public override bool CanCast
        {
            get
            {
                if (ForceStaff == null || ForceStaff.parent.def != Props.requiredStaff)
                {
                    return false;
                }
                return base.CanCast;
            }
        }
        public override bool GizmoDisabled(out string reason)
        {
            if (ForceStaff == null)
            {
                reason = "StaffNotEquipped".Translate();
                return true;
            }
            if(ForceStaff.parent.def != Props.requiredStaff)
            {
                reason = "RequiredStaffNotEquipped".Translate() + ".\n" + Props.requiredStaff.LabelCap;
                return true;
            }
            return base.GizmoDisabled(out reason);
        }
    }

    public class CompProperties_ForceStaffRequired : CompProperties_AbilityEffect
    {
        public CompProperties_ForceStaffRequired()
        {
            this.compClass = typeof(CompAbilityEffect_ForceStaffRequired);
        }
        public ThingDef requiredStaff;

    }
}
