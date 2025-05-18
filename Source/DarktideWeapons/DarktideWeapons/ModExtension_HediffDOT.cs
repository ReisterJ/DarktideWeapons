using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_HediffDOT :DefModExtension
    {
        public int damageTickPeriod = 60;

        public float damageBonusPerLevel = 0.5f;

        public DamageDef damageDefOf;

        public float damageMultiplier = 1.0f;

        public float armorPenetrationBase = 0.3f;

        public bool ignoreArmor = false;

        public int downgradeDamageTime = 4;

        public bool isBrain = false;
    }
}
