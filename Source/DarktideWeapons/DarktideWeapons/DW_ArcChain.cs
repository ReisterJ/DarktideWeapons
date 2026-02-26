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

        public int MaxChainTargets = 1;
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
                Util_Crit.CritMoteMaker(hitThing);
            }
            BodyPartRecord bodyPart = null;
            damageAmount *= DamageMultiplier_Outer;
            DamageDef ddef = def.projectile.damageDef;
            if (ModLister.AnomalyInstalled)
            {
                ddef = DamageDefOf.ElectricalBurn;
            }
            DamageInfo dinfo = new DamageInfo(ddef, damageAmount, armorPenetration, ExactRotation.eulerAngles.y, launcher, bodyPart, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            return dinfo;
        }
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

            ChainTargetModify();
            DrawLaser();
            ImpactSomething();
        }

        protected void ChainTargetModify()
        {
            if(this.launcher is Pawn pawn)
            {
                Comp_DarktideForceStaff comp = pawn.equipment.Primary.TryGetComp<Comp_DarktideForceStaff>();
                if(comp != null)
                {
                    comp.ChainTarget_QualityOffset(ref MaxChainTargets);
                }
            }
        }
        protected void ArcJump(Pawn mainTarget)
        {
            if (mainTarget == null) return;
            ChainTargets.Clear();
            int linkedEnemiesCount = 0;
            List<Pawn> searchQueue = new List<Pawn>();
            HashSet<Pawn> visited = new HashSet<Pawn>();

            searchQueue.Add(mainTarget);
            visited.Add(mainTarget);

            int queueIndex = 0;
            while (queueIndex < searchQueue.Count && linkedEnemiesCount < MaxChainTargets)
            {
                Pawn currentTarget = searchQueue[queueIndex];
                queueIndex++;
                List<IntVec3> adjacentCells = Util_Melee.GetPawnNearArea(mainTarget,3f);

                foreach (IntVec3 cell in adjacentCells)
                {
                    if (cell.InBounds(currentTarget.Map))
                    {
                        foreach(Thing thing in cell.GetThingList(mainTarget.Map))
                        {
                            if (thing is Pawn targetPawn && !visited.Contains(targetPawn) && targetPawn.HostileTo(launcher))
                            {
                                ChainTargets.Add(targetPawn);
                                linkedEnemiesCount++;
                                visited.Add(targetPawn);
                                searchQueue.Add(targetPawn);
                            }
                        }
                    }
                }
            }
            /*
            if (linkedEnemiesCount < MaxChainTargets)
            {
                foreach (Thing target in ChainTargets)
                {
                    if (target is Pawn targetPawn)
                    {
                        ArcJump(targetPawn);
                    }
                }
            }
            */
            foreach (Thing target in ChainTargets)
            {
                if (target != null)
                {
                    Impact(target);
                }
            }
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
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
            if (hitThing != null)
            {

                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = CalculateDamage(hitThing);
                dinfo.SetWeaponQuality(equipmentQuality);
                DamageWorker.DamageResult damageResult = hitThing.TakeDamage(dinfo);
                damageResult.AssociateWithLog(battleLogEntry_RangedImpact);

                Pawn pawn2 = hitThing as Pawn;
                Util_Stagger.StunHandler(pawn2,Math.Min(this.MaxChainTargets * (int)dinfo.Amount,210) , launcher);

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
        protected override void ImpactSomething()
        {
            usedTargetHit = true;

            if (usedTarget.HasThing)
            {
                
                if (usedTarget.Thing is Pawn target)
                {
                    ArcJump(target);
                }
                Impact(usedTarget.Thing);
                return;
            }
           
            Impact(null);
        }
    }
}
