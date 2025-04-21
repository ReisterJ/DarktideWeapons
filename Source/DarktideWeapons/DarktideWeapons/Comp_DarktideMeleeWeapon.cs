using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DarktideMeleeWeapon : ThingComp
    {
        protected int counter = 0;

        protected Comp_DWToughnessShield comp_DWToughnessShield;
        public CompProperties_DarktideMeleeWeapon Props
        {
            get
            {
                return (CompProperties_DarktideMeleeWeapon)props;
            }
        }

        public int cleaveTargetsinGame;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            cleaveTargetsinGame = Props.cleaveTargets;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

        }

        public override void CompTick()
        {
            base.CompTick();
            counter++;
            if(counter >= 300)
            {
                //Util_Melee.DEV_output(this.parent + " is ticking");
                //Util_Melee.DEV_output("Cleave Target : " + this.cleaveTargetsinGame);
                counter = 0;
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            
        }

    }

    public class CompProperties_DarktideMeleeWeapon : CompProperties
    {
        public CompProperties_DarktideMeleeWeapon()
        {
            this.compClass = typeof(Comp_DarktideMeleeWeapon);
        }

        public int cleaveTargets = 0;


    }
}
