using DarktideWeapons.MeleeComps;
using DarktideWeapons.ModExtensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public class Verb_ThunderHammerStrike : Verb_AbilityMelee, ISpecialMeleeAttack
    {
        private bool isActive = false;
        private int ticksElapsed = 0;
        private int lastDamageTick = 0;

        private bool crit = false;

        private bool doneCritCheck = false;

        private const int ChargeConsumePerUse = 1;

        public Comp_SpecialMelee SpecialMeleeComp
        {
            get
            {
                return DW_equipment?.TryGetComp<Comp_SpecialMelee>();
            }
        }

        /// <summary>
        /// 需要武器已充能才能使用。
        /// </summary>
        public override bool Available()
        {
            if (!base.Available())
                return false;

            if (DW_equipment?.Comp_DWChargeWeapon == null)
                return false;

            if (!DW_equipment.Comp_DWChargeWeapon.isCharged)
                return false;

            return true;
        }

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
            if (SpecialMeleeComp == null)
            {
                Log.Error("Verb_ThunderHammerStrike: SpecialMeleeComp is null, cannot perform special attack.");
                return false;
            }
            this.tool = this.SpecialMeleeComp.Props.specialAttackToolPrimary;
            DoMeleeDamage();
            return true;
        }

        public override void BurstingTick()
        {
            base.BurstingTick();
        }

        private bool IsCrit()
        {
            return Util_Crit.IsCrit(this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critChance);
        }

        private void DoMeleeDamage()
        {
            Pawn targetPawn = currentTarget.Pawn;
            Log.Message($"[DW-Thunder] DoMeleeDamage() called. target={targetPawn?.LabelShort}, Caster={CasterPawn?.LabelShort}");
            if (targetPawn != null && !targetPawn.Dead)
            {
                Util_Stagger.StunHandler(targetPawn, this.verbProps.burstShotCount * verbProps.ticksBetweenBurstShots, CasterPawn);
                SpecialAttackStatSet(out float dam, out float ap, out DamageDef damageDef);

                // 消耗充能层数
                DW_equipment?.Comp_DWChargeWeapon?.ConsumeCharge(ChargeConsumePerUse);

                var dinfo = new DamageInfo(damageDef, dam, ap, -1, CasterPawn, hitPart: Util_Melee.TryHitCorePart(CasterPawn, targetPawn));
                targetPawn.TakeDamage(dinfo);

                // 闪电特效与音效 —— map 从 CasterPawn 获取，避免 target.Map 为 null
                ThrowLightningEffect(targetPawn, CasterPawn.Map);
            }
        }

        /// <summary>
        /// 命中时播放闪电击中地面的特效与雷鸣音效。
        /// 完全对齐原版 WeatherEvent_LightningStrike.DoStrike 的调用模式。
        /// </summary>
        private void ThrowLightningEffect(Pawn target, Map map)
        {
            if (map == null || target == null) return;

            IntVec3 strikeLoc = target.Position;
            Vector3 loc = strikeLoc.ToVector3Shifted();

            // —— 烟雾与火花（原版闪电同款） ——
            for (int i = 0; i < 4; i++)
            {
                //FleckMaker.ThrowSmoke(loc, map, 1.5f);
                //FleckMaker.ThrowMicroSparks(loc, map);
                FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
            }

            // —— 雷鸣音效（完全对齐原版调用） ——
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);

            // —— 暴击时追加更大闪电光效 ——
            if (crit)
            {
                FleckMaker.ThrowLightningGlow(loc, map, 3f);
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

            // 充能伤害加成
            if (DW_equipment?.Comp_DWChargeWeapon != null && DW_equipment.Comp_DWChargeWeapon.isCharged)
            {
                dam *= DW_equipment.Comp_DWChargeWeapon.NewDamageFactor;
                ap *= DW_equipment.Comp_DWChargeWeapon.NewArmorPenetrationFactor;
            }

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
            if (crit)
            {
                dam *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critDamageMultiplier;
                ap *= this.tool.Maneuvers.First().GetModExtension<ModExtension_MeleeWeaponProperties>().critArmorPenetrationMultiplier;
            }
        }
    }
}
