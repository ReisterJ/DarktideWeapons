using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class DW_SoulBlaze : DW_FireCloud
    {
        protected override DamageInfo CalculateDamage(Thing hitThing)
        {
            bool instigatorGuilty = !(launcher is Pawn pawn) || !pawn.Drafted;
            float damageAmount = this.DamageAmount * RangedDamageMultiplierGlobal;
            float armorPenetration = this.armorPenetrationinGame;
            if (critFlag)
            {
                damageAmount *= this.critDamageMultiplierinGame;
                armorPenetration *= this.critArmorPenetrationMultiplier;
            }
            damageAmount *= DamageMultiplier_Outer;
            
            BodyPartRecord bodyPart = null;
            DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, damageAmount, armorPenetration, ExactRotation.eulerAngles.y, launcher, bodyPart, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            return dinfo;
        }
    }
}
