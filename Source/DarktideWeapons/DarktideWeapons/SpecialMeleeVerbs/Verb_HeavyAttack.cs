using DarktideWeapons.MeleeComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static DarktideWeapons.Util_Melee;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    public class Verb_HeavyAttack : Verb_AbilityMelee, IHeavyMeleeAttack
    {

        private const int TargetCooldown = 50;

        protected int cleaveTargetsNum = 0;

        public const float expEarnedBase = 200f;

        protected Util_Melee.CraftType craftType = Util_Melee.CraftType.None;
 
        public List<Pair<Pawn, List<DamageInfo>>> TargetChain = new List<Pair<Pawn, List<DamageInfo>>>();

        public Comp_HeavyAttack HeavyAttackComp => DW_equipment?.TryGetComp<Comp_HeavyAttack>();
        public Tool UsedTool
        {
            get 
            { 
                if(HeavyAttackComp == null)
                {
                    return null;
                }
                return HeavyAttackComp.Props.heavyAttackToolPrimary;
            }
        }
        public ModExtension_MeleeWeaponProperties ModExtension_MeleeProp => this.UsedTool?.Maneuvers.FirstOrDefault().GetModExtension<ModExtension_MeleeWeaponProperties>();

        public override bool Available()
        {
            return base.Available();
        }

        public void Notify_HeavyMeleeAttacked()
        {

        }
        public virtual void Notify_MeleeAttacked()
        {

        }
        protected override bool TryCastShot()
        {
            return TryStartHeavyMeleeAttack();
        }

        public bool IsChargeAttack()
        {
            if (this.DW_equipment != null)
            {
                if (this.DW_equipment.Comp_DWChargeWeapon != null)
                {
                    if (this.DW_equipment.Comp_DWChargeWeapon.isCharged)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        protected virtual void ApplyAOEMeleeDamage(Pawn target)
        {
            if (ModExtension_MeleeProp != null)
            {
                this.cleaveTargetsNum = ModExtension_MeleeProp.cleaveTargets;
                if (IsChargeAttack())
                {
                    this.cleaveTargetsNum = DW_equipment.Comp_DWChargeWeapon.NewCleaveNum;
                }
                craftType = ModExtension_MeleeProp.craftType;

                if (craftType == Util_Melee.CraftType.Cardinal)
                {
                    List<IntVec3> targetAdjNear = GenAdjFast.AdjacentCellsCardinal(target.Position);
                    ApplyMeleeDamageToTarget(target);
                    ApplyMeleeDamageToNearTarget(target, targetAdjNear);
                }
                if (craftType == Util_Melee.CraftType.Impale)
                {
                    List<IntVec3> targetAdjNear = new List<IntVec3>();
                    targetAdjNear.Add(target.Position);
                    ApplyMeleeDamageToTarget(target);
                    ApplyMeleeDamageToNearTarget(target, targetAdjNear);
                }
                if (craftType == Util_Melee.CraftType.Strikedown)
                {
                    ApplyMeleeDamageToTarget(target);
                }
                if (craftType == Util_Melee.CraftType.Vanguard)
                {
                    List<IntVec3> targetAdjNear = GenAdjFast.AdjacentCells8Way(target);
                    ApplyMeleeDamageToTarget(target);
                    ApplyMeleeDamageToNearTarget(target, targetAdjNear);
                }
            }
            else
            {
                Log.Error("Verb_HeavyAttack ApplyAOEMeleeDamage called but ModExtension_MeleeProp is null");
            }
        }

        protected virtual void ApplyMeleeDamageToNearTarget(Pawn target, List<IntVec3> cells)
        {
            if (target == null || target.Dead || target.Map == null || target.Position.InBounds(target.Map) == false)
            {
                return;
            }
            if (this.cleaveTargetsNum <= 1)
            {
                return;
            }
            int pawnNum = 0;
            bool stopFlag = false;
            TargetChain.Clear();
            foreach (var p in cells)
            {
                //Log.Message(p);
                IntVec3 tempPos = p;
                if (!tempPos.InBounds(target.Map))
                {
                    continue;
                }
                //Util_Melee.DEV_output(" Position check: " + tempPos);

                foreach (Thing thing in tempPos.GetThingList(target.Map))
                {
                    if (thing is Pawn nextPawnTarget)
                    {
                        if (nextPawnTarget.Dead || !nextPawnTarget.Spawned || nextPawnTarget == CasterPawn) continue;
                        if (nextPawnTarget.HostileTo(CasterPawn))
                        {
                            pawnNum += Mathf.CeilToInt(nextPawnTarget.BodySize);
                            //Util_Melee.DEV_output("Cleave target " + pawnNum + " | Name : " + nextPawnTarget.Name);
                            if (pawnNum >= cleaveTargetsNum)
                            {
                                //Util_Melee.DEV_output("Cleave targets reach Maximum, no more targets");
                                stopFlag = true;
                                break;
                            }
                            List<DamageInfo> damageInfos = new List<DamageInfo>();
                            foreach (DamageInfo item in DamageInfosToApplyCleaveTarget(target, pawnNum))
                            {
                                damageInfos.Add(item);
                            }
                            TargetChain.Add(new Pair<Pawn, List<DamageInfo>>(nextPawnTarget, damageInfos));
                        }
                    }
                }
                if (stopFlag)
                {
                    break;
                }
            }
            foreach (var t in TargetChain)
            {
                if (t != null)
                {
                    if (t.First != null)
                    {
                        foreach (var d in t.Second)
                        {
                            if(t.First is Pawn pawn)
                            {
                                Util_Stagger.StaggerHandler(pawn, Util_Stagger.StaggerTicks(pawn, HeavyAttackComp.staggerlevel), CasterPawn);
                            }
                            t.First.TakeDamage(d);
                        }
                    }
                }
            }
        }
        protected new BattleLogEntry_MeleeCombat CreateCombatLog(Func<ManeuverDef, RulePackDef> rulePackGetter, bool alwaysShow)
        {
            if (maneuver == null)
            {
                return null;
            }
            if (tool == null)
            {
                return null;
            }
            BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = new BattleLogEntry_MeleeCombat(rulePackGetter(maneuver), alwaysShow, CasterPawn, currentTarget.Thing, base.ImplementOwnerType, tool.labelUsedInLogging ? tool.label : "", (base.EquipmentSource == null) ? null : base.EquipmentSource.def, (base.HediffCompSource == null) ? null : base.HediffCompSource.Def, maneuver.logEntryDef);
            Find.BattleLog.Add(battleLogEntry_MeleeCombat);
            return battleLogEntry_MeleeCombat;
        }

        protected bool IsTargetImmobile(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;
            if (thing.def.category == ThingCategory.Pawn && !pawn.Downed)
            {
                return pawn.GetPosture() != PawnPosture.Standing;
            }
            return true;
        }

        protected DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();
            foreach (DamageInfo item in DamageInfosToApply(target))
            {
                if (target.ThingDestroyed)
                {
                    break;
                }
                result = target.Thing.TakeDamage(item);
            }
            return result;
        }



        protected IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
        {
            HeavyAttackStatSet(out float num, out float armorPenetration, out DamageDef def);
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;
            QualityCategory qc = QualityCategory.Normal;
            if (IsChargeAttack())
            {
                num *= DW_equipment.Comp_DWChargeWeapon.NewDamageFactor;
                armorPenetration *= DW_equipment.Comp_DWChargeWeapon.NewArmorPenetrationFactor;
                int chargeconsume = 2;

                if (this.craftType == Util_Melee.CraftType.Strikedown)
                {
                    if (target.Pawn != null)
                    {
                        if (target.Pawn.BodySize >= 3f && DW_equipment.Comp_DWChargeWeapon.BodysizeMatters)
                        {
                            num *= Mathf.Sqrt(target.Pawn.BodySize - 1);
                        }
                    }
                    if (target.Thing is Building building && building.MaxHitPoints >= 2000)
                    {
                        num *= (building.MaxHitPoints / 2000 + 2);
                    }
                }
                if (DW_equipment.Comp_DWChargeWeapon.CauseExplosion)
                {
                    DW_equipment.Comp_DWChargeWeapon.DoChargedExplosion(target.Cell, target.Thing.Map, CasterPawn);
                }
                DW_equipment.Comp_DWChargeWeapon.ConsumeCharge(chargeconsume);
            }
            ThingDef source;
            if (DW_equipment != null)
            {
                source = DW_equipment.def;
                DW_equipment.TryGetQuality(out qc);
            }
            else
            {
                source = CasterPawn.def;
            }
            Vector3 direction = (target.Thing.Position - CasterPawn.Position).ToVector3();
            bool instigatorGuilty = !(caster is Pawn pawn) || !pawn.Drafted;
            DamageInfo damageInfo = new DamageInfo(def, num, armorPenetration, -1f, caster,target.Pawn != null ? Util_Melee.TryHitCorePart(CasterPawn, target.Pawn) :null , source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
            damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
            damageInfo.SetWeaponHediff(hediffDef);
            damageInfo.SetAngle(direction);
            damageInfo.SetTool(tool);
            damageInfo.SetWeaponQuality(qc);
            yield return damageInfo;
            if (tool != null && tool.extraMeleeDamages != null)
            {
                foreach (ExtraDamage extraMeleeDamage in tool.extraMeleeDamages)
                {
                    if (Rand.Chance(extraMeleeDamage.chance))
                    {
                        num = extraMeleeDamage.amount;
                        num = Rand.Range(num * 0.8f, num * 1.2f);
                        damageInfo = new DamageInfo(extraMeleeDamage.def, num, extraMeleeDamage.AdjustedArmorPenetration(this, CasterPawn), -1f, caster, null, source);
                        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                        damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                        damageInfo.SetWeaponHediff(hediffDef);
                        damageInfo.SetAngle(direction);
                        yield return damageInfo;
                    }
                }
            }
            if (!surpriseAttack || ((verbProps.surpriseAttack == null || verbProps.surpriseAttack.extraMeleeDamages.NullOrEmpty()) && (tool == null || tool.surpriseAttack == null || tool.surpriseAttack.extraMeleeDamages.NullOrEmpty())))
            {
                yield break;
            }
            IEnumerable<ExtraDamage> enumerable = Enumerable.Empty<ExtraDamage>();
            if (tool != null && tool.surpriseAttack != null && !tool.surpriseAttack.extraMeleeDamages.NullOrEmpty())
            {
                enumerable = enumerable.Concat(tool.surpriseAttack.extraMeleeDamages);
            }
            foreach (ExtraDamage item in enumerable)
            {
                int num2 = GenMath.RoundRandom(item.AdjustedDamageAmount(this, CasterPawn));
                float armorPenetration2 = item.AdjustedArmorPenetration(this, CasterPawn);
                DamageInfo damageInfo2 = new DamageInfo(item.def, num2, armorPenetration2, -1f, caster, null, source);
                damageInfo2.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                damageInfo2.SetWeaponBodyPartGroup(bodyPartGroupDef);
                damageInfo2.SetWeaponHediff(hediffDef);
                damageInfo2.SetAngle(direction);
                yield return damageInfo2;
            }
        }

        protected virtual IEnumerable<DamageInfo> DamageInfosToApplyCleaveTarget(Pawn target, int pawnNum, float damageFalloffRatio = 0.8f)
        {
           
            HeavyAttackStatSet(out float num, out float armorPenetration, out DamageDef def);
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;
            QualityCategory qc = QualityCategory.Normal;
            for (int i = pawnNum; i > 0; i--)
            {
                num *= damageFalloffRatio;
            }
            if (IsChargeAttack())
            {
                num *= DW_equipment.Comp_DWChargeWeapon.NewDamageFactor;
                armorPenetration *= DW_equipment.Comp_DWChargeWeapon.NewArmorPenetrationFactor;
                DW_equipment.Comp_DWChargeWeapon.ConsumeCharge((int)target.BodySize);
            }
            ThingDef source;
            if (DW_equipment != null)
            {
                source = DW_equipment.def;
                DW_equipment.TryGetQuality(out qc);
            }
            else
            {
                source = CasterPawn.def;
            }
            Vector3 direction = (target.Position - CasterPawn.Position).ToVector3();
            bool instigatorGuilty = !(caster is Pawn pawn) || !pawn.Drafted;
            DamageInfo damageInfo = new DamageInfo(def, num, armorPenetration, -1f, caster, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
            damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
            damageInfo.SetWeaponHediff(hediffDef);
            damageInfo.SetAngle(direction);
            damageInfo.SetTool(tool);
            damageInfo.SetWeaponQuality(qc);
            yield return damageInfo;

            //Extra damage
            if (tool != null && tool.extraMeleeDamages != null)
            {
                foreach (ExtraDamage extraMeleeDamage in tool.extraMeleeDamages)
                {
                    if (Rand.Chance(extraMeleeDamage.chance))
                    {
                        num = extraMeleeDamage.amount;
                        for (int i = pawnNum; i > 1; i--)
                        {
                            num *= damageFalloffRatio;
                        }
                        damageInfo = new DamageInfo(extraMeleeDamage.def, num, extraMeleeDamage.AdjustedArmorPenetration(this, CasterPawn), -1f, caster, null, source);
                        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                        damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                        damageInfo.SetWeaponHediff(hediffDef);
                        damageInfo.SetAngle(direction);
                        yield return damageInfo;
                    }
                }
            }
        }

        public bool TryStartHeavyMeleeAttack()
        {
            this.tool = this.UsedTool;
            Pawn casterPawn = CasterPawn;
            if (!casterPawn.Spawned)
            {
                return false;
            }
            Thing thing = currentTarget.Thing;
            if ((thing == null) || (thing is Pawn victim && victim.Dead))
            {
                return false;
            }
            casterPawn.rotationTracker.Face(thing.DrawPos);
            if (!IsTargetImmobile(currentTarget) && casterPawn.skills != null && (currentTarget.Pawn == null || !currentTarget.Pawn.IsColonyMech))
            {
                casterPawn.skills.Learn(SkillDefOf.Melee, expEarnedBase);
            }
            Pawn pawn = thing as Pawn;
            if (pawn != null && !pawn.Dead && (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || pawn.MentalStateDef != MentalStateDefOf.SocialFighting) && (casterPawn.story == null || !casterPawn.story.traits.DisableHostilityFrom(pawn)))
            {
                pawn.mindState.meleeThreat = casterPawn;
                pawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
            }
            Map map = thing.Map;
            Vector3 drawPos = thing.DrawPos;
            SoundDef soundDef;
            bool result;
            if (verbProps.impactMote != null)
            {
                MoteMaker.MakeStaticMote(drawPos, map, verbProps.impactMote);
            }
            if (verbProps.impactFleck != null)
            {
                FleckMaker.Static(drawPos, map, verbProps.impactFleck);
            }
            BattleLogEntry_MeleeCombat battleLogEntry_MeleeCombat = CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesHit, alwaysShow: true);
            result = true;
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();

            if (currentTarget.Thing is Pawn pawnTarget)
            {
                ApplyAOEMeleeDamage(pawnTarget);
            }
            else
            {
                damageResult = ApplyMeleeDamageToTarget(currentTarget);
            }
            if (casterPawn.Spawned)
            {
                casterPawn.Drawer.Notify_MeleeAttackOn(thing);
            }

            Util_Stagger.StaggerHandler(pawn, Util_Stagger.StaggerTicks(pawn,HeavyAttackComp.staggerlevel),casterPawn);


            if (casterPawn.Spawned)
            {
                casterPawn.rotationTracker.FaceCell(thing.Position);
            }
            if (casterPawn.caller != null)
            {
                casterPawn.caller.Notify_DidMeleeAttack();
            }
            return result;
        }

        protected void HeavyAttackStatSet(out float dam, out float ap,out DamageDef damageDef)
        {
            if(HeavyAttackComp == null)
            {
                Log.Error("HeavyAttackStatSet called but HeavyAttackComp is null");
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
        }
    }
}
