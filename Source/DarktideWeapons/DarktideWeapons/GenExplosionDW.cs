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
    public static class GenExplosionDW
    {
        private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);

        private static List<Room> exploderOverlapRooms = new List<Room>();

        public static void DoExplosionNoFriendlyFire(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, GasType? postExplosionGasType = null, float? postExplosionGasRadiusOverride = null, int postExplosionGasAmount = 255, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null, FloatRange? affectedAngle = null, bool doVisualEffects = true, float propagationSpeed = 1f, float excludeRadius = 0f, bool doSoundEffects = true, ThingDef postExplosionSpawnThingDefWater = null, float screenShakeFactor = 1f, SimpleCurve flammabilityChanceCurve = null, List<IntVec3> overrideCells = null, ThingDef postExplosionSpawnSingleThingDef = null, ThingDef preExplosionSpawnSingleThingDef = null)
        {
            if (map == null)
            {
                Log.Warning("Tried to do explosion in a null map.");
                return;
            }
            if (damAmount < 0)
            {
                damAmount = damType.defaultDamage;
                armorPenetration = damType.defaultArmorPenetration;
                if (damAmount < 0)
                {
                    Log.ErrorOnce("Attempted to trigger an explosion without defined damage", 91094882);
                    damAmount = 1;
                }
            }
            if (armorPenetration < 0f)
            {
                armorPenetration = (float)damAmount * 0.015f;
            }
            Explosion obj = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
            IntVec3? needLOSToCell = null;
            IntVec3? needLOSToCell2 = null;
            if (direction.HasValue)
            {
                CalculateNeededLOSToCells(center, map, direction.Value, out needLOSToCell, out needLOSToCell2);
            }
           
            if(ignoredThings == null)
            {
                ignoredThings = new List<Thing>();
            }
            foreach (Pawn pawn in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                ignoredThings.Add(pawn);
            }
            obj.radius = radius;
            obj.damType = damType;
            obj.instigator = instigator;
            obj.damAmount = damAmount;
            obj.armorPenetration = armorPenetration;
            obj.weapon = weapon;
            obj.projectile = projectile;
            obj.intendedTarget = intendedTarget;
            obj.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
            obj.preExplosionSpawnChance = preExplosionSpawnChance;
            obj.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
            obj.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
            obj.postExplosionSpawnThingDefWater = postExplosionSpawnThingDefWater;
            obj.postExplosionSpawnChance = postExplosionSpawnChance;
            obj.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
            obj.postExplosionGasType = postExplosionGasType;
            obj.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            obj.chanceToStartFire = chanceToStartFire;
            obj.damageFalloff = damageFalloff;
            obj.needLOSToCell1 = needLOSToCell;
            obj.needLOSToCell2 = needLOSToCell2;
            obj.excludeRadius = excludeRadius;
            obj.affectedAngle = affectedAngle;
            obj.doSoundEffects = doSoundEffects;
            obj.screenShakeFactor = screenShakeFactor;
            obj.flammabilityChanceCurve = flammabilityChanceCurve;
            obj.doVisualEffects = doVisualEffects;
            obj.propagationSpeed = propagationSpeed;
            obj.overrideCells = overrideCells;
            obj.StartExplosion(explosionSound, ignoredThings);
        }

        private static void CalculateNeededLOSToCells(IntVec3 position, Map map, float direction, out IntVec3? needLOSToCell1, out IntVec3? needLOSToCell2)
        {
            needLOSToCell1 = null;
            needLOSToCell2 = null;
            if (position.CanBeSeenOverFast(map))
            {
                return;
            }
            direction = GenMath.PositiveMod(direction, 360f);
            IntVec3 intVec = position;
            intVec.z++;
            IntVec3 intVec2 = position;
            intVec2.z--;
            IntVec3 intVec3 = position;
            intVec3.x--;
            IntVec3 intVec4 = position;
            intVec4.x++;
            if (direction < 90f)
            {
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec3;
                }
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec;
                }
            }
            else if (direction < 180f)
            {
                if (intVec.InBounds(map) && intVec.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec;
                }
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec4;
                }
            }
            else if (direction < 270f)
            {
                if (intVec4.InBounds(map) && intVec4.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec4;
                }
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec2;
                }
            }
            else
            {
                if (intVec2.InBounds(map) && intVec2.CanBeSeenOverFast(map))
                {
                    needLOSToCell1 = intVec2;
                }
                if (intVec3.InBounds(map) && intVec3.CanBeSeenOverFast(map))
                {
                    needLOSToCell2 = intVec3;
                }
            }
        }

        public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius, Color color)
        {
            GenDraw.DrawFieldEdges(DamageDefOf.Bomb.Worker.ExplosionCellsToHit(loc, Find.CurrentMap, radius, null, null, null).ToList(), color, null);
        }

     }
}
