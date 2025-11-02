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
    public class Comp_Block : DW_WeaponComp
    {
        public bool isShield = false;

        public float staminaCurrent;

        public bool isDefending = false;
        public new CompProperites_Block Props => (CompProperites_Block)props;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref isShield, "isShield");
            Scribe_Values.Look(ref isDefending, "isDefending");
            Scribe_Values.Look(ref staminaCurrent, "staminaCurrent", Props.maxStamina);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            staminaCurrent = Props.maxStamina;
        }

        // Ability切换举盾状态
        public void ToggleBlock()
        {
            isDefending = !isDefending;
            Pawn pawn = this.parent as Pawn;
            if (pawn != null)
            {
                if (isDefending)
                {
                   
                }
                else
                {
                  
                }
            }
        }

        public virtual void BreakBlock()
        {

        }

        public bool TryBlockDamage(float damage)
        {
            if (isDefending && staminaCurrent > 0)
            {
                staminaCurrent -= StaminaCost(damage);
                if (staminaCurrent < 0) staminaCurrent = 0;
                return true; 
            }
            return false;
        }
        public virtual float StaminaCost(float damage)
        {
            float cost = damage;
            return cost;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (isDefending)
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

    public class CompProperites_Block : CompProperties
    {
        public float maxStamina = 100f;

        public List<HediffDef> shieldHediffs = new List<HediffDef>();

        public bool isShield = false;
        public CompProperites_Block()
        {
            this.compClass = typeof(Comp_Block);
        }
    }
}
