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
    public class Comp_EquippableAbilities : DW_WeaponComp , IMeleeAttacked
    {
        //public List<Ability> Abilities = new List<Ability>();
        public new CompProperties_EquippableAbilities Props => props as CompProperties_EquippableAbilities;

        public Comp_RechargeByAttack RechargeComp => parent.TryGetComp<Comp_RechargeByAttack>();

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
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
            base.Notify_Unequipped(pawn);
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
            RechargeAbilities(this.PawnOwner);
        }

        // 不需要保存wielder，会在Notify_Equipped时重新设置
        // 避免与其他DW_WeaponComp子类的"wielder"键名冲突
        public override void PostExposeData()
        {
            // 故意不调用 base.PostExposeData() 来避免重复保存 wielder
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
