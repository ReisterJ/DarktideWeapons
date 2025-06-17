using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace DarktideWeapons
{
    public class Hediff_StickBomb : HediffWithComps
    {

        public int explodeTicks = 120;
        public int counter = 0;
        protected bool exploded;


        public float bombDamage = 20f;
        public float bombRadius = 2f;
        public float bombArmorPenetration = 0.2f;
        public override void Tick()
        {
            base.Tick();
            counter++;
            if (counter >= explodeTicks)
            {
                exploded = true;
                GenExplosionDW.DoExplosionNoFriendlyFire(pawn.Position, this.pawn.Map, bombRadius, DamageDefOf.Bomb, null, (int)bombDamage, bombArmorPenetration);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.counter, "counter", 120);
        }
    }

    
}
