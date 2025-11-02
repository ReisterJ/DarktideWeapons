using DarktideWeapons.MeleeComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    // 特殊攻击接口 
    // 用ability来实现暗潮当中一些特殊近战攻击的逻辑
    public interface ISpecialMeleeAttack
    {
        bool TryStartSpecialMeleeAttack();

        Comp_SpecialMelee SpecialMeleeComp { get; }
    }
}
