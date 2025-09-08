using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class CompAbilityEffect_SwitchFireMode: CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ChangeVerb();
        }

        public void ChangeVerb()
        {
            Pawn caster = parent.pawn;
            if (caster == null || caster.equipment == null) return;
            if(caster.equipment.Primary is DW_Equipment weapon)
            {
                //Log.Message("Switching fire mode for " + weapon.LabelCap);
                //Log.Message("Current verb: " + weapon.GetComp<CompEquippable>().PrimaryVerb.ToString());
                weapon.ChangeVerb();
                //Log.Message("Change verb: " + weapon.GetComp<CompEquippable>().PrimaryVerb.ToString());
            }
        }
    }
    public class CompProperties_AbilityEffect_SwitchFireMode : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_SwitchFireMode()
        {
            this.compClass = typeof(CompAbilityEffect_SwitchFireMode);
        }
    }
}
