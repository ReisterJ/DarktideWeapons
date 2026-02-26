using DarktideWeapons.MeleeComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons.SpecialMeleeVerbs
{
    // 重攻击接口
    // 用ability来实现暗潮当中一些重攻击的逻辑
    public interface IHeavyMeleeAttack
    {
        Comp_HeavyAttack HeavyAttackComp { get; }
        bool TryStartHeavyMeleeAttack();

        void Notify_HeavyMeleeAttacked();
    }
}
