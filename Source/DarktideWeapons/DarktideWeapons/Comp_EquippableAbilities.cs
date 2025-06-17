using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_EquippableAbilities : CompEquippable
    {
        //public List<Ability> Abilities = new List<Ability>();
        public CompProperties_EquippableAbilities Props => props as CompProperties_EquippableAbilities;

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
