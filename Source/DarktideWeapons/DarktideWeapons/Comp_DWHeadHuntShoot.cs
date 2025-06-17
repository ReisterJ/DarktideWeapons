using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace DarktideWeapons
{
    public class Comp_DWHeadHuntShoot : DW_WeaponComp
    {
        public override string ShowInfo(Thing wielder)
        {
            string header = "HeadHunt".Translate();
            float chance = Util_Ranged.HeadHuntBaseChance;
            if (wielder is Pawn pawn)
            {
                int shootlevel = pawn.skills.GetSkill(SkillDefOf.Shooting).Level;
                chance = Util_Ranged.HeadHuntChanceCalculation(shootlevel);
                
            }
            string text = "HeadShotChance".Translate() + " : " + chance.ToString();
            return header + "\n" + text + "\n";
        }
    }

    public class CompProperties_DWHeadHuntShoot : CompProperties
    {
        public CompProperties_DWHeadHuntShoot()
        {
            this.compClass = typeof(Comp_DWHeadHuntShoot);
        }

    }
}
