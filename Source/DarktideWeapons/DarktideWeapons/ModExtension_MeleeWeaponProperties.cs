using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_MeleeWeaponProperties : DefModExtension
    {
        public int cleaveTargets = 2;
        public float cleaveDamageFalloffRatio = 0.90f;
        public Util_Melee.CraftType craftType;

        public float critChance = 0.05f;
        public float critDamageMultiplier = 2.0f;
        public float staggerImpact = 1f;
        public float critArmorPenetrationMultiplier = 2.0f;

        public SoundDef chargedHitSound;


        
    }
}
