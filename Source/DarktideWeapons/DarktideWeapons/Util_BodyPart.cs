using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public static class Util_BodyPart
    {
        public static BodyPartRecord GetHeadPart(Pawn pawn)
        {
            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == BodyPartDefOf.Head)
                {
                    return notMissingPart;
                }
            }
            return null;
        }
        public static BodyPartRecord GetTorsoPart(Pawn pawn)
        {
            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == BodyPartDefOf.Torso)
                {
                    return notMissingPart;
                }
            }

            return null;
        }
    }
}
