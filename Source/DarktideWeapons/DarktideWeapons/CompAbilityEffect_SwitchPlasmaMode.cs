using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_SwitchPlasmaMode : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            SwitchMode();
            base.Apply(target, dest);
            
        }

        public void SwitchMode()
        {
            Comp_DarktidePlasma compPlasma = parent.pawn.equipment.Primary.TryGetComp<Comp_DarktidePlasma>();
            compPlasma?.SwitchMode();   
        }
    }
    public class CompProperties_AbilityEffect_SwitchPlasmaMode : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_SwitchPlasmaMode()
        {
            this.compClass = typeof(CompAbilityEffect_SwitchPlasmaMode);
        }
    }

    public class CompAbilityEffect_PlasmaChargedShot : CompAbilityEffect
    {
        Comp_DarktidePlasma CompPlasma => parent.pawn.equipment.Primary.TryGetComp<Comp_DarktidePlasma>();
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            ChargeWeapon();
            base.Apply(target, dest);
        }

        public void ChargeWeapon()
        {
            
            if(CompPlasma != null)
            {
                CompPlasma.plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Charged;
            }
           
        }
        public override bool GizmoDisabled(out string reason)
        {
            reason = "";
            if(CompPlasma == null)
            {
                reason = "CompPlasmaNotExist".Translate();
                return true;
            }
            if (CompPlasma.SafeMode)
            {
                reason = "PlasmaSafeModeOn".Translate();
                return true;
            }
            return false;
        }
    }

    public class CompProperties_AbilityEffect_PlasmaChargedShot : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_PlasmaChargedShot()
        {
            this.compClass = typeof(CompAbilityEffect_PlasmaChargedShot);
        }
    }

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
