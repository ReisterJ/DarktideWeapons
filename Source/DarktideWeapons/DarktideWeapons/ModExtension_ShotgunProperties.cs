using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_ShotgunProperties: DefModExtension
    {
        public int projectileNum = 6;

        public float spreadAngleMax = 20f;

        public float spreadAngleMin = 0f;

        public static ModExtension_ShotgunProperties DefaultValue = new ModExtension_ShotgunProperties()
        {
            projectileNum = 6
            , spreadAngleMax = 20f
            , spreadAngleMin = 0f
        };
    }
}
