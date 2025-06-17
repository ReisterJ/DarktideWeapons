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
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Normal)
                {
                    this.penetrateWall = false;
                }
                
                compPlasma.HeatBuild();
                if (compPlasma.plasmaWeaponMode == Util_Ranged.PlasmaWeaponMode.Charged)
                {
                    compPlasma.plasmaWeaponMode = Util_Ranged.PlasmaWeaponMode.Normal;
                }
            }
        }
    }
}
