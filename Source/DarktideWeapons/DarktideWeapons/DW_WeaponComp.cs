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
            Scribe_References.Look(ref wielder, "wielder" );
        }
    }
    public class DW_WeaponCompProperties : CompProperties
    {
    }
}
