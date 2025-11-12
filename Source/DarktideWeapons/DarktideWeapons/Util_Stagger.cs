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
        public const int baseBlockBreakStunDuration = 30;

        public const int baseStunTick = 120;
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


        public static void StunHandler(Pawn pawn , int ticks , Thing instigator)
        {
            if (pawn != null && !pawn.Dead && pawn.Spawned)
            {
                pawn.stances.stunner.StunFor(ticks, instigator);
            }
        }
        public static void StaggerHandler(Pawn pawn , int ticks,Thing instigator, float staggerlevel = 1f)
        {
            if(staggerlevel > pawn.BodySize * 2)
            {
                StunHandler(pawn, StunTicks(pawn,staggerlevel), instigator);
                return;
            }
            if (pawn != null && !pawn.Dead && pawn.Spawned)
            {
                pawn.stances.stagger.StaggerFor(ticks);
            }
        }

        public static int StaggerTicks(Pawn hitpawn,float staggerlevel)
        {
            int tick = staggerlevel > hitpawn.BodySize ?(int) (baseStaggerTick * staggerlevel) :(int) (baseStaggerTick / (1f + hitpawn.BodySize - staggerlevel));
            return tick;
        }
        public enum StaggerLevel
        {
            None,
            Light,
            Medium,
            Heavy,
            Stunned
        }

        public static bool IsStaggered(Pawn pawn)
        {
            if (pawn != null && pawn.stances != null)
            {
                return pawn.stances.stagger.Staggered || pawn.stances.stunner.Stunned;
            }
            return false;
        }
        public static int StunTicks(Pawn hitpawn ,float level)
        {
            int stunticks = Math.Min((int)(baseStunTick * (1f + level - hitpawn.BodySize)), baseStunTick * 4);
            return baseStunTick;
        }
    }
}
