using RimWorld;
using Verse;

namespace DarktideWeapons
{
    [DefOf]
    public static class DW_JobDefOf
    {
        public static JobDef DW_UseOmniBlessing;

        static DW_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DW_JobDefOf));
        }
    }
}
