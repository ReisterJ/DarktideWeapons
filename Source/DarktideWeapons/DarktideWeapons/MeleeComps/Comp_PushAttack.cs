using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace DarktideWeapons.MeleeComps
{
    public class Comp_PushAttack : DW_WeaponComp
    {
        public CompProperties_PushAttack Props => (CompProperties_PushAttack)props;
    }
    
    public class CompProperties_PushAttack : CompProperties
    {
        public CompProperties_PushAttack()
        {
            this.compClass = typeof(Comp_PushAttack);
        }

        public Tool pushAttackToolPrimary;

    }
}
