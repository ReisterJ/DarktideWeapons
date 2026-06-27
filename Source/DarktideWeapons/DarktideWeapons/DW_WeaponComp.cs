using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class DW_WeaponComp : ThingComp
    {

        public Comp_DWToughnessShield Linked_CompDWToughnessShield;

        public Thing wielder;
        //protected int hediffStoredCounter = 0;
        public Pawn PawnOwner
        {
            get
            {
                if (wielder == null) return null;
                if(wielder is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }
        
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            
        }
        
        public virtual string ShowInfo(Thing wielder)
        {
            return "DWWEAPONCOMP_DEFAULTINFO".Translate();
        }
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            if (this.parent.def.IsMeleeWeapon)
            {
                Linked_CompDWToughnessShield?.Recharge_Afterkill();
            }
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            this.wielder = null;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            this.wielder = pawn;
            base.Notify_Equipped(pawn);
        }
        
        public virtual void SwitchMode(bool AuxiMode , int id = 0 )
        {
            if (AuxiMode)
            {

            }
            else
            {

            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            // 不再在此处保存/加载 wielder。
            // 原因：多个 DW_WeaponComp 子类实例存在于同一武器上时，
            // 每个子类的 base.PostExposeData() 都会尝试以相同键名 "wielder" 
            // 读写 Scribe_References.Look，导致 LoadIDsWantedBank 中
            // 出现重复的 loadID 注册
            //
            // wielder 字段会通过以下方式正确恢复：
            // 1. DW_Equipment.ExposeData() 加载 holder 后同步到所有 comp
            // 2. Notify_Equipped 在装备时被调用
        }
    }
    public class DW_WeaponCompProperties : CompProperties
    {
    }
}
