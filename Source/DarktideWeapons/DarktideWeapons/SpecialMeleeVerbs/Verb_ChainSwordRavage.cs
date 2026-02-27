using DarktideWeapons.MeleeComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public class Verb_ChainSwordRavage : Verb_AbilityMelee, ISpecialMeleeAttack
    {
        private bool isActive = false;
        private int ticksElapsed = 0;
        private int lastDamageTick = 0;
        private const int durationTicks = 60; 
        private const int damageInterval = 20;

        private bool crit = false;

        private bool doneCritCheck = false;
        public Comp_SpecialMelee SpecialMeleeComp
        {
            get
            {
                return DW_equipment.TryGetComp<Comp_SpecialMelee>();
            }
        }
        protected override int ShotsPerBurst => verbProps.burstShotCount;


        public override void WarmupComplete()
        {
            base.WarmupComplete();
            this.doneCritCheck = false;
        }
        protected override bool TryCastShot()
        {
            return TryStartSpecialMeleeAttack();
        }

        public bool TryStartSpecialMeleeAttack()
        {
            if (currentTarget.Pawn == null || currentTarget.Pawn.Dead)
                return false;

            if (!CasterPawn.Spawned)
            {
                return false;
            }
            this.tool = this.SpecialMeleeComp.Props.specialAttackToolPrimary;
            DoMeleeDamage();
            return true;
        }
        public override void BurstingTick()
        {
            base.BurstingTick();
            //Log.Message("bursting tick");
            
        }
       
        private bool IsCrit()
        {
            return Util_Crit.IsCrit(this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critChance);
        }
        private void DoMeleeDamage()
        {
            Pawn targetPawn = currentTarget.Pawn;
            if (targetPawn != null && !targetPawn.Dead)
            {
                Util_Stagger.StunHandler(targetPawn, this.verbProps.burstShotCount * verbProps.ticksBetweenBurstShots, CasterPawn);
                SpecialAttackStatSet(out float dam, out float ap, out DamageDef damageDef);
                var dinfo = new DamageInfo(damageDef, dam, ap, -1, CasterPawn,hitPart:Util_Melee.TryHitCorePart(CasterPawn,targetPawn));
                targetPawn.TakeDamage(dinfo);
            }
        }
        protected void SpecialAttackStatSet(out float dam, out float ap, out DamageDef damageDef)
        {
            if (SpecialMeleeComp == null)
            {
                Log.Error("SpecialMelee called but SpecialMeleeComp is null");
                dam = 1f;
                ap = 0.1f;
                damageDef = DamageDefOf.Blunt;
                return;
            }
            dam = this.tool.power * CasterPawn.GetStatValue(StatDefOf.MeleeDamageFactor);
            dam *= CasterPawn.ageTracker.CurLifeStage.meleeDamageFactor;
            dam *= Util_Melee.PawnMeleeLevelDamageMultiplier(CasterPawn) * MeleeDamageMultiplierGlobal;
            ap = this.tool.armorPenetration;
            ap *= DW_equipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier);
            damageDef = this.tool.Maneuvers.FirstOrDefault().verb.meleeDamageDef;
            if (!doneCritCheck)
            {
                crit = IsCrit();
                doneCritCheck = true;
                if (crit)
                {
                    Util_Crit.CritMoteMaker(currentTarget.Thing);
                }
            }
            if (crit) {
                dam *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critDamageMultiplier;
                ap *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critArmorPenetrationMultiplier;
            }
        }

        
    }
}
