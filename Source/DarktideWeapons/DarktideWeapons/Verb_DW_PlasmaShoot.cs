using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DarktideWeapons
{
    public class Verb_DW_PlasmaShoot : Verb_DW_Shoot
    {
        public Comp_DarktidePlasma PlasmaComp => DarktideWeapon.TryGetComp<Comp_DarktidePlasma>();
        public override bool Available()
        {
            bool flag = base.Available();
            if (DarktideWeapon != null)
            {
                if (PlasmaComp != null)
                {
                    flag = PlasmaComp.AllowShoot();
                }
            }
            return flag;
        }

        //我不认为暗潮有人会拿着等离子这种纯直弹道打不中人，于是乎我这么写了
        protected override bool TryCastShot()
        {
            PlasmaComp?.SwitchToNormalMode();
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
            Vector3 drawPos = caster.DrawPos;
            DW_Projectile projectile2 = (DW_Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
            ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;

            if (currentTarget.Thing != null)
            {
                projectile2.Launch(manningPawn, drawPos, currentTarget, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
            }
            else
            {
                projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
            }
            PlasmaComp?.HeatBuild();
            return true;
        }
    }

    public class Verb_DW_PlasmaChargedShoot : Verb_DW_PlasmaShoot
    {
        protected override bool TryCastShot()
        {
            PlasmaComp?.SwitchToChargeMode();
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
            Vector3 drawPos = caster.DrawPos;
            DW_Projectile projectile2 = (DW_Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
            ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;

            if (currentTarget.Thing != null)
            {
                projectile2.Launch(manningPawn, drawPos, currentTarget, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
            }
            else
            {
                projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
            }
            PlasmaComp?.HeatBuild();
            return true;
        }
    }



}
