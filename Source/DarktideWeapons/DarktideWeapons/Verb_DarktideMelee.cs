using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.RitualStage_InteractWithRole;
using static UnityEngine.GraphicsBuffer;

namespace DarktideWeapons
{
    public class Verb_DarktideMelee : Verb_MeleeAttack
    {
        private const int TargetCooldown = 50;

        // cleaveTargetsNum = MainTarget + cleaveTargets. If cleaveTargetsNum is 2 , the main target and one additional target will take damage.
        protected int cleaveTargetsNum = 0;

        public const float expEarnedBase = 200f;

        protected Util_Melee.CraftType craftType = Util_Melee.CraftType.None;
        public ModExtension_MeleeWeaponProperties ModExtension_MeleeProp => this.maneuver.GetModExtension<ModExtension_MeleeWeaponProperties>();

        public List<Pair<Pawn, List<DamageInfo>>> TargetChain = new List<Pair<Pawn, List<DamageInfo>>>();
        public DW_Equipment DW_equipment => this.EquipmentSource as DW_Equipment;
        public float MeleeDamageMultiplierGlobal => LoadedModManager.GetMod<DW_Mod>().GetSettings<DW_ModSettings>().MeleeDamageMultiplierGlobal;

        
        protected override bool TryCastShot()
        {
            Pawn casterPawn = CasterPawn;
            if (!casterPawn.Spawned)
            {
                return false;
            }
            if (casterPawn.stances.FullBodyBusy)
            {
                return false;
            }
            Thing thing = currentTarget.Thing;
            if((thing == null) || (thing is Pawn victim && victim.Dead))
            {
                return false;
            }
            
            if (!CanHitTarget(thing))
            {
                Log.Warning(string.Concat(casterPawn, " meleed ", thing, " from out of melee position."));
            }
            casterPawn.rotationTracker.Face(thing.DrawPos);
            if (!IsTargetImmobile(currentTarget) && casterPawn.skills != null && (currentTarget.Pawn == null || !currentTarget.Pawn.IsColonyMech))
            {
                casterPawn.skills.Learn(SkillDefOf.Melee, expEarnedBase * verbProps.AdjustedFullCycleTime(this, casterPawn));
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
            if (Rand.Chance(GetNonMissChance(thing)))
            {
                if (!Rand.Chance(GetDodgeChance(thing)))
                {
                    soundDef = ((thing.def.category != ThingCategory.Building) ? SoundHitPawn() : SoundHitBuilding());
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
                    // aoe
                    if (currentTarget.Thing is Pawn pawnTarget)
                    {
                        ApplyAOEMeleeDamage(pawnTarget);
                    }
                    else
                    {
                        damageResult = ApplyMeleeDamageToTarget(currentTarget);
                    }
                }
                else
                {
                    result = false;
                    soundDef = SoundDodge(thing);
                    MoteMaker.ThrowText(drawPos, map, "TextMote_Dodge".Translate(), 1.9f);
                    CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesDodge, alwaysShow: false);
                }
            }
            else
            {
                result = false;
                soundDef = SoundMiss();
                CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesMiss, alwaysShow: false);
            }
            soundDef?.PlayOneShot(new TargetInfo(thing.Position, map));
            if (casterPawn.Spawned)
            {
                casterPawn.Drawer.Notify_MeleeAttackOn(thing);
            }
            if (pawn != null && !pawn.Dead && pawn.Spawned)
            {
                pawn.stances.stagger.StaggerFor(95);
            }
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
                    ApplyMeleeDamageToNearTarget(target,targetAdjNear);
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
            
        }

