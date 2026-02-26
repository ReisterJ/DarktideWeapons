using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;


namespace DarktideWeapons
{
    public class Comp_Block : DW_WeaponComp
    {
        public bool isShield = false;

        protected float staminaMax = 100f;

        public float staminaCurrent;

        public float basePushStaminaCost => Props.pushStaminaCost;

        public float basePushPower => Props.pushPower;

        public float staminaRegenRate => Props.staminaRegenRate;
        public bool isBlocking = false;
        public new CompProperties_Block Props => (CompProperties_Block)props;

        private int blockTick = -1;
        public virtual float GetAdjustedMaxStamina() {

            return staminaMax;
        }
        public override void CompTickInterval(int delta)
        {
            if(staminaCurrent < staminaMax)
            {
                staminaCurrent += (delta * StaminaRegenRate());
            }
            if(staminaCurrent > staminaMax)
            {
                staminaCurrent = staminaMax;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref isShield, "isShield",false);
            Scribe_Values.Look(ref isBlocking, "isBlocking",false);
            Scribe_Values.Look(ref staminaCurrent, "staminaCurrent", Props.maxStamina);
            Scribe_Values.Look(ref staminaMax, "staminaMax", Props.maxStamina);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            staminaCurrent = Props.maxStamina;
            staminaMax = Props.maxStamina;
            isShield = Props.isShield;
        }

        public void ToggleBlock()
        {
            isBlocking = !isBlocking;
            if (isBlocking)
            {
                blockTick = Find.TickManager.TicksGame;
                return;
            }
            blockTick = -1;
        }
        public bool IsPerfectBlock()
        {
            if(blockTick < 0)
            {
                return false;
            }
            int ticksSinceBlock = Find.TickManager.TicksGame - blockTick;
            if(ticksSinceBlock <= 45)
            {
                return true;
            }
            return false;
        }
      
        public virtual bool AllowAttackWhileBlocking()
        {
            if (Props.allowAttackWhileBlocking)
            {
                return true;
            }
            return false;
        }

        public virtual void BreakBlock(DamageInfo dinfo)
        {
            isBlocking = false;
            staminaCurrent = 0f;
            if(PawnOwner != null)
            {
                Util_Stagger.StunHandler(PawnOwner, Util_Stagger.baseBlockBreakStunDuration, dinfo.Instigator);
            }
            
        }

        protected void PlayBlockSound(DamageInfo dinfo)
        {
            EffecterDef effecterDef = (Util_Melee.IsMeleeDamage(dinfo)) ? EffecterDefOf.Deflect_Metal : EffecterDefOf.Deflect_Metal_Bullet;
            if (PawnOwner.health.deflectionEffecter == null || PawnOwner.health.deflectionEffecter.def != effecterDef)
            {
                if (PawnOwner.health.deflectionEffecter != null)
                {
                    PawnOwner.health.deflectionEffecter.Cleanup();
                    PawnOwner.health.deflectionEffecter = null;
                }
                PawnOwner.health.deflectionEffecter = effecterDef.Spawn();
            }
            TargetInfo targetInfo = new TargetInfo(PawnOwner.Position, PawnOwner.MapHeld);
            Effecter deflectionEffecter = PawnOwner.health.deflectionEffecter;
            Thing instigator = dinfo.Instigator;
            deflectionEffecter.Trigger(targetInfo, (instigator != null) ? ((TargetInfo)instigator) : targetInfo);
            ImpactSoundUtility.PlayImpactSound(PawnOwner, dinfo.Def.impactSoundType, PawnOwner.MapHeld);
        }
        public bool TryBlockDamage(DamageInfo dinfo)
        {
            if (!CanBlockDamageType(dinfo))
            {
                return false;
            }
            if (!TryBlockRanged(dinfo))
            {
                return false;
            }
            if (isBlocking && staminaCurrent > 0)
            {
                staminaCurrent -= StaminaCost(dinfo);
                if (staminaCurrent < 0)
                {
                    BreakBlock(dinfo);
                }
                PlayBlockSound(dinfo);
                return true; 
            }
            return false;
        }
        protected virtual bool CanBlockDamageType(DamageInfo dinfo)
        {
            if (Util_Melee.IsMeleeDamage(dinfo) || Util_Ranged.IsRangedAttack(dinfo))
            {
                if (this.isShield)
                {
                    return true;
                }
                else
                {
                    if(Util_Ranged.IsRangedAttack(dinfo))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        protected bool TryBlockRanged(DamageInfo dinfo)
        {
            if (PawnOwner != null && Util_Ranged.IsRangedAttack(dinfo) && isShield && isBlocking)
            {
                if(dinfo.Instigator == null)
                {
                    return true;
                }
                if (PawnOwner.stances.curStance is Stance_Busy stance_Busy && stance_Busy.focusTarg.IsValid)
                {
                    IntVec3 pawnDirection = stance_Busy.focusTarg.Cell - PawnOwner.Position;
                    IntVec3 bulletDirection = dinfo.Instigator.Position - PawnOwner.Position;

                    float angle = Vector3.Angle(pawnDirection.ToVector3(), bulletDirection.ToVector3());
                    if (angle > 90f)
                    {
                        return false; 
                    }
                    return true;
                }
            }
            return true;
        }
        public virtual float StaminaCost(DamageInfo dinfo)
        {
            if(dinfo.Amount <= float.Epsilon)
            {
                return 0f;
            }
            float cost = dinfo.Amount;

            if (Util_Melee.IsMeleeDamage(dinfo))
            {
                cost = cost * (1f - GetMeleeBlockingEfficiency());
            }
            else if (dinfo.Def == DamageDefOf.Bullet)
            {
                cost = cost * (1f - GetRangedBlockingEfficiency());
            }
            
            return cost;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.allowBlock)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "DW_ToggleBlock",
                    defaultDesc = "DW_ToggleBlockDesc",
                    isActive = () => isBlocking,
                    toggleAction = ToggleBlock,
                    icon = TexCommand.DesirePower
                };
            }
            yield return new Gizmo_BlockStaminaStatus(this);
        }

