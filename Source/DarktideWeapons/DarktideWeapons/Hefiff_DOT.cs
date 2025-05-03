using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Hefiff_DOT : Hediff_Level
    {
        protected int damageTick = 0;

        public int damageTickPeriod = 60;

        public float damageBonusPerLevel = 0.5f;

        public DamageDef damageDefOf = DamageDefOf.Blunt;

        public float damageMultiplier = 1.0f;

        public float armorPenetrationBase = 0.3f;
        public HediffComp_Disappears CompDisappears => this.TryGetComp<HediffComp_Disappears>();
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }
        
        public void SetRemainingTime(int remainingtick)
        {
            if (CompDisappears != null)
            {
                CompDisappears.ticksToDisappear = remainingtick;
            }
        }

        protected virtual float GetDamage()
        {
            return this.level * this.level / 2 * damageBonusPerLevel;
        }
        public override void Tick()
        {
            damageTick ++;
            if (CompDisappears != null)
            {
               
            }
            if(damageTick >= damageTickPeriod)
            {
                DamageInfo dinfo = new DamageInfo(damageDefOf,GetDamage());
                dinfo.SetIgnoreArmor(true);
                pawn.TakeDamage(dinfo);
            }
        }
    }
}
