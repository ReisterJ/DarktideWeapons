using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace DarktideWeapons
{
    public class DW_Grenade : DW_Projectile
    {
        public bool landedFlag = false;
        public bool sticksToTarget = false;
        public bool sticked = false;
        public bool bounceFlag = false;
        public Thing stickTarget;

        public int ticksToDetonation = 120;

        protected Vector3 bounceDest;
        protected Vector3 bounceStart;

        public float lastBounceRadius;
        public int bounceCounter = 0;
        public override Vector3 ExactPosition
        {
            get
            {
                if(!landed)
                {
                    Vector3 vector = (destination - origin).normalized * flyingTicks * def.projectile.SpeedTilesPerTick;
                    return origin.Yto0() + vector + Vector3.up * def.Altitude;
                }
                else
                {
                    if (!sticksToTarget)
                    {
                        if(LastPosition == bounceDest || flyingTicks > 60)  return LastPosition;
                        Vector3 vector = (bounceDest - bounceStart).normalized * flyingTicks / (bounceCounter * 10);
                        return bounceStart.Yto0() + vector + Vector3.up * def.Altitude;
                    }
                    else
                    {
                        if (stickTarget == null || !stickTarget.Spawned || stickTarget.DestroyedOrNull())
                        {
                            return LastPosition;
                        }
                        return stickTarget.Position.ToVector3Shifted();
                    }   
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 30);
        }

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
            if (this.CheckForFreeInterceptBetween(exactPosition, exactPosition2))
            {
                return;
            }
            LastPosition = exactPosition;
            base.Position = ExactPosition.ToIntVec3();
            
            if (ambientSustainer != null)
            {
                ambientSustainer.Maintain();
            }
            lifetime--;
            if (lifetime <= 0)
            {
                StartCountDown();
            }
            if (ticksToDetonation > 0 && landedFlag)
            {
                ticksToDetonation --;
                if (ticksToDetonation <= 0)
                {
                    Explode();
                }
            }
        }
        public void StartCountDown()
        {
            landedFlag = true;
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
                Util_Crit.CritMoteMaker(hitThing);
            }
            //damage falloff by penetration
            if (this.penetrateNum > 0)
            {
                int temp = Math.Min(Math.Max(this.PenetratedTarget - 1, 0), this.penetrateNum);
                damageAmount *= Mathf.Pow(this.penetrateDamageFalloffRatio, temp);
            }
            damageAmount *= DamageMultiplier_Outer * (landedFlag ? 0.01f : 1f );
            DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, damageAmount, armorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            return dinfo;
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            bool destroyFlag = false;
            //landedFlag = true;
            landed = true;
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
                destroyFlag = true;
                Explode();
            }
            if (hitThing != null)
            {
                bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
                DamageInfo dinfo = CalculateDamage(hitThing);
                dinfo.SetWeaponQuality(equipmentQuality);
                DamageWorker.DamageResult damageResult = hitThing.TakeDamage(dinfo);
                damageResult.AssociateWithLog(battleLogEntry_RangedImpact);
                sticksToTarget = this.projectileProps.isStickyBomb;
                Pawn pawn2 = hitThing as Pawn;
                if(pawn2 != null)
                {
                    pawn2.stances?.stagger.Notify_BulletImpact(this);
                    foreach (HediffDef hediffDef in projectileProps.applyHediffDefs)
                    {
                        pawn2.health.AddHediff(hediffDef, GetTorsoPart(pawn2) , null, null);
                    }
                }
                if (sticksToTarget)
                {
                    sticked = true;
                    stickTarget = hitThing;
                }
                else
                {
                    if(bounceCounter < 1)
                    {
                        flyingTicks = 0;
                        bounceStart = this.ExactPosition;
                        Bounce(bounceStart);
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
        protected virtual void Explode()
        {
            Map map = base.Map;
            Destroy();
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                if (def.projectile.explosionEffectLifetimeTicks != 0)
                {
                    map.effecterMaintainer.AddEffecterToMaintain(effecter, base.Position.ToVector3().ToIntVec3(), def.projectile.explosionEffectLifetimeTicks);
                }
                else
                {
                    effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                    effecter.Cleanup();
                }
            }
            IntVec3 position = base.Position;
            float explosionRadius = projectileProps.explosionRadius;
            DamageDef damageDef = projectileProps.explosionDamageDef;
            Thing instigator = launcher;
            int damageAmount = (int)projectileProps.explosionDamage;
            float armorPenetration = projectileProps.explosionArmorPenetration;
            SoundDef soundExplode = def.projectile.soundExplode;
            ThingDef weapon = equipmentDef;
            ThingDef projectile = def;
            Thing thing = intendedTarget.Thing;
            ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef ?? (def.projectile.explosionSpawnsSingleFilth ? null : def.projectile.filth);
            ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
            float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
            GasType? postExplosionGasType = def.projectile.postExplosionGasType;
            ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
            float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
            int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
            bool applyDamageToExplosionCellsNeighbors = def.projectile.applyDamageToExplosionCellsNeighbors;
            float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
            bool explosionDamageFalloff = def.projectile.explosionDamageFalloff;
            float? direction = origin.AngleToFlat(destination);
            float expolosionPropagationSpeed = damageDef.expolosionPropagationSpeed;
            float screenShakeFactor = def.projectile.screenShakeFactor;
            bool doExplosionVFX = def.projectile.doExplosionVFX;
            ThingDef preExplosionSpawnSingleThingDef = def.projectile.preExplosionSpawnSingleThingDef;
            ThingDef postExplosionSpawnSingleThingDef = def.projectile.postExplosionSpawnSingleThingDef;
            GenExplosionDW.DoExplosionNoFriendlyFire(position, map, explosionRadius, damageDef, instigator, damageAmount, armorPenetration, soundExplode, weapon, projectile, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, null, 255, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, explosionChanceToStartFire, explosionDamageFalloff, direction, null, null, doExplosionVFX, expolosionPropagationSpeed, 0f, doSoundEffects: true, postExplosionSpawnThingDefWater, screenShakeFactor, null, null, postExplosionSpawnSingleThingDef, preExplosionSpawnSingleThingDef);
            if (def.projectile.explosionSpawnsSingleFilth && def.projectile.filth != null && def.projectile.filthCount.TrueMax > 0 && Rand.Chance(def.projectile.filthChance) && !base.Position.Filled(map))
            {
                FilthMaker.TryMakeFilth(base.Position, map, def.projectile.filth, def.projectile.filthCount.RandomInRange);
            }
        }

        protected void Bounce(Vector3 startpoint)
        {
            float radius = Rand.Range(0.5f, 2f);
            lastBounceRadius = radius;
            float angle = Rand.Range(float.Epsilon, 80f);
            Vector3 bounceDirection =  Quaternion.AngleAxis(Rand.Range(-angle,angle)  , Vector3.up) * (origin - destination).normalized;
            bounceDest = startpoint + bounceDirection * radius;
        }
    }
}
