using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace DarktideWeapons.RangedComps
{
    public class Comp_TorchUnderBarrel : DW_WeaponComp
    {
        public CompProperties_TorchUnderBarrel Props => (CompProperties_TorchUnderBarrel)props;
    }

    public class CompProperties_TorchUnderBarrel : CompProperties
    {
        public CompProperties_TorchUnderBarrel()
        {
            this.compClass = typeof(Comp_TorchUnderBarrel);
        }
        public float lightRadius = 2f;
        public float lightIntensity = 1f;
        public bool onlyInDarkness = true;
    }
}
