using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons.Blessings
{
    public class DW_Blessing_DamageEffect : DW_Blessing
    {
        
        public bool TryApplyDamageEffect2Victim(DW_Equipment equipment)
        {
            return false;
        }
        
        public bool TryApplyDamageEffect2Attacker(DW_Equipment equipment)
        {
            return false;
        }
    }
}
