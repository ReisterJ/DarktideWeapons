using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DarktideWeapons
{
    public class DW_FireCloud : DW_Projectile
    {
        //protected float effectRadiusinGame = 1f;

       
        protected override void Tick()
        {
            Vector3 exactPosition = ExactPosition;
            flyingTicks++;
            if (!ExactPosition.InBounds(base.Map))
            {
                base.Position = ExactPosition.ToIntVec3();
                Destroy();
                return;
            }
            Vector3 exactPosition2 = ExactPosition;
            LastPosition = exactPosition;
            if (this.CheckForFreeInterceptBetween(exactPosition, exactPosition2))
            {
                return;
            }
            base.Position = ExactPosition.ToIntVec3();
            if (ambientSustainer != null)
            {
                ambientSustainer.Maintain();
            }
            lifetime--;
            if (lifetime <= 0)
            {
                this.Destroy();
            }
        }
        protected override DamageInfo CalculateDamage(Thing hitThing)
        {
            bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
            float damageAmount = this.DamageAmount * RangedDamageMultiplierGlobal;
            float armorPenetration = this.armorPenetrationinGame;
            //Do crit
            if (critFlag)
            {
                damageAmount *= this.critDamageMultiplierinGame;
                armorPenetration *= this.critArmorPenetrationMultiplier;
                //Util_Crit.CritMoteMaker(hitThing);
            }
            damageAmount *= DamageMultiplier_Outer;
            if (!(hitThing is Pawn))
            {
                damageAmount *=( hitThing.def.BaseFlammability >= Util_Ranged.MinFlammabilityForDamage ? hitThing.def.BaseFlammability : Util_Ranged.MinFlammabilityForDamage);
                if(Rand.Chance(hitThing.def.BaseFlammability / 2f))
                {
                    FireUtility.TryStartFireIn(hitThing.Position, hitThing.Map, 0.1f, launcher);
                }
            }
            BodyPartRecord bodyPart = null;
            DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, damageAmount, armorPenetration, ExactRotation.eulerAngles.y, launcher, bodyPart, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            return dinfo;
        }

        protected override bool CheckForFreeIntercept(IntVec3 c)
        {
            foreach(Thing thing in c.GetThingList(this.Map))
            {
                if (thing.def.Fillage == FillCategory.Full)
                {
                    this.Destroy();
                    return true;
                }
            }
            return false;
        }
        protected override bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
        {
            //Util_Ranged.DEV_output("lastExactPos:" + lastExactPos + " , " + "newExactPos:" + newExactPos);
            if (lastExactPos == newExactPos)
            {
                return false;
            }

            IntVec3 intVecLastExactPos = lastExactPos.ToIntVec3();
            IntVec3 intVecNewExactPos = newExactPos.ToIntVec3();

            //Util_Ranged.DEV_output("intVecLastExactPos:" + intVecLastExactPos + " , intVecNewExactPos:" + intVecNewExactPos);
            if (intVecNewExactPos == intVecLastExactPos || intVecLastExactPos == this.launcher.Position)
            {
                return false;
            }
            if (!intVecLastExactPos.InBounds(base.Map) || !intVecNewExactPos.InBounds(base.Map))
            {
                return false;
            }

            ImpactSomething();

            return CheckForFreeIntercept(intVecNewExactPos);
        }

        protected override void ImpactSomething()
        {
            List<Thing> targets = new List<Thing>();
            List<IntVec3> cells = GenAdjFast.AdjacentCellsCardinal(this.LastPosition.ToIntVec3());
            foreach (IntVec3 cell in cells)
            {
                foreach (Thing thing in cell.GetThingList(this.Map))
                {
                    if (thing is Pawn pawn)
                    {
                        if (!pawn.Spawned || (pawn.Faction != null && !pawn.HostileTo(this.launcher) && preventFriendlyFireinGame))
                        {
                            continue;
                        }
                    }
                    targets.Add(thing);
                }
            }
            foreach (Thing thing in targets) 
            {
                if (thing != null && thing.Spawned)
                {
                    Impact(thing);
                }
            }
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            bool destroyFlag = false;
            Map map = base.Map;
            IntVec3 position = base.Position;
            GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);

            if (!blockedByShield && def.projectile.landedEffecter != null)
            {
                def.projectile.landedEffecter.Spawn(base.Position, base.Map).Cleanup();
            }
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            NotifyImpact(hitThing, map, position);
            if (blockedByShield)
            {
                //destroyFlag = true;
                this.ExplosionImpact(hitThing);
                this.Destroy();
                return;
            }

            if (hitThing != null)
            {
                //Util_Ranged.DEV_output(hitThing.Label);
                lastCollisionTick = Find.TickManager.TicksGame;
                lastHitThing = hitThing;
                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = CalculateDamage(hitThing);
                dinfo.SetWeaponQuality(equipmentQuality);
                DamageWorker.DamageResult damageResult = hitThing.TakeDamage(dinfo);
                damageResult.AssociateWithLog(battleLogEntry_RangedImpact);



                Pawn pawn2 = hitThing as Pawn;
                //pawn2?.stances?.stagger.Notify_BulletImpact(this);
                if (pawn2 != null)
                {
                    pawn2.stances?.stagger.Notify_BulletImpact(this);

                    foreach (HediffDef hediffdef in this.projectileProps.applyHediffDefs)
                    {
                        this.TryAddHediff(hediffdef, pawn2);
                    }
                }

                if (def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                }
                if (this.PenetratedTarget > penetrateNum || forcedStop)
                {
                    this.ExplosionImpact(hitThing);
                    this.Destroy();
                }
                return;
            }
            if (forcedStop)
            {
                this.ExplosionImpact(hitThing);
                this.Destroy();
                return;
            }
            if (!blockedByShield)
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(DamageAmount) * 1f, 4f);
                }
                else
                {
                    FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
                }
            }
        }
    }
}
