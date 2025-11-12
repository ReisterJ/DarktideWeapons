using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DarktideWeapons
{
    public class DW_Plasma : DW_Projectile
    {
        protected MoteDualAttached TrailMote;

        public IntVec3 finalHitCell;
        protected override void EquipmentProjectileInit(ThingWithComps equipment)
        {
            PlasmaShotInit(equipment);
        }
        protected Material TrailMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColor, Color.red);

        protected override void Tick()
        {
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
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            if (projectileProps != null)
            {
                this.Initiate(equipment);
            }
            this.launcher = launcher;
            this.origin = origin;
            this.usedTarget = usedTarget;
            this.intendedTarget = intendedTarget;
            this.targetCoverDef = targetCoverDef;
            this.preventFriendlyFire = preventFriendlyFire;
            HitFlags = hitFlags;
            stoppingPower = def.projectile.stoppingPower;
            if (stoppingPower == 0f && def.projectile.damageDef != null)
            {
                stoppingPower = def.projectile.damageDef.defaultStoppingPower;
            }
            this.lifetime = 20;
            if (equipment != null)
            {
                this.equipment = equipment;
                equipmentDef = equipment.def;
                equipment.TryGetQuality(out equipmentQuality);
                if (equipment.TryGetComp(out CompUniqueWeapon comp))
                {
                    foreach (WeaponTraitDef item in comp.TraitsListForReading)
                    {
                        if (!Mathf.Approximately(item.additionalStoppingPower, 0f))
                        {
                            stoppingPower += item.additionalStoppingPower;
                        }
                    }
                }
            }
            else
            {
                equipmentDef = null;
            }
            
            critFlag = Util_Crit.IsCrit(this.critChanceinGame);

            if (!def.projectile.soundAmbient.NullOrUndefined())
            {
                ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }

            int totalHitCount = 0;
            List<Thing> targetList = new List<Thing>();
            IntVec3 finalDestination = intendedTarget.Cell;
            List<IntVec3> cells = Util_Ranged.GetLineSegmentCells(origin.ToIntVec3(), this.usedTarget.Cell, this.projectileProps.effectiveRange,this.Map);
            finalHitCell = cells.Last();
            foreach (IntVec3 cell in cells)
            {
                List<Thing> things = cell.GetThingList(this.Map);
                int cellHitCount = 0;
                foreach (Thing thing in things)
                { 
                    if ( HitCheck(thing))
                    {
                        totalHitCount++;
                        Log.Message("Available target : " + thing.Label);
                        targetList.Add(thing);
                    }
                    
                }
                if (totalHitCount >= penetrateNum || forcedStop)
                {
                    finalDestination = cell;
                    finalHitCell = cell;
                    break;
                }
            }
            // Mote_PlasmaTrail plasmaTrail = (Mote_PlasmaTrail)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
            //plasmaTrail.start = origin;
            //plasmaTrail.end = this.destination;
            //GenSpawn.Spawn(plasmaTrail, origin.ToIntVec3(), this.Map);
            //GenDraw.DrawLineBetween(origin, finalDestination.ToVector3(), TrailMat, 0.05f);
            DrawTrail();
            foreach (Thing target in targetList)
            {
                Impact(target);
            }
        }

        protected void DrawTrail()
        {
            if (this.projectileProps?.beamMoteDef != null)
            {
                TrailMote = MoteMaker.MakeInteractionOverlay(this.projectileProps.beamMoteDef, this.launcher, new TargetInfo(this.finalHitCell, this.Map));
            }
            else
            {
                GenDraw.DrawLineBetween(origin, usedTarget.Cell.ToVector3(), TrailMat, 0.05f);
            }
        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            //DrawLaser();
            Comps_PostDraw();
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            IntVec3 position = base.Position;
            GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            NotifyImpact(hitThing, map, position);
            if (blockedByShield)
            {
                this.ExplosionImpact(hitThing);
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
                pawn2?.stances?.stagger.Notify_BulletImpact(this);
                if (pawn2 != null)
                {
                    pawn2.stances?.stagger.Notify_BulletImpact(this);

                    foreach (HediffDef hediffdef in this.projectileProps.applyHediffDefs)
                    {
                        pawn2.health.AddHediff(hediffdef, Util_BodyPart.GetTorsoPart(pawn2), null, null);
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
                return;
            }
            if (forcedStop)
            {
                this.ExplosionImpact(hitThing);
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
        public bool HitCheck(Thing thing)
        {
            if (thing.def.Fillage == FillCategory.Full && !CanHit(thing) && !this.penetrateWall)
            {
                return false;
            }
            if (thing.def.Fillage == FillCategory.Full)
            {
                if (thing is Building_Door Door && Door.Open == true)
                {
                    return false;
                }
                else
                {

                    if (penetrateWall)
                    {
                        return true;
                    }
                    forcedStop = true;
                    return true;
                }
            }
            else
            {
                if(thing is Pawn pawn)
                {
                    float pawnHitProbability = Util_Ranged.Intercept_PawnBodySize_Factor * Mathf.Clamp(pawn.BodySize, 0.5f, 3f);
                    if (pawn.GetPosture() != 0)
                    {
                        pawnHitProbability *= Util_Ranged.Intercept_PawnPosture_Downed_Factor;
                    }
                    if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
                    {
                        if (preventFriendlyFireinGame)
                        {
                            pawnHitProbability = 0f;
                        }
                        else
                        {
                            pawnHitProbability *= Find.Storyteller.difficulty.friendlyFireChanceFactor * 0.5f;
                        }
                    }
                    if (pawnHitProbability > 0.01f)
                    {
                        return true;
                    }
                }
                else if(thing.def.fillPercent > Util_Ranged.MinFillPercentCountAsCover)
                {
                    if (this.penetrateWall)
                    {
                        return true;
                    }
                    if (this.usedTarget.Thing == thing)
                    {
                        return true;
                    }
                    return Rand.Chance(thing.def.fillPercent);
                }
            }
            return false;
        }
        protected void PlasmaShotInit(ThingWithComps thingWithComps)
        {
            Comp_DarktidePlasma compPlasma = thingWithComps.TryGetComp<Comp_DarktidePlasma>();
            if (compPlasma != null)
            {
                this.isPlasma = true;
                
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal)
                {
                    this.penetrateWall = false;
                }
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged)
                {
                    this.penetrateWall = true;
                }
            }
            
        }
    }

}
