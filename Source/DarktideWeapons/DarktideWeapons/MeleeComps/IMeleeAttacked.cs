using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons.MeleeComps
{
    public interface IMeleeAttacked
    {
        void PostMeleeAttacked(MeleeAttackData data);
    }
}
