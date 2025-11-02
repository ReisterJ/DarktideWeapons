using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons.MeleeComps
{
    public class Comp_HeavyAttack : DW_WeaponComp
    {
        public float cleaveMultiplier = 1.5f;

        public float staggerlevel = 1.5f;

        public float damageMultiplier = 2f;

        public float armorPenetrationMultiplier = 1.5f;

        public CompProperties_HeavyAttack Props => (CompProperties_HeavyAttack)props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            cleaveMultiplier = Props.cleaveMultiplier;
            staggerlevel = Props.staggerlevel;
            damageMultiplier = Props.damageMultiplier;
            armorPenetrationMultiplier = Props.armorPenetrationMultiplier;
        }
    }

    public class CompProperties_HeavyAttack : CompProperties
    {
        public float cleaveMultiplier = 1.5f;

        public float staggerlevel = 1.5f;

        public float damageMultiplier = 2f;

        public float armorPenetrationMultiplier = 1.5f;
        public CompProperties_HeavyAttack()
        {
            this.compClass = typeof(Comp_HeavyAttack);
        }
        public Tool heavyAttackToolPrimary;
            
    }
}
