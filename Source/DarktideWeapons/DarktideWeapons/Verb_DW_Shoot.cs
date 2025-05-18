using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Verb_DW_Shoot : Verb_LaunchProjectile
    {
        protected override int ShotsPerBurst => verbProps.burstShotCount;

        public Comp_DarktideWeapon DarktideWeapon         
        {
            get
            {
                if (EquipmentSource != null)
                {
                    return EquipmentSource.TryGetComp<Comp_DarktideWeapon>();
                }
                return null;
            }
        }
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (currentTarget.Thing is Pawn pawn && CasterIsPawn && CasterPawn.skills != null)
            {
                float num = (pawn.HostileTo(caster) ? 170f : 20f);
                float num2 = verbProps.AdjustedFullCycleTime(this, CasterPawn);
                CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
            }
        }

        public override bool Available()
        {
            bool flag = base.Available();
            if (DarktideWeapon != null)
            {
                if (DarktideWeapon.comp_DarktidePlasma != null)
                {
                    flag = DarktideWeapon.comp_DarktidePlasma.AllowShoot();
                }
            }
            return flag;
        }
        protected override bool TryCastShot()
        {
            bool num = base.TryCastShot();
            if (num && CasterIsPawn)
            {
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
            
            return num;
        }
    }
}
