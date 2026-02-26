using DarktideWeapons.MeleeComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_EquippableAbilities : CompEquippable , IMeleeAttacked
    {
        //public List<Ability> Abilities = new List<Ability>();
        public CompProperties_EquippableAbilities Props => props as CompProperties_EquippableAbilities;

        public Comp_RechargeByAttack RechargeComp => parent.TryGetComp<Comp_RechargeByAttack>();
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            /*
            if (base.Holder != null)
            {
                foreach (AbilityDef abilityDef in Props.abilityDefList)
                {
                    Ability ability = AbilityUtility.MakeAbility(abilityDef, base.Holder);
                    ability.pawn = base.Holder;
                    ability.verb.caster = base.Holder;
                    Abilities.Add(ability);
                }
            }*/
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            foreach (AbilityDef abilityDef in Props.abilityDefList)
            {
                pawn.abilities.GainAbility(abilityDef);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            foreach (AbilityDef abilityDef in Props.abilityDefList)
            {
                pawn.abilities.RemoveAbility(abilityDef);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            
        }


        public void RechargeAbilities(Pawn pawn)
        {
            if (RechargeComp == null) return;
            if (RechargeComp.IsFullyCharged())
            {
                foreach (AbilityDef abilityDef in Props.abilityDefList)
                {
                    Ability ability = pawn.abilities.GetAbility(abilityDef);
                    if(ability != null)
                    {
                        if (ability.UsesCharges && ability.RemainingCharges < ability.maxCharges)
                        {
                            ability.RemainingCharges++;
                            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "+1 charge", UnityEngine.Color.cyan, 1.5f);
                        }
                    }
                }
            }
        }
        public void PostMeleeAttacked(MeleeAttackData data)
        {
            RechargeAbilities(this.Holder);
        }

       
    }

    public class CompProperties_EquippableAbilities : CompProperties
    {
        public List<AbilityDef> abilityDefList = new List<AbilityDef>();

        public CompProperties_EquippableAbilities()
        {
            compClass = typeof(Comp_EquippableAbilities);
        }
    }
}
