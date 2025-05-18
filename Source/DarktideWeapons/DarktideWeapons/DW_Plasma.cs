using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class DW_Plasma : DW_Projectile
    {
        protected override void EquipmentProjectileInit(ThingWithComps equipment)
        {
            PlasmaShotInit(equipment);
        }

        protected void PlasmaShotInit(ThingWithComps thingWithComps)
        {
            Comp_DarktidePlasma compPlasma = thingWithComps.TryGetComp<Comp_DarktidePlasma>();
            if (compPlasma != null)
            {
                this.isPlasma = true;
                //Util_Ranged.DEV_output("Plasma weapon found , name : " + thingWithComps.Label);
                //Util_Ranged.DEV_output("Plasma weapon mode : " + compPlasma.plasmaWeaponMode.ToString());
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal)
                {
                    this.penetrateWall = false;
                }
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged)
                {
                    this.DamageMultiplier_Outer *= compPlasma.chargedModeDamageMultiplier;
                    this.armorPenetrationinGame *= compPlasma.chargedModeArmorPenetrationMultiplier;
                    this.penetrateNum *= 2;
                    this.penetrateWall = true;
                    this.effectiveRange *= 1.2f;
                }
                compPlasma.HeatBuild();
            }
        }
    }
}