        protected virtual void ApplyMeleeDamageToNearTarget(Pawn target,List<IntVec3> cells)
        {
            if (target == null || target.Dead || target.Map == null || target.Position.InBounds(target.Map) == false)
            {
                return;
            }
            if (this.cleaveTargetsNum <= 1)
            {
                return;
            }
            //Util_Melee.DEV_output(" CleaveTargets valid ");
            //IntVec3 targetPos = target.Position;
            //var targetAdjCardinal = cells;

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

        protected virtual float GetNonMissChance(LocalTargetInfo target)
        {
            if (surpriseAttack)
            {
                return 1f;
            }
            if (IsTargetImmobile(target))
            {
                return 1f;
            }
            float num = CasterPawn.GetStatValue(StatDefOf.MeleeHitChance);
            if (ModsConfig.IdeologyActive && target.HasThing)
            {
                if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                {
                    num += caster.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsLitOffset);
                }
                else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                {
                    num += caster.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                {
                    num += caster.GetStatValue(StatDefOf.MeleeHitChanceIndoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                {
                    num += caster.GetStatValue(StatDefOf.MeleeHitChanceIndoorsLitOffset);
                }
            }
            return num;
        }

        protected virtual float GetDodgeChance(LocalTargetInfo target)
        {
            if (surpriseAttack)
            {
                return 0f;
            }
            if (IsTargetImmobile(target))
            {
                return 0f;
            }
            if (!(target.Thing is Pawn pawn))
            {
                return 0f;
            }
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
			if (stance_Busy != null && stance_Busy.verb != null && !stance_Busy.verb.verbProps.IsMeleeAttack)
			{
				return 0f;
			}
            float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
            if (ModsConfig.IdeologyActive)
            {
                if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset);
                }
                else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsLitOffset);
                }
            }
            return num;
        }

        protected virtual bool IsTargetImmobile(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;
            if (thing.def.category == ThingCategory.Pawn && !pawn.Downed)
            {
                return pawn.GetPosture() != PawnPosture.Standing;
            }
            return true;
        }

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
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


        // first target
        protected IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
        {
            float num = verbProps.AdjustedMeleeDamageAmount(this, CasterPawn) * Util_Melee.PawnMeleeLevelDamageMultiplier(CasterPawn) * MeleeDamageMultiplierGlobal;
            float armorPenetration = verbProps.AdjustedArmorPenetration(this, CasterPawn);
            DamageDef def = verbProps.meleeDamageDef;
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;
            QualityCategory qc = QualityCategory.Normal;
            //充能武器对单有特殊判定
            if (IsChargeAttack())
            {
                num *= DW_equipment.Comp_DWChargeWeapon.NewDamageFactor;
                armorPenetration *= DW_equipment.Comp_DWChargeWeapon.NewArmorPenetrationFactor;
                int chargeconsume = 2;
                
                if(this.craftType == Util_Melee.CraftType.Strikedown)
                {
                    if(target.Pawn != null)
                    {
                        if(target.Pawn.BodySize >= 3f && DW_equipment.Comp_DWChargeWeapon.BodysizeMatters)
                        {
                            num *= Mathf.Sqrt(target.Pawn.BodySize - 1);
                        }
                    }
                    if(target.Thing is Building building && building.MaxHitPoints >= 2000)
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

            if (CasterIsPawn)
            {
                bodyPartGroupDef = verbProps.AdjustedLinkedBodyPartsGroup(tool);
                if (num >= 1f)
                {
                    if (base.HediffCompSource != null)
                    {
                        hediffDef = base.HediffCompSource.Def;
                    }
                }
                else
                {
                    num = 1f;
                    def = DamageDefOf.Blunt;
                }
            }
            ThingDef source;
            if (base.EquipmentSource != null)
            {
                source = base.EquipmentSource.def;
                base.EquipmentSource.TryGetQuality(out qc);
            }
            else
            {
                source = CasterPawn.def;
            }
            Vector3 direction = (target.Thing.Position - CasterPawn.Position).ToVector3();
            bool instigatorGuilty = !(caster is Pawn pawn) || !pawn.Drafted;
            DamageInfo damageInfo = new DamageInfo(def, num, armorPenetration, -1f, caster, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, instigatorGuilty);
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
            if (verbProps.surpriseAttack != null && verbProps.surpriseAttack.extraMeleeDamages != null)
            {
                enumerable = enumerable.Concat(verbProps.surpriseAttack.extraMeleeDamages);
            }
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

    
        //main target takes full damage. Others take less as more targets are inflicted
        protected virtual IEnumerable<DamageInfo> DamageInfosToApplyCleaveTarget(Pawn target , int pawnNum ,float damageFalloffRatio = 0.8f)
        {
            float num = verbProps.AdjustedMeleeDamageAmount(this, CasterPawn) * Util_Melee.PawnMeleeLevelDamageMultiplier(CasterPawn) * MeleeDamageMultiplierGlobal;
            float armorPenetration = verbProps.AdjustedArmorPenetration(this, CasterPawn);
            DamageDef def = verbProps.meleeDamageDef;
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;
            QualityCategory qc = QualityCategory.Normal;
            for(int i = pawnNum; i > 0; i--)
            {
                num *= damageFalloffRatio;
            }
            if (IsChargeAttack())
            {
                num *= DW_equipment.Comp_DWChargeWeapon.NewDamageFactor;
                armorPenetration *= DW_equipment.Comp_DWChargeWeapon.NewArmorPenetrationFactor;
                DW_equipment.Comp_DWChargeWeapon.ConsumeCharge((int)target.BodySize);
            }

            if (CasterIsPawn)
            {
                bodyPartGroupDef = verbProps.AdjustedLinkedBodyPartsGroup(tool);
                if (num >= 1f)
                {
                    if (base.HediffCompSource != null)
                    {
                        hediffDef = base.HediffCompSource.Def;
                    }
                }
                else
                {
                    num = 1f;
                    def = DamageDefOf.Blunt;
                }
            }
            ThingDef source;
            if (base.EquipmentSource != null)
            {
                source = base.EquipmentSource.def;
                base.EquipmentSource.TryGetQuality(out qc);
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


        protected bool CanApplyMeleeSlaveSuppression(Pawn targetPawn)
        {
            if (CasterPawn != null && CasterPawn.IsColonist && !CasterPawn.IsSlave && targetPawn != null && targetPawn.IsSlaveOfColony && targetPawn.health.capacities.CanBeAwake)
            {
                return !SlaveRebellionUtility.IsRebelling(targetPawn);
            }
            return false;
        }



        protected virtual void ApplyMeleeSlaveSuppression(Pawn targetPawn, float damageDealt)
        {
            if (CanApplyMeleeSlaveSuppression(targetPawn))
            {
                SlaveRebellionUtility.IncrementMeleeSuppression(CasterPawn, targetPawn, damageDealt);
            }
        }

        protected virtual SoundDef SoundHitPawn()
        {
            if (base.EquipmentSource != null && !base.EquipmentSource.def.meleeHitSound.NullOrUndefined())
            {
                return base.EquipmentSource.def.meleeHitSound;
            }
            if (tool != null && !tool.soundMeleeHit.NullOrUndefined())
            {
                return tool.soundMeleeHit;
            }
            if (base.EquipmentSource != null && base.EquipmentSource.Stuff != null)
            {
                if (verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
                    {
                        return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp;
                    }
                }
                else if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
                {
                    return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt;
                }
            }
            if (CasterPawn != null && !CasterPawn.def.race.soundMeleeHitPawn.NullOrUndefined())
            {
                return CasterPawn.def.race.soundMeleeHitPawn;
            }
            return SoundDefOf.Pawn_Melee_Punch_HitPawn;
        }

        protected virtual SoundDef SoundHitBuilding()
        {
            if (currentTarget.Thing is Building building && !building.def.building.soundMeleeHitOverride.NullOrUndefined())
            {
                return building.def.building.soundMeleeHitOverride;
            }
            if (base.EquipmentSource != null && !base.EquipmentSource.def.meleeHitSound.NullOrUndefined())
            {
                return base.EquipmentSource.def.meleeHitSound;
            }
            if (tool != null && !tool.soundMeleeHit.NullOrUndefined())
            {
                return tool.soundMeleeHit;
            }
            if (base.EquipmentSource != null && base.EquipmentSource.Stuff != null)
            {
                if (verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
                    {
                        return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp;
                    }
                }
                else if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
                {
                    return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt;
                }
            }
            if (CasterPawn != null && !CasterPawn.def.race.soundMeleeHitBuilding.NullOrUndefined())
            {
                return CasterPawn.def.race.soundMeleeHitBuilding;
            }
            return SoundDefOf.MeleeHit_Unarmed;
        }

        protected virtual SoundDef SoundMiss()
        {
            if (CasterPawn != null)
            {
                if (tool != null && !tool.soundMeleeMiss.NullOrUndefined())
                {
                    return tool.soundMeleeMiss;
                }
                if (!CasterPawn.def.race.soundMeleeMiss.NullOrUndefined())
                {
                    return CasterPawn.def.race.soundMeleeMiss;
                }
            }
            return SoundDefOf.Pawn_Melee_Punch_Miss;
        }

        protected virtual SoundDef SoundDodge(Thing target)
        {
            if (target.def.race != null && target.def.race.soundMeleeDodge != null)
            {
                return target.def.race.soundMeleeDodge;
            }
            return SoundMiss();
        }
    }
    
}
