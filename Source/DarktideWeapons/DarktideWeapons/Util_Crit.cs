using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public static class Util_Crit
    {
        public static bool IsCrit(float chance)
        {
            if (Rand.Chance(chance))
            {
                return true;
            }
            return false;
        }
        public static void DoCrit(ref DamageInfo dinfo)
        {
            dinfo.SetAmount(dinfo.Amount);
        }
        
        public static void CritMoteMaker(Thing hitThing)
        {
            MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "Crit", 2f);
        }
    }
}
