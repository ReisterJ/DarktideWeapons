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
       

        public Pawn PawnOwner
        {
            get
            {
                if(wielder is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }

        public virtual string ShowInfo(Thing wielder)
        {
            return "DEFAULTINFO".Translate();
        }
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            if (this.parent.def.IsMeleeWeapon)
            {
                Linked_CompDWToughnessShield?.Recharge_Afterkill();
            }


        }


    }
}
