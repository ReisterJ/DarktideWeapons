using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{

    public class CompAbilityEffect_EmergencyCooling: CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            EmergencyCooling();
            base.Apply(target, dest);
        }
        Comp_DarktidePlasma CompPlasma => this.parent.pawn.equipment.Primary.TryGetComp<Comp_DarktidePlasma>();
        public void EmergencyCooling()
        {
            
            CompPlasma?.ForcedCooling();

        }
        public override bool GizmoDisabled(out string reason)
        {
            reason = "";
            if (CompPlasma == null)
            {
                reason = "CompPlasmaNotExist".Translate();
                return true;
            }
            if (CompPlasma.heat <= float.Epsilon)
            {
                reason = "PlasmaZeroHeat".Translate();
                return true;
            }
            return false;
        }
    }
    public class CompProperties_AbilityEffect_EmergencyCooling : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_EmergencyCooling()
        {
            this.compClass = typeof(CompAbilityEffect_EmergencyCooling);
        }
    }
}