        public float GetMeleeBlockingEfficiency()
        {
            float efficiency = Props.meleeBlockingEfficiency;
            return Mathf.Min(efficiency,0.9f);
        }
        public float GetRangedBlockingEfficiency()
        {
            float efficiency = Props.rangedBlockingEfficiency;
            return Mathf.Min(efficiency, 0.99f);
        }
       

        public virtual float StaminaRegenRate()
        {
            return staminaRegenRate; 
        }

       

        
    }

    public class CompProperties_Block : CompProperties
    {
        public float maxStamina = 100f;

        public List<HediffDef> shieldHediffs = new List<HediffDef>();

        public bool isShield = false;

        public float meleeBlockingEfficiency = 0.3f;

        public float rangedBlockingEfficiency = 0.5f;

        public bool allowAttackWhileBlocking = false;
            
        public float staminaRegenRate = 0.1f;

        public float pushPower = 1f;

        public float pushStaminaCost = 10f;

        public bool allowBlock = false;
        public CompProperties_Block ()
        {
            this.compClass = typeof(Comp_Block);
        }

    }

    [StaticConstructorOnStartup]
    public class Gizmo_BlockStaminaStatus : Gizmo
    {
        public Comp_Block compBlock;
        protected static readonly Texture2D FullStaminaBarTex = SolidColorMaterials.NewSolidColorTexture(Color.green);
        protected static readonly Texture2D EmptyStaminaBarTex = SolidColorMaterials.NewSolidColorTexture(Color.gray);
        protected static Color StaminaBarColor = Color.green;

        public Gizmo_BlockStaminaStatus(Comp_Block comp)
        {
            compBlock = comp;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(overRect);
            Rect labelRect = new Rect(overRect.x + 10f, overRect.y + 5f, overRect.width - 20f, 20f);
            Rect barRect = new Rect(overRect.x + 10f, overRect.y + 45f, overRect.width - 20f, 20f);
            string wielderlabel = compBlock.PawnOwner != null ? compBlock.PawnOwner.Label : "";
            Widgets.Label(labelRect, wielderlabel + " " + "DW_Stamina".Translate().Resolve() );
            float fillPercent = compBlock.staminaCurrent / compBlock.GetAdjustedMaxStamina();
            Widgets.FillableBar(barRect, fillPercent, FullStaminaBarTex, EmptyStaminaBarTex, false);

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
