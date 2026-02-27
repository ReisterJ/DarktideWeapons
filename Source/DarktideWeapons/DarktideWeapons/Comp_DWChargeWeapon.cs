using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{

    //为近战武器准备，充能武器以获取更强大的攻击
    public class Comp_DWChargeWeapon : DW_WeaponComp
    {
        public int ChargeMax = 100;
        public int ChargeCurrent = 0;
        protected int chargeTick = 0;
        public  int ChargeLastingTicks => Props.weaponChargeLastingTicks;
        public bool isCharged = false;
        public int NewCleaveNum => Props.cleaveNum; 
        public float NewDamageFactor => Props.weaponChargeDamageFactor;
        public float NewArmorPenetrationFactor => Props.weaponChargeArmorPenetrationFactor;

        public bool CauseExplosion => Props.causeExplosion;

        public bool BodysizeMatters => Props.bodysizeMatters;

        public bool HeatBuildup => Props.heatBuildup;

        public float heat = 0f;

        public float heatBuildRate = 0.025f;

        public float maxHeat = 100f;
        public CompProperties_DWChargeWeapon Props
        {
            get
            {
                return (CompProperties_DWChargeWeapon)this.props;
            }

        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            ChargeMax = Props.ChargeMax;
        }

        public override void CompTickInterval(int delta)
        {
            //base.CompTickInterval(delta);
            if (isCharged)
            {
                if (HeatBuildup)
                {
                    heat += heatBuildRate * delta;
                    if(heat >= maxHeat)
                    {
                        heat = maxHeat;
                        LoseCharge();
                        return;
                    }
                }

                chargeTick -= delta;
                if(chargeTick <= 0)
                {
                    LoseCharge();
                }
            }
            if (HeatBuildup && !isCharged && heat > 0)
            {
                heat -= heatBuildRate * delta * 5f;
                if (heat < 0) heat = 0;
            }
            
        }

        public void DoChargedExplosion(IntVec3 center, Map map,Thing instigator)
        {
            GenExplosionDW.DoExplosionNoFriendlyFire(center, map, Props.explosionRadius, Props.explosionDamageDef, instigator, (int)Props.explosionDamage);
        }
        public void ConsumeCharge(int charge)
        {
            ChargeCurrent -= charge;
            if(ChargeCurrent <= 0)
            {
                LoseCharge();
            }
        }
        public void ResetCharge()
        {
            ChargeCurrent = 0;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
                yield return g;

            
            yield return new Gizmo_ChargeCurrentSimple
            {
                comp = this
            };

            if (Props.heatBuildup)
            {
                yield return new Gizmo_ChargeCurrentHeat
                {
                    comp = this
                };
            }
        }

        public void LoseCharge() 
        {
            isCharged = false;
            ChargeCurrent = 0;
            chargeTick = -1;
        }
        public void ChargeWeapon()
        {
            if(this.PawnOwner != null)
            {
                isCharged = true;
                ChargeCurrent = ChargeMax;
                chargeTick = ChargeLastingTicks;
            }
        }
    }
    public class CompProperties_DWChargeWeapon : CompProperties
    {
        public int ChargeMax = 100;

        public int weaponChargeLastingTicks = 1800;

        public float weaponChargeDamageFactor = 1.5f;

        public float weaponChargeArmorPenetrationFactor = 1.5f;

        public int cleaveNum = 1;

        public bool causeExplosion = false;

        public bool bodysizeMatters = false;

        public float explosionDamage = 1f;
        public float explosionRadius = 1f;
        public float explosionArmorPenetration = 1f;
        public DamageDef explosionDamageDef;

        public bool heatBuildup = false;
        public CompProperties_DWChargeWeapon()
        {
            this.compClass = typeof(Comp_DWChargeWeapon);
        }
    }

    public class Gizmo_ChargeCurrentSimple : Gizmo
    {
        public Comp_DWChargeWeapon comp;

        public override float GetWidth(float maxWidth) => 100f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect inRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(inRect);
            Rect labelRect = inRect.ContractedBy(6f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect.TopPartPixels(24f), "DW_CHARGE");

            int current = comp.ChargeCurrent;
            int max = comp.ChargeMax;
            string valueStr = $"{current}";

            UnityEngine.Color oldColor = GUI.color;
            if (current < max / 5)
                GUI.color = Color.red;

            Widgets.Label(labelRect.BottomPartPixels(24f), valueStr);

            GUI.color = oldColor;
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
    }

    [StaticConstructorOnStartup]
    public class Gizmo_ChargeCurrentHeat : Gizmo
    {
        public Comp_DWChargeWeapon comp;

        public override float GetWidth(float maxWidth) => 120f;

        protected static Color HeatBarColor = new Color(165 / 255f, 204 / 255f, 249 / 255f);

        protected Texture2D FullHeatBarTex = SolidColorMaterials.NewSolidColorTexture(HeatBarColor);

        protected static readonly Texture2D EmptyHeatBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;

            string wielderlabel = comp.wielder != null ? comp.wielder.Label : "";
            Widgets.Label(rect3, wielderlabel + " " + "DWChargeHeat".Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = comp.heat / Mathf.Max(1f, comp.maxHeat);

            if (fillPercent > 0.9f)
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
            Widgets.Label(rect4, (comp.heat).ToString("F0"));
            Text.Anchor = (TextAnchor)0;
            TooltipHandler.TipRegion(rect2, "PlasmaHeatPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }

    public class Comp_DWChainSawRending : Comp_DWChargeWeapon
    {

    }

    public class CompProperties_DWChainSawRending : CompProperties_DWChargeWeapon
    {
        public int rendDamageTick = 10;

        public float rendDamage = 20f;

        public int rendTickLasting = 120;
        public CompProperties_DWChainSawRending()
        {
            this.compClass = typeof(Comp_DWChainSawRending);
        }
    }
}
