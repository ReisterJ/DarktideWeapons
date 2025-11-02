using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_ProjectileProperties: DefModExtension 
    {
        public float critChance = 0.05f;
        public float critDamageMultiplier = 1.5f;
        public float stunChance = 0f;
        public int stunTicks = 30;

        //public bool isGrenade = false;
        public float critArmorPenetrationMultiplier = 2.0f;

        public float effectiveRange = 25f;
        public bool penetrateWall = false;
        public int penetrationPower = 0;    

        public bool friendlyFire = false;

        public float weaknessDamageMultiplier = 2.0f;

        public bool isExplosive = false;
        public float explosionDamage = 20f;
        public float explosionRadius = 2f;
        public float explosionArmorPenetration = 0.2f;

        public bool isStickyBomb = false;

        public bool damageFalloffByRange = false;

        public float minRangeStartFalloff = 25f;
        

        public DamageDef explosionDamageDef;

        // for laser and plasma
        public ThingDef beamMoteDef;
        
        public List<HediffDef> applyHediffDefs = new List<HediffDef>();
    }

   
    /*
    public class ModExtension_ShotgunSlug: DefModExtension
    {
        public int stunTicksSlug = 120;

    }
    */
}
