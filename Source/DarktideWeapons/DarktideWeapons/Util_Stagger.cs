using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public static class Util_Stagger
    {
        public const int baseStaggerTick = 90;

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
            if (victim != null)
            {
                //victim.stances

                if (!victim.DeadOrDowned && victim.BodySize <= stoppingPower)
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
        public enum StaggerLevel
        {
            None,
            Light,
            Medium,
            Heavy,
            Stunned
        }
        public static int GetStaggerTick(int level)
        {
            if(level < 4)
            {
                return baseStaggerTick * ( level + 1 ); 
            }
            return baseStaggerTick;
        }
    }
}
