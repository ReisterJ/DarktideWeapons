using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using DarktideWeapons.MeleeComps;

namespace DarktideWeapons
{
    public class CompAbilityEffect_SwitchMeleeHediff : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ChangeHediffToApply();
        }

        public void ChangeHediffToApply()
        {
            Pawn caster = parent?.pawn;
            if (caster == null || caster.equipment == null) return;

            if (!(caster.equipment.Primary is DW_Equipment weapon)) return;
            var comp = weapon.GetComp<Comp_SwitchMeleeHediff>();
            if (comp == null)
            {
                Log.Warning("CompAbilityEffect_SwitchMeleeHediff: The equipped weapon does not have a Comp_SwitchMeleeHediff. Cannot switch hediff.");
                return;
            }
            if (!comp.TrySwitchHediff())
            {
                Log.Error("CompAbilityEffect_SwitchMeleeHediff: Failed to switch hediff.");
            }
        }
    }

    public class CompProperties_AbilityEffect_SwitchMeleeHediff : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_SwitchMeleeHediff()
        {
            this.compClass = typeof(CompAbilityEffect_SwitchMeleeHediff);
        }
    }
}
