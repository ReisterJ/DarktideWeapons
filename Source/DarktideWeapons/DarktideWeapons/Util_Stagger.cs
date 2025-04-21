using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons
{
    public static class Util_Stagger
    {
        public const int baseStaggerTick = 90;
        public enum StaggerLevel
        {
            None,
            Light,
            Medium,
            Heavy,
            Stunned
        }
        public static int GetStaggerTick(int level)
        {
            if(level < 4)
            {
                return baseStaggerTick * ( level + 1 ); 
            }
            return baseStaggerTick;
        }
    }
}
