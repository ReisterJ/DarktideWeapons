using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DarktideWeapons
{
    public class Verb_DW_PlasmaShoot : Verb_DW_Shoot
    {
        public override bool Available()
        {
            bool flag = base.Available();
            if (DarktideWeapon != null)
            {
                Comp_DarktidePlasma plasmaComp = DarktideWeapon.TryGetComp<Comp_DarktidePlasma>();
                if (plasmaComp != null)
                {
                    flag = plasmaComp.AllowShoot();
                }
            }
            return flag;
        }
    }

   
}
