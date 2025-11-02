using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
namespace DarktideWeapons.MeleeComps
{
    public class Comp_SpecialMelee : DW_WeaponComp
    {
        public CompProperties_SpecialMelee Props => (CompProperties_SpecialMelee)props;
    }

    public class CompProperties_SpecialMelee : CompProperties
    {
        public CompProperties_SpecialMelee()
        {
            this.compClass = typeof(Comp_SpecialMelee);
        }

        public Tool specialAttackToolPrimary;
    }   
}
