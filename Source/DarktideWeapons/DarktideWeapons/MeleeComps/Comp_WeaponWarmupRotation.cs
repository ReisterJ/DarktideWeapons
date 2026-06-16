using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarktideWeapons.SpecialMeleeVerbs;
using RimWorld;
using UnityEngine;
using Verse;

namespace DarktideWeapons.MeleeComps
{

    /// 武器前摇旋转动画组件 —— 在 ability warmup 期间缓慢向后旋转武器贴图，
    /// warmup 结束后快速向前挥击，模拟蓄力重击的视觉效果。

    public class Comp_WeaponWarmupRotation : DW_WeaponComp
    {
        private bool wasWarmup = false;
        private int warmupStartTick = 0;
        private int warmupTotalTicks = 0;
        private int lastTicksLeft = 0;

        private bool isStriking = false;
        private int strikeStartTick = 0;

        public CompProperties_WeaponWarmupRotation Props => (CompProperties_WeaponWarmupRotation)props;

        private int SnapDuration => Props.snapDurationTicks > 0 ? Props.snapDurationTicks : 8;
        private float WarmupRotateAngle => Props.warmupRotateAngle;
        private float StrikeRotateAngle => Props.strikeRotateAngle;

        /// 由 DW_Equipment.DrawAt() 调用，返回当前帧应叠加的旋转角度（度）。

        public float GetExtraRotation()
        {
            Pawn pawn = this.PawnOwner;
            if (pawn == null) return 0f;

            Stance curStance = pawn.stances?.curStance;
            if (curStance is Stance_Warmup stance && stance.verb is Verb_ThunderHammerStrike)
            {
                // 检测是否是新的一次 warmup
                if (!wasWarmup || stance.ticksLeft > lastTicksLeft)
                {
                    warmupStartTick = Find.TickManager.TicksGame;
                    warmupTotalTicks = stance.ticksLeft;
                    wasWarmup = true;
                    isStriking = false;
                }
                lastTicksLeft = stance.ticksLeft;

                float progress = 1f - (float)stance.ticksLeft / Mathf.Max(warmupTotalTicks, 1);
                return Mathf.Lerp(0f, WarmupRotateAngle, progress);
            }
            else if (wasWarmup)
            {
                // warmup 刚结束，触发挥击动画
                wasWarmup = false;
                isStriking = true;
                strikeStartTick = Find.TickManager.TicksGame;
            }

            if (isStriking)
            {
                int elapsed = Find.TickManager.TicksGame - strikeStartTick;
                if (elapsed < SnapDuration)
                {
                    float t = (float)elapsed / SnapDuration;
                    float easedT = 1f - Mathf.Pow(1f - t, 3f);
                    return Mathf.Lerp(WarmupRotateAngle, StrikeRotateAngle, easedT);
                }
                else
                {
                    isStriking = false;
                }
            }

            return 0f;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref wasWarmup, "wasWarmup");
            Scribe_Values.Look(ref warmupStartTick, "warmupStartTick");
            Scribe_Values.Look(ref warmupTotalTicks, "warmupTotalTicks");
            Scribe_Values.Look(ref lastTicksLeft, "lastTicksLeft");
            Scribe_Values.Look(ref isStriking, "isStriking");
            Scribe_Values.Look(ref strikeStartTick, "strikeStartTick");
        }
    }

    public class CompProperties_WeaponWarmupRotation : CompProperties
    {
        /// <summary>蓄力阶段向后旋转的角度（度），负值向后。</summary>
        public float warmupRotateAngle = -65f;

        /// <summary>挥击阶段向前旋转的角度（度），正值向前。</summary>
        public float strikeRotateAngle = 30f;

        /// <summary>挥击动画持续的 tick 数。</summary>
        public int snapDurationTicks = 6;

        public CompProperties_WeaponWarmupRotation()
        {
            this.compClass = typeof(Comp_WeaponWarmupRotation);
        }
    }
}
