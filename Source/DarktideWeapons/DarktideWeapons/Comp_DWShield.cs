using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DWShield : DW_WeaponComp
    {
        public bool isShieldUp = false;
        public float staminaCurrent;
        public new CompProperites_DWShield Props => (CompProperites_DWShield)props;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref isShieldUp, "isShieldUp");
            Scribe_Values.Look(ref staminaCurrent, "staminaCurrent", Props.maxStamina);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            staminaCurrent = Props.maxStamina;
        }

        // Ability切换举盾状态
        public void ToggleShield()
        {
            isShieldUp = !isShieldUp;
            Pawn pawn = this.parent as Pawn;
            if (pawn != null)
            {
                if (isShieldUp)
                {
                    // 添加hediff
                    if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("DWShield_Active")))
                        pawn.health.AddHediff(HediffDef.Named("DWShield_Active"));
                }
                else
                {
                    // 移除hediff
                    pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("DWShield_Active")));
                }
            }
        }

        public void BreakBlock()
        {

        }

        public bool TryBlockDamage(float damage)
        {
            if (isShieldUp && staminaCurrent > 0)
            {
                staminaCurrent -= StaminaCost(damage);
                if (staminaCurrent < 0) staminaCurrent = 0;
                return true; // 格挡成功
            }
            return false;
        }
        public virtual float StaminaCost(float damage)
        {
            float cost = damage;
            return cost;
        }

        // 绘制体力条
        public override void PostDraw()
        {
            base.PostDraw();
            if (isShieldUp)
            {
                Vector3 drawPos = parent.DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                GenDraw.FillableBarRequest req = new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = new Vector2(1.2f, 0.2f),
                    fillPercent = staminaCurrent / Props.maxStamina,
                    filledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.green),
                    unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.gray)
                };
                GenDraw.DrawFillableBar(req);
            }
        }

        public virtual float StaminaRegenRate()
        {
            return 1f; 
        }

        public override void CompTickInterval(int delta)
        {
            staminaCurrent += (delta * StaminaRegenRate());
        }
    }

    public class CompProperites_DWShield : CompProperties
    {
        public float maxStamina = 100f;

        public List<HediffDef> shieldHediffs = new List<HediffDef>();
        public CompProperites_DWShield()
        {
            this.compClass = typeof(Comp_DWShield);
        }
    }
}
