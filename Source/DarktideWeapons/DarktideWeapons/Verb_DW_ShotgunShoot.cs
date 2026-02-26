using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{
    public class Verb_DW_ShotgunShoot : Verb_DW_Shoot
    {
        public int projectileNum;

        public float spreadAngleMax;

        public float spreadAngleMin;

        protected float spreadOffset;


        
        public void SpreadOffset()
        {

            spreadAngleMax -= spreadOffset;
            if (spreadAngleMax < spreadAngleMin) spreadAngleMax = spreadAngleMin;
        }
        protected bool ShotgunFire(ProjectileHitFlags hitflag, ThingDef targetCoverDef)
        {
            ModExtension_ShotgunProperties shotgunProperties = this.EquipmentSource?.def.GetModExtension<ModExtension_ShotgunProperties>();
            if(shotgunProperties != null)
            {
                if (shotgunProperties.projectileNum <= 0)
                {
                    Log.Error($"Shotgun {this.EquipmentSource.def.defName} has invalid projectileNum {shotgunProperties.projectileNum}.Switch to Default");
                    projectileNum = ModExtension_ShotgunProperties.DefaultValue.projectileNum;
                }
                else
                {
                    projectileNum = shotgunProperties.projectileNum;
                }
                if(shotgunProperties.spreadAngleMax < shotgunProperties.spreadAngleMin)
                {
                    Log.Error($"Shotgun {this.EquipmentSource.def.defName} has invalid spreadAngle {shotgunProperties.spreadAngleMax}.Switch to Default");
                    spreadAngleMax = ModExtension_ShotgunProperties.DefaultValue.spreadAngleMax;
                    spreadAngleMin = ModExtension_ShotgunProperties.DefaultValue.spreadAngleMin;
                }
                else
                {
                    spreadAngleMin = shotgunProperties.spreadAngleMin;
                    spreadAngleMax = shotgunProperties.spreadAngleMax;
                }
                SpreadOffset();

                Vector3 origin = caster.DrawPos;
                Vector3 target = currentTarget.CenterVector3;
                for (int i = 0; i < projectileNum; i++)
                {
                    float angle = Rand.Range(spreadAngleMin, spreadAngleMax);
                    float randomAngle = Rand.Range(-angle, angle);

                    Vector3 scatterDirection = (target - origin).normalized;
                    scatterDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * scatterDirection;
                    Vector3 scatterTarget = origin + scatterDirection * (target - origin).magnitude;

                    Projectile projectile = (Projectile)GenSpawn.Spawn(verbProps.defaultProjectile, caster.Position, caster.Map);
                    if (randomAngle > 1f) {
                        hitflag = ProjectileHitFlags.NonTargetPawns;
                    }  
                    else { hitflag = ProjectileHitFlags.IntendedTarget; }
                    projectile.Launch(caster, origin, scatterTarget.ToIntVec3(), currentTarget, hitflag, true, EquipmentSource,targetCoverDef);
                }
                if (verbProps.consumeFuelPerShot > 0f && caster.TryGetComp<CompRefuelable>() is CompRefuelable compRefuelable)
                {
                    compRefuelable.ConsumeFuel(verbProps.consumeFuelPerShot);
                }
                return true;
            }
            return false;
        }
        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ThingDef projectile = Projectile;
            if (projectile == null)
            {
                return false;
            }
            ShootLine resultingLine;
            bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
            if (verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompApparelVerbOwner_Charged>()?.UsedOnce();
            }
            lastShotTick = Find.TickManager.TicksGame;
            Thing manningPawn = caster;
            Thing equipmentSource = base.EquipmentSource;
            CompMannable compMannable = caster.TryGetComp<CompMannable>();
            if (compMannable?.ManningPawn != null)
            {
                manningPawn = compMannable.ManningPawn;
                equipmentSource = caster;
            }
            ProjectileHitFlags hitflags = ProjectileHitFlags.IntendedTarget;
            
            ShotgunFire(hitflags, null);
            CasterPawn?.records.Increment(RecordDefOf.ShotsFired);
            return true;
        }
    }
}
