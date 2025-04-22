using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DarktideRangedWeapon : ThingComp
    {

        public CompProperties_DarktideRangedWeapon Props
        {
            get
            {
                return (CompProperties_DarktideRangedWeapon)this.props;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
    }

    public class CompProperties_DarktideRangedWeapon : CompProperties
    {
        public CompProperties_DarktideRangedWeapon()
        {
            this.compClass = typeof(Comp_DarktideRangedWeapon);
        }

        public int penetrateTargets = 0;



    }
}
