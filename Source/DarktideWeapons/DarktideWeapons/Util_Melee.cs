using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public static class Util_Melee
    {
        public const float Quality_Excellent_Bias = 1.25f;
        public const float Quality_Master_Bias = 1.5f;
        public const float Quality_Legendary_Bias = 2f;
        public const int MaxLevel = 20;

        public static float PawnMeleeLevelDamageMultiplier(Pawn pawn)
        {
            int meleeLevel = pawn.skills.GetSkill(SkillDefOf.Melee).Level;
            float bias = 0.001f;
            if (meleeLevel >= MaxLevel)
            {
                return 1.2f;
            }

            return (meleeLevel * meleeLevel) / 2 * bias + 1;
        }
        
        
        public static bool IsMeleeDamage(DamageInfo? dinfo)
        {
            if (((dinfo != null) ? dinfo.GetValueOrDefault().WeaponBodyPartGroup : null) != null 
                || ((dinfo != null) ? dinfo.GetValueOrDefault().WeaponLinkedHediff : null) != null
                || (dinfo.Value.Weapon != null && dinfo.Value.Weapon.IsMeleeWeapon))
            {
                return true;
            }
            return false;
        }
        
       

        public static void ApplyBleeding(Pawn victim , int ticks)
        {
            if(victim != null)
            {
               
            }
        }

        public static void AbilityThunderHammerChargedStrike()
        {
            //
        }
        

        public static void DEV_output(object o)
        {
#if DEBUG
            Log.Message(o);
#endif
        }
        public enum CleaveType
        {
            None,
            Cardinal,
            Impale,
            CrowdControl,
            Juggernaut,
            type5,
            type6,
            type7,
            type8,
            type9,
            type10,
            type11,
            type12,
            type13,
            type14,
            type15

        }
    }
}
