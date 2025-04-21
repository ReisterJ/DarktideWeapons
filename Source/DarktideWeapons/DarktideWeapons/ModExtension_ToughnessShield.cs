using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class ModExtension_ToughnessShield : DefModExtension
    {
        public float shieldIncrement;

        public ModExtension_ToughnessShield() {
            shieldIncrement = 50f;
        }

        public float GetShieldIncrement(int level)
        {
            return (shieldIncrement * level);
        }

        public static ModExtension_ToughnessShield DefaultValue = new ModExtension_ToughnessShield();
        
           
        
    }
}
