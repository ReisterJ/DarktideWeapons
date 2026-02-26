using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.MeleeComps
{
    public class Comp_MeleeApplyHediffs: DW_WeaponComp
    {
        public CompProperties_MeleeApplyHediffs Props => (CompProperties_MeleeApplyHediffs)props;
    }

    public class CompProperties_MeleeApplyHediffs : CompProperties
    {
        public CompProperties_MeleeApplyHediffs()
        {
            this.compClass = typeof(Comp_MeleeApplyHediffs);
        }
        public List<HediffDef> hediffsToApply;
    }
}
