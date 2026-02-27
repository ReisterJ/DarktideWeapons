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

        // 覆盖 CurStageIndex，使 level 值与 minSeverity 对应
        // level 1 对应 minSeverity 1 的 stage（index 0）
        public override int CurStageIndex
        {
            get
            {
                if (this.def.stages == null)
                    return 0;
                // level - 1 是因为 stages 数组从 0 开始，而 minSeverity 从 1 开始
                int index = Math.Max(0, this.level - 1);
                return Math.Min(index, this.def.stages.Count - 1);
            }
        }

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
            return this.level * DamageBonusPerLevel * DamageMultiplier_Global;
        }


        protected virtual void DOTDamage()
        {
            float ap = this.ArmorPenetrationBase;
            DamageInfo dinfo = new DamageInfo(damageDefOf, GetDamage(), ap);
            dinfo.SetIgnoreArmor(this.IgnoreArmor);
            pawn.TakeDamage(dinfo);
        }
        public override void TickInterval(int delta)
        {
            if (!this.pawn.Spawned || this.pawn.Dead)
            {
                return;
            }
            damageTick += delta;
            levelkeepcheck += delta;
            if (damageTick % DamageTickPeriod == 0)
            {
                DOTDamage();
            }
            if (levelkeepcheck / 60 >= Extension_HediffDOT.downgradeDamageTime)
            {
                levelkeepcheck = 0;
                this.SetLevelTo(this.level - 1);
            }
        }
        public void RefreshDOT()
        {
            //damageTick = 0;
            levelkeepcheck = 0;
        }
    }
}
