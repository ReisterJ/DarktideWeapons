using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DarktideWeapons
{
    public class DW_Plasma : DW_Projectile
    {
        protected override void EquipmentProjectileInit(ThingWithComps equipment)
        {
            PlasmaShotInit(equipment);
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            
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
