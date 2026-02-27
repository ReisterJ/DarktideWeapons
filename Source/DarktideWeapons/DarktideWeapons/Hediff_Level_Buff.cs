using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Hediff_Level_Buff : Hediff_Level
    {
        public virtual bool DoDamage { get { return damage; } set { } }

        protected bool damage = false;

        protected int appliedTick = 0;

        public HediffComp_Disappears CompDisappears => this.TryGetComp<HediffComp_Disappears>();

        public ModExtension_HediffLevelBuff Extension_HediffLevelBuff => this.def.GetModExtension<ModExtension_HediffLevelBuff>();

        // 覆盖 CurStageIndex，使 level 值与 minSeverity 对应
        // level 1 对应 minSeverity 1 的 stage（index 0）
        // level 6 对应 minSeverity 6 的 stage（index 5）
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
        public override void TickInterval(int delta)
        {
            if (!this.pawn.Spawned || this.pawn.Dead)
            {
                return;
            }
            if (Extension_HediffLevelBuff == null)
            {
                return;
            }
            appliedTick += delta;

            if (appliedTick / 60 >= Extension_HediffLevelBuff.downgradeDamageTime)
            {
                this.SetLevelTo(this.level - 1);
                appliedTick = 0;
            }
        }
        public void RefreshBuff()
        {
            appliedTick = 0;
        }

    }
}
