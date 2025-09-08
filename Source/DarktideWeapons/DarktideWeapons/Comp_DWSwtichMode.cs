using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DarktideWeapons
{
    public class Comp_DWSwtichMode : ThingComp
    {
        public Comp_DWToughnessShield Linked_CompDWToughnessShield;

        public Thing wielder;
        //protected int hediffStoredCounter = 0;
        public Pawn PawnOwner
        {
            get
            {
                if (wielder == null) return null;
                if (wielder is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }

        public bool isMainMode = true;
        public List<HediffDef> weaponCompHediffDefs = new List<HediffDef>();
        public CompProperties_DWSwitchMode Props
        {
            get
            {
                return (CompProperties_DWSwitchMode)props;
            }
        }

        public override void CompTickInterval(int delta)
        {
           
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            RemoveGivenHediffs();
            this.wielder = null;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            this.wielder = pawn;
            base.Notify_Equipped(pawn);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach (var hediffDef in Props.hediffDefs)
            {
                this.weaponCompHediffDefs.Add(hediffDef);
            }
        }

        
        protected virtual void RemoveGivenHediffs(int id = 0)
        {
            if(PawnOwner == null) return;
            foreach (HediffDef hediffDef in weaponCompHediffDefs)
            {
                Hediff H = PawnOwner.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (H != null)
                {
                    PawnOwner.health.RemoveHediff(H);
                    Log.Message("Removed Hediff: " + hediffDef.defName);
                }
            }
        }

        protected virtual void AddGivenHediffs(int id = 0)
        {
            if (PawnOwner == null) return;
            foreach (HediffDef hediff in weaponCompHediffDefs)
            {
                if (PawnOwner.health.hediffSet.HasHediff(hediff)) continue;
                PawnOwner.health.AddHediff(hediff);
                Log.Message("Added Hediff: " + hediff.defName);
            }
        }
        public virtual void EquipmentChangeVerbWithHediff(bool AuxiMode , int id = 0)
        {
            if (PawnOwner != null)
            {
                if (AuxiMode)
                {
                    isMainMode = false;
                    AddGivenHediffs();
                }
                else
                {
                    isMainMode = true;
                    RemoveGivenHediffs();
                }
            }
        }
    }

    public class CompProperties_DWSwitchMode : CompProperties
    {
        public List<HediffDef> hediffDefs = new List<HediffDef>();
        public CompProperties_DWSwitchMode()
        {
            this.compClass = typeof(Comp_DWSwtichMode);
        }
    }
}
