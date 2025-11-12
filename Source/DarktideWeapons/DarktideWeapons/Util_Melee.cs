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
    public static class Util_Melee
    {
        public const float Quality_Excellent_Bias = 1.25f;
        public const float Quality_Master_Bias = 1.5f;
        public const float Quality_Legendary_Bias = 2f;
        public const int MaxLevel = 20;

        
        public const int ExpertLevel = 10;
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
        
        public static List<IntVec3> GetPawnNearArea(Pawn pawn,float radius)
        {
            IntVec3 root = pawn.Position;
            List<IntVec3> area = new List<IntVec3>();
            int minx = root.x - (int)radius;
            int maxx = root.x + (int)radius;    
            int minz = root.z - (int)radius;
            int maxz = root.z + (int)radius;
            for (int x = minx; x <= maxx; x++)
            {
                for (int z = minz; z <= maxz; z++)
                {
                    IntVec3 cell = new IntVec3(x, root.y, z);
                    if (cell.InBounds(pawn.Map)) //&& cell.DistanceTo(root) <= radius)
                    {
                        area.Add(cell);
                    }
                }
            }
            return area;
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

        public static BodyPartRecord TryHitCorePart(Pawn CasterPawn,Pawn targetPawn)
        {
            float headhit = 0f;
            int level = CasterPawn?.skills.GetSkill(SkillDefOf.Melee).Level ?? 0;
            if (level > ExpertLevel)
            {
                headhit = (level - ExpertLevel) * 0.05f;
            }
            if (Rand.Chance(headhit))
            {
                return Util_BodyPart.GetHeadPart(targetPawn);
            }
            return null;
        }
        public static void Melee_Stats(Pawn pawn)
        {

        }

        public static void DEV_output(object o)
        {
#if DEBUG
            Log.Message(o);
#endif
        }
        public enum CraftType
        {
            None,
            Cardinal,
            Impale,
            Vanguard,
            Juggernaut,
            Strikedown,
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
