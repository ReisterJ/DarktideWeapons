using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using static UnityEngine.Scripting.GarbageCollector;

namespace DarktideWeapons
{
    public class DW_Laser : DW_Projectile
    {
        protected Material LaserLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColor, Color.red);
        protected int laserTicks = 30;

        protected Vector3 startPoint;
        protected Vector3 endPoint;


        protected MoteDualAttached laserMote;
        protected bool isLaserActive = false;
        protected override void Tick()
        {
            laserTicks--;
            if (isLaserActive)
            {
                
                if (laserTicks <= 0)
                {
                    isLaserActive = false;
                }
            }
            
            if (ambientSustainer != null)
            {
                ambientSustainer.Maintain();
            }
            if (laserTicks <= 0)
            {
                isLaserActive = false;
                if (laserMote != null && !laserMote.Destroyed)
                {
                    laserMote.Destroy();
                    laserMote = null;
                }
                this.Destroy();
            }
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            this.isLaser = true;
            this.startPoint = origin;
            List<IntVec3> cells = new List<IntVec3>();
            laserTicks = 45;

            this.isLaserActive = true;
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
            this.endPoint = usedTarget.Cell.ToVector3Shifted();
            critFlag = Util_Crit.IsCrit(this.critChanceinGame);

            if (!def.projectile.soundAmbient.NullOrUndefined())
            {
                ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            DrawLaser();
            ImpactSomething();
        }

        
        protected void DrawLaser()
        {
            if (this.projectileProps.beamMoteDef != null)
            {
                laserMote = MoteMaker.MakeInteractionOverlay(this.projectileProps.beamMoteDef, this.launcher ,new TargetInfo(this.usedTarget.Cell,this.Map) );
            }
            else
            {
                GenDraw.DrawLineBetween(origin, usedTarget.Cell.ToVector3(), LaserLineMat, 0.05f);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Comps_PostDraw();
        }

        protected override void ImpactSomething()
        {
            usedTargetHit = true;
           
            if (usedTarget.HasThing && CanHit(usedTarget.Thing))
            {
                if (usedTarget.Thing is Pawn p && p.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && !Rand.Chance(0.5f))
                {
                    //ThrowDebugText("miss-laying", base.Position);
                    Impact(null);
                }
                else
                {
                    Impact(usedTarget.Thing);
                }
                return;
            }
            List<Thing> list = VerbUtility.ThingsToHit(base.Position, base.Map, CanHit);
            list.Shuffle();
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                float num;
                if (thing is Pawn pawn)
                {
                    num = Util_Ranged.Intercept_PawnBodySize_Factor * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
                    if (pawn.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f)
                    {
                        num *= 0.5f;
                    }

                }
                else
                {
                    num = 1.5f * thing.def.fillPercent;
                }
                if (Rand.Chance(num))
                {
                    //ThrowDebugText("hit-" + num.ToStringPercent(), base.Position);
                    Impact(list.RandomElement());
                    return;
                }
                //ThrowDebugText("miss-" + num.ToStringPercent(), base.Position);
            }
            Impact(null);
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
            if (this.PenetratedTarget > penetrateNum || forcedStop)
            {
                destroyFlag = true;
            }
            if (hitThing != null)
            {

                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = CalculateDamage(hitThing);
                dinfo.SetWeaponQuality(equipmentQuality);
                DamageWorker.DamageResult damageResult = hitThing.TakeDamage(dinfo);
                damageResult.AssociateWithLog(battleLogEntry_RangedImpact);

                if (critFlag)
                {
                    DamageInfo dinfoFlame = new DamageInfo(DamageDefOf.Flame, this.DamageAmount / 2 * RangedDamageMultiplierGlobal, 1f, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                    hitThing.TakeDamage(dinfoFlame).AssociateWithLog(battleLogEntry_RangedImpact);
                }

                Pawn pawn2 = hitThing as Pawn;
                pawn2?.stances?.stagger.Notify_BulletImpact(this);

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
