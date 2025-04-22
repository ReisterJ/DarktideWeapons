using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_MeleeWeaponProperties : DefModExtension
    {
        public int cleaveTargets = 2;
        public float cleaveDamageFalloffRatio = 0.90f;

    }
}
