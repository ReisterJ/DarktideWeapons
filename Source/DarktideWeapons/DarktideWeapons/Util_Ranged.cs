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
    public static class Util_Ranged
    {
        public const float Quality_Excellent_Multiplier = 1.1f;
        public const float Quality_Master_Multiplier = 1.25f;
        public const float Quality_Legendary_Multiplier = 1.5f;
        public const float Quality_Legendary_Stun_Tick_Multiplier = 2f;
        public const int SlugStunTicks = 120;

        public const float PenetrateWall_Probability_Base = 0.001f;

        public const float Intercept_PawnBodySize_Factor = 0.9f;
        public const float Intercept_PawnPosture_Downed_Factor = 0.1f;

        public const float CoverHitFactor_NotCloseToTarget = 0.01f;
        public const float CoverHitFactor_CloseToTarget = 1f;
        public const float CoverPenetrationBaseChance = 0.85f;

        public const float MinFillPercentCountAsCover = 0.3f;

        public const float HeadHuntBaseChance = 0.075f;
        public const float HeadHuntShootLevelBonusConstant = 2f;
        public const int MinHeadHuntShootLevel = 5;

        public const float MarksmanBase = 0.2f;
        public const float MarksmanShootLevelBonusConstant = 0.0015f;

        public const float MinFlammabilityForDamage = 0.1f;
        public enum PlasmaWeaponMode
        {
            Normal,
            Charged,
            Cooling
        }
        public const int PLASMA_SELFCOOLING_TICKS = 150; // 2.5 seconds
       

        

        public static List<IntVec3> GetLineSegmentCells(IntVec3 origin, IntVec3 dest, float range ,Map map )
        {
            List<IntVec3> cells = new List<IntVec3>();

            Vector3 direction = (dest - origin).ToVector3().normalized;

            Vector3 endPoint = origin.ToVector3() + direction * range;

            foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(origin, endPoint.ToIntVec3()))
            {
                if(cell.InBounds(map))  cells.Add(cell);
            }

            return cells;
        }



        public static float HeadHuntChanceCalculation(int shootlevel)
        {
            float chance = Util_Ranged.HeadHuntBaseChance;
            chance = shootlevel > Util_Ranged.MinHeadHuntShootLevel ?
                chance * (Mathf.Pow(Util_Ranged.HeadHuntShootLevelBonusConstant, (float)shootlevel / 10f) * Util_Ranged.HeadHuntShootLevelBonusConstant)
                : chance;
            return chance;
        }

        public static float MarksmanFunc(int shootlevel)
        {
            float chance = Util_Ranged.MarksmanBase;
            chance += (float)shootlevel * (float)shootlevel * MarksmanShootLevelBonusConstant;
            return chance;
        }

        public static List<IntVec3> GetCellsWithinRadius(IntVec3 root, float radius, Map map)
        {
            List<IntVec3> area = new List<IntVec3>();
            int minx = root.x - (int)radius;
            int maxx = root.x + (int)radius;
            int minz = root.z - (int)radius;
            int maxz = root.z + (int)radius;
            for (int x = minx; x <= maxx; x++)
            {
                for (int z = minz; z <= maxz; z++)
                {
                    IntVec3 cell = new IntVec3(x, root.y, z);
                    if (cell.InBounds(map)) //&& cell.DistanceTo(root) <= radius)
                    {
                        area.Add(cell);
                    }
                }
            }
            return area;
        }

        public static void Ranged_Stats(Pawn pawn)
        {

        }
        public static void DEV_output(object o,int level = 0)
        {
#if DEBUG
            Log.Message("Ranged | " + o);
#endif
        }
    }
}
