using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace DarktideWeapons
{
    public class Comp_CounterAttack : ThingComp
    {
        protected float CounterAttackArmorPenetrationBase;

        protected float CounterAttackStabChance;

        protected float CounterAttackStabArmorPenetration;

        public float CounterAttackChance ;

        public float CounterAttackDamage ;

        public float CounterAttackStabDamage ;

        public float counterAttackChanceCurrent;
        public CompProperties_CounterAttack Props
        {
            get
            {
                return (CompProperties_CounterAttack)this.props;
            }
           
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            CounterAttackChance = Props.CounterAttackChance;
            CounterAttackDamage = Props.CounterAttackDamage;
            CounterAttackArmorPenetrationBase = Props.CounterAttackArmorPenetrationBase;
            CounterAttackStabChance = Props.CounterAttackStabChance;
            CounterAttackStabArmorPenetration = Props.CounterAttackStabArmorPenetration;
            CounterAttackStabDamage = Props.CounterAttackStabDamage;
            counterAttackChanceCurrent = CounterAttackChance;
        }

        public bool CanCounterAttack(Pawn pawn, DamageInfo dinfo)
        {
            if (pawn != null)
            {
                if (!pawn.DeadOrDowned && pawn.Drafted && Util_Melee.IsMeleeDamage(dinfo))
                {
                    int meleelevel = pawn.skills.GetSkill(SkillDefOf.Melee).Level;
                    counterAttackChanceCurrent = CounterAttackChance + meleelevel * Props.CounterAttackChanceIncreasePerLevel;
                    if (Rand.Chance(counterAttackChanceCurrent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void CounterAttack(Pawn wielder, Pawn opponent)
        {
            int meleelevel = wielder.skills.GetSkill(SkillDefOf.Melee).Level;
            float counterAttackChanceCurrent = CounterAttackChance + meleelevel * Props.CounterAttackChanceIncreasePerLevel;
            float counterAttackStabChanceCurrent = CounterAttackStabChance + meleelevel * Props.CounterAttackChanceIncreasePerLevel * 0.5f;
            float damageBonus = meleelevel * Util_Melee.PawnMeleeLevelDamageMultiplier(wielder);
            float damage = CounterAttackDamage + damageBonus;
            if (wielder != null && opponent != null)
            {
                DamageDef damagedef;
                float armorPenetration = CounterAttackArmorPenetrationBase;
                if (Rand.Chance(CounterAttackStabChance))
                {
                    float stabBonus = damageBonus * Util_Melee.PawnMeleeLevelDamageMultiplier(wielder);
                    damagedef = DamageDefOf.Stab;
                    armorPenetration = CounterAttackStabArmorPenetration;
                    damage = CounterAttackStabDamage + stabBonus;
                }
                damagedef = DamageDefOf.Cut;
                DamageInfo dinfo = new DamageInfo(damagedef, damage, armorPenetration, -1
                    , wielder, null, wielder.equipment.Primary.def, DamageInfo.SourceCategory.ThingOrUnknown,
                    opponent, true, true, QualityCategory.Normal, true);
                opponent.TakeDamage(dinfo);
                MoteMaker.ThrowText(wielder.PositionHeld.ToVector3(), wielder.MapHeld, "CounterAttack", 1f);
            }

        }

    }
    public class CompProperties_CounterAttack : CompProperties
    {
        public CompProperties_CounterAttack()
        {
            this.compClass = typeof(Comp_CounterAttack);
        }
        public float CounterAttackChance = 0.2f;
        public float CounterAttackDamage = 20f;

        public float CounterAttackStabChance = 0.1f;
        public float CounterAttackArmorPenetrationBase = 0.4f;
        public float CounterAttackStabArmorPenetration = 0.8f;
        public float CounterAttackStabDamage = 40f;
        public float CounterAttackChanceIncreasePerLevel = 0.02f;
    }

}
