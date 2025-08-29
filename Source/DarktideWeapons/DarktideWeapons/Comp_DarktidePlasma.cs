using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace DarktideWeapons
{
    public class Comp_DarktidePlasma : DW_WeaponComp
    {
        public float heat = 0f;

        public float overheatExplosionDamage = 50f;

        public float overheatExplosionRadius = 3f;

        public float heatGenMultiplier = 1f;

        public float maxHeat = 100f;

        public float coolingWeaponHeatLossRate = 0.05f;

        public float coolingWeaponHeatLossForced = 30f;

        public Util_Ranged.PlasmaWeaponMode plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;

        public float chargedModeDamageMultiplier = 2f;

        public float chargedModeArmorPenetrationMultiplier = 1.5f;

        protected Gizmo_PlasmaWeaponStatus plasmaWeaponStatus;

        public const float DangerHeatThreshold = 0.85f;

        protected bool safeMode = true;

        protected int ticksSinceLastShot = 150;
        public bool SafeMode
        {
            get { return safeMode; } 
        }

        private int tick = 0;
        public CompProperties_DarktidePlasma Props
        {
            get
            {
                return (CompProperties_DarktidePlasma)this.props;
            }
        }

        public void ResetLastShotTick()
        {
            ticksSinceLastShot = 0;
        }
        public override void CompTick()
        {
            base.CompTick();
            tick++;
            ticksSinceLastShot++;
            if (plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Cooling && heat > 0)
            {
                heat -= coolingWeaponHeatLossRate;
                if(heat < 0)
                {
                    heat = 0;
                }   
                
            }
            if(plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Cooling && heat <= 0)
            {
                plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;
            }
            if(plasmaWeaponMode != Util_Ranged.PlasmaWeaponMode.Cooling && heat > 0)
            {
                if(PawnOwner != null)
                {
                    if (PawnOwner.stances?.curStance is Stance_Warmup)
                    {
                        return;
                    }
                    if(ticksSinceLastShot >= Util_Ranged.PLASMA_SELFCOOLING_TICKS)
                    {
                        heat -= coolingWeaponHeatLossRate / 2;
                        if (heat < 0)
                        {
                            heat = 0;
                        }
                    }
                }
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.maxHeat = Props.maxHeat;
            this.overheatExplosionDamage = Props.overheatExplosionDamage;
            this.overheatExplosionRadius = Props.overheatExplosionRadius;
            this.coolingWeaponHeatLossRate = Props.coolingWeaponHeatLossRate;
            this.coolingWeaponHeatLossForced = Props.coolingWeaponHeatLossForced;
            this.heatGenMultiplier = 1f;
            this.heat = 0f;
        }

        public virtual void ForcedCooling()
        {
            if (this.heat >= 40f)
            {
                this.heat -= coolingWeaponHeatLossForced;
                if (this.heat < 0)
                {
                    this.heat = 0;
                }
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, Props.coolingWeaponHeatDamage, 0.05f, -1, null, null, null);
                wielder?.TakeDamage(dinfo);
            }
            else
            {
                this.heat -= this.heat / 2;
                if (this.heat < 0)
                {
                    this.heat = 0;
                }
            }
            this.plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;
        }
        public void HeatBuild()
        {
            this.ResetLastShotTick();
            if(plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged && heat == maxHeat)
            {
                this.OverHeatExplostion(wielder);
            }
            float heatgen = Props.heatGen;
            if(plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged)
            {
                heatgen = Props.heatGenCharged;
            }

            if(heat + heatgen * heatGenMultiplier <= maxHeat)
            {
                heat += heatgen * heatGenMultiplier;
                
                /*
                 * if(heat > maxHeat * 0.7f && weaponHolder is Pawn pawn)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(, null);
                }*/
            }
            else
            {
                heat = maxHeat;
                plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Cooling;
                if(wielder != null)
                {
                    MoteMaker.ThrowText(wielder.PositionHeld.ToVector3(), wielder.MapHeld, "OverHeat", 2f);
                }
                
            }
            //Util_Ranged.DEV_output("Plasma Heat : " + this.heat);
        }

        /*
        public void SwitchMode()
        {
            if(plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal)
            {
                plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Charged; 
                
            }
            else
            {
                plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;
            }
        }
        */

        public void SwitchToNormalMode()
        {
            plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;
        }
        public void SwitchToChargeMode()
        {
            plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Charged;
        }

        protected IEnumerable<Gizmo> GetGizmos()
        {
           
                yield return new Gizmo_PlasmaWeaponStatus
                {
                    compPlasma = this
                };
           
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            
            foreach (Gizmo gizmo2 in this.GetGizmos())
            {
                yield return gizmo2;
            }
            /*
            yield return new Command_Action
            {
                defaultLabel = "ForcedCooling".Translate(),
                defaultDesc = "ForcedCoolingDesc".Translate(),
                //icon = TexCommand.DesirePower,
                //action = new Action()
            };
            
            
            yield return new Command_Action
            {
                defaultLabel = "SafeMode".Translate() + " : " + this.safeMode.ToString().Translate(),
                defaultDesc = "SwitchSafeModeDesc".Translate(),
                icon = TexCommand.DesirePower,
                action = SwitchSafeMode
            };
            */
        }

        protected void SwitchSafeMode()
        {
            safeMode = !safeMode;
        }


        public override string ShowInfo(Thing wielder)
        {
            string header = "PlasmaWeapon".Translate();
            string text = "PlasmaWeaponMode".Translate() + " : " + this.plasmaWeaponMode.ToString().Translate();

            return header + "\n" + text;

        }
        public bool AllowShoot()
        {
            if((this.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Cooling) || ((this.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal) && (this.heat == this.maxHeat) ))
            {
                //Util_Ranged.DEV_output("Plasma Weapon is Cooling or Overheated");
                //Util_Ranged.DEV_output("can shoot now : " + !(verb is Verb_LaunchProjectile));
                return false;
            }
            return true;
        }
        // destroy weapon
        public void OverHeatExplostion( Thing weaponHolder ) { 
            GenExplosion.DoExplosion(weaponHolder.Position, weaponHolder.Map, overheatExplosionRadius, DamageDefOf.Bomb, weaponHolder, (int)overheatExplosionDamage,0.5f,
                 null, null, null, null, null); 
            parent.Destroy(DestroyMode.Vanish); 
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.heat, "heat", 0f);
            Scribe_Values.Look<Util_Ranged.PlasmaWeaponMode>(ref this.plasmaWeaponMode, "plasmaWeaponMode" , Util_Ranged.PlasmaWeaponMode.Normal);
        }
    }

    public class CompProperties_DarktidePlasma : CompProperties
    {
        public CompProperties_DarktidePlasma()
        {
            this.compClass = typeof(Comp_DarktidePlasma);
        }

        public float heatGen = 10f;

        public float heatGenCharged = 40f;

        public float coolingWeaponHeatDamage = 10f;

        public float coolingWeaponHeatLossForced = 20f;

        public float coolingWeaponHeatLossRate = 0.1f;

        public float overheatExplosionDamage = 50f;

        public float overheatExplosionRadius = 3f;

        public float maxHeat = 100f;
    }

    [StaticConstructorOnStartup]
    public class Gizmo_PlasmaWeaponStatus : Gizmo
    {
        public Comp_DarktidePlasma compPlasma;

        protected static Color HeatBarColor = new Color(165 / 255f, 204 / 255f, 249 / 255f);

        protected Texture2D FullHeatBarTex = SolidColorMaterials.NewSolidColorTexture(HeatBarColor);

        protected static readonly Texture2D EmptyHeatBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_PlasmaWeaponStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 150f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;

            string wielderlabel = compPlasma.wielder != null ? compPlasma.wielder.Label : "";
            Widgets.Label(rect3, wielderlabel +" "+ "PlasmaHeat".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = compPlasma.heat / Mathf.Max(1f, compPlasma.maxHeat);
            
            if(fillPercent > 0.9f)
            {
                Color tempDangerHeatBarColor = new Color(204 / 255f, 0 / 255f, 0 / 255f);
                FullHeatBarTex = SolidColorMaterials.NewSolidColorTexture(tempDangerHeatBarColor);
            }
            else if (fillPercent > 0.5f)
            {
                Color tempHeatBarColor = new Color(255 / 255f, 165 / 255f, 79 / 255f);
                FullHeatBarTex = SolidColorMaterials.NewSolidColorTexture(tempHeatBarColor);
            }
            Widgets.FillableBar(rect4, fillPercent, FullHeatBarTex, EmptyHeatBarTex, doBorder: false);
            Text.Font = GameFont.Small;
            Text.Anchor = (TextAnchor)4;
            Widgets.Label(rect4, (compPlasma.heat).ToString("F0"));
            Text.Anchor = (TextAnchor)0;
            TooltipHandler.TipRegion(rect2, "PlasmaHeatPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
