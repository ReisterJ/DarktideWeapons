using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Hediff_DOT : Hediff_Level
    {
        protected int damageTick = 0;

        public int DamageTickPeriod => Extension_HediffDOT.damageTickPeriod;

        public float DamageBonusPerLevel => Extension_HediffDOT.damageBonusPerLevel;

        public DamageDef damageDefOf => Extension_HediffDOT.damageDefOf;

        public float damageMultiplier = 1.0f;

        public float ArmorPenetrationBase => Extension_HediffDOT.armorPenetrationBase;

        public bool IgnoreArmor => Extension_HediffDOT.ignoreArmor;

        public float DamageMultiplier_Global => LoadedModManager.GetMod<DW_Mod>().GetSettings<DW_ModSettings>().RangedDamageMultiplierGlobal;
        public HediffComp_Disappears CompDisappears => this.TryGetComp<HediffComp_Disappears>();

        public ModExtension_HediffDOT Extension_HediffDOT => this.def.GetModExtension<ModExtension_HediffDOT>();

        protected int levelkeepcheck = 0;
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
            return this.level * this.level / 2 * DamageBonusPerLevel * DamageMultiplier_Global;
        }

        public override void Tick()
        {
            if (this.pawn.Dead)
            {
                return;
            }
            damageTick ++;
            if(damageTick % DamageTickPeriod == 0)
            {
                float ap = this.ArmorPenetrationBase;
                DamageInfo dinfo = new DamageInfo(damageDefOf, GetDamage(), ap);
                dinfo.SetIgnoreArmor(this.IgnoreArmor);
                pawn.TakeDamage(dinfo);
                levelkeepcheck++;
            }
            if(levelkeepcheck >= Extension_HediffDOT.downgradeDamageTime)
            {
                levelkeepcheck = 0;
                this.SetLevelTo(this.level - 1);
            }
        }
    }
}
