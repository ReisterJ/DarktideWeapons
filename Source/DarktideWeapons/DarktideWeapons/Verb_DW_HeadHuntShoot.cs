using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{
    public class Verb_DW_HeadHuntShoot : Verb_DW_Shoot
    {
        
        public virtual float HeadHuntShootChance()
        {
            if (CasterIsPawn && CasterPawn != null)
            {
                int shootlevel = CasterPawn.skills.GetSkill(SkillDefOf.Shooting).Level;
                return Util_Ranged.HeadHuntChanceCalculation(shootlevel);
            }
            CompMannable compMannable = caster.TryGetComp<CompMannable>();
            if(compMannable.ManningPawn != null)
            {
                int shootlevel = compMannable.ManningPawn.skills.GetSkill(SkillDefOf.Shooting).Level;
                return Util_Ranged.HeadHuntChanceCalculation(shootlevel);
            }
                
            return 0.05f;
        }

        protected override bool TryCastShot()
        {
            if (!Rand.Chance(HeadHuntShootChance()))
            {
                return base.TryCastShot();
            }

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
            projectile2.HeadShotSet();
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
            return true;
        }
    }
}
