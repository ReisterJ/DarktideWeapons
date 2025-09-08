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
    public class DW_ArcChain : DW_Laser
    {
        public List<Thing> ChainTargets = new List<Thing>();

        public int MaxChainTargets;

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            this.isLaser = true;
            this.startPoint = origin;
            List<IntVec3> cells = new List<IntVec3>();


            this.isLaserActive = true;
            this.laserTicks = 10;
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
        protected void ArcJump(Pawn mainTarget)
        {
            if (mainTarget == null) return;
            ChainTargets.Clear();

            //ChainTargets.Add(mainTarget);

            IntVec3 cell = mainTarget.Position;
            List<IntVec3> area = Util_Melee.GetPawnNearArea(mainTarget, 2f);
            List<IntVec3> chosenCells = Util_Rand.ChooseRandomCell(area, MaxChainTargets, false);
            for (int i = 0; i < MaxChainTargets; i++)
            {
                if(i>chosenCells.Count) i= chosenCells.Count - 1;
                Pawn pawn = chosenCells[i].GetFirstPawn(mainTarget.Map);
                if(this.preventFriendlyFireinGame && launcher.Faction != null && pawn.Faction != null && pawn.Faction.HostileTo(launcher.Faction))
                {
                    continue;
                }
                ChainTargets.Add(pawn);
            }
            foreach(Thing target in ChainTargets)
            {
                if (target == null) continue;
                //this.startPoint = cell.ToVector3Shifted();
                //this.endPoint = target.Position.ToVector3Shifted();
                //DrawLaser();
                Impact(target);
                cell = target.Position;
            }   
        }

        protected override void ImpactSomething()
        {
            usedTargetHit = true;

            if (usedTarget.HasThing)
            {
                Impact(usedTarget.Thing);
                if (usedTarget.Thing is Pawn target)
                {
                    ArcJump(target);
                }
                return;
            }
           
            Impact(null);
        }
    }
}
