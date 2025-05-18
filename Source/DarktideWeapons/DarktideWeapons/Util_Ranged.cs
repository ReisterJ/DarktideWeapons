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

        public const float Intercept_PawnBodySize_Factor = 0.75f;
        public const float Intercept_PawnPosture_Downed_Factor = 0.2f;

        public const float CoverHitFactor_NotCloseToTarget = 0.15f;
        public const float CoverHitFactor_CloseToTarget = 1f;
        public const float CoverPenetrationBaseChance = 0.85f;

        public const float MinFillPercentCountAsCover = 0.3f;

        public const float HeadHuntBaseChance = 0.1f;
        public const float HeadHuntShootLevelBonusConstant = 2f;
        public const int MinHeadHuntShootLevel = 5;

        public enum PlasmaWeaponMode
        {
            Normal,
            Cooling,
            Charged,
        }

       

        public static bool IsStun(float chance)
        {
            if (Rand.Chance(chance))
            {
                return true;
            }
            return false;
        }
        public static bool CanBeStunned(Pawn victim, float stoppingPower)
        {          
             if(victim != null)
             {
                    //victim.stances
                    
                    if (!victim.DeadOrDowned  && victim.BodySize <= stoppingPower )
                    {
                        Pawn_StanceTracker stance = victim.stances;
                        if (stance != null)
                        {
                            return true;
                        }
                    }
                   
                } 
            return false;
        }

        public static List<IntVec3> GetLineSegmentCells(IntVec3 origin, IntVec3 dest, float range)
        {
            List<IntVec3> cells = new List<IntVec3>();

            Vector3 direction = (dest - origin).ToVector3().normalized;

            Vector3 endPoint = origin.ToVector3() + direction * range;

            foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(origin, endPoint.ToIntVec3()))
            {
                cells.Add(cell);
            }

            return cells;
        }

        public static void DEV_output(object o)
        {
#if DEBUG
            Log.Message("Ranged | " + o);
#endif
        }
    }
}
