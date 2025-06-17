using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DarktideWeapons
{
    public class HediffComp_SpreadHediffAfterDeath : HediffComp
    {
        public HediffCompProperties_SpreadHediffAfterDeath Props
        {
            get
            {
                return (HediffCompProperties_SpreadHediffAfterDeath)this.props;
            }
        }

        
        public override void Notify_PawnKilled()
        {
            List<IntVec3> adj8way = Util_Melee.GetPawnNearArea(this.Pawn,2f);
            foreach (IntVec3 cell in adj8way)
            {
                Pawn victim = cell.GetFirstPawn(this.Pawn.Map);
                if (victim == null || victim.Dead || victim.health == null || victim.health.hediffSet == null)
                {
                    continue;
                }
                if (victim == this.Pawn)
                {
                    continue;
                }
                if (victim.Faction == Faction.OfPlayer && !Props.friendlySpread)
                {
                    continue;
                }
                
                Hediff hediff = victim.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                if (hediff != null)
                {
                    HediffSpread(ref hediff);
                    SpreadDebug(victim, hediff);
                }
                else
                {
                    BodyPartRecord bodyPartRecord = Props.isBrain ? victim.health.hediffSet.GetBrain() : null;
                    victim.health.AddHediff(Props.hediffDef, bodyPartRecord, null, null);
                    Hediff newhediff = victim.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                    HediffSpread(ref newhediff);
                    SpreadDebug(victim, hediff);
                }
            }
            base.Notify_PawnKilled();
        }
        public virtual void HediffSpread(ref Hediff hediff)
        {
            if(hediff is Hediff_DOT dot)
            {
                dot.ChangeLevel(Props.spreadLevel);
                return;
            }
            else
            {
                hediff.Severity = Props.spreadSeverity;
            }
        }

        
        private void SpreadDebug(Pawn pawn , Hediff hediff)
        {
#if DEBUG
            Log.Message("Spread to Pawn :" + pawn.Name.ToStringShort);
            Log.Message(pawn.Name.ToStringShort + " hediff : " + hediff.def.label + " | Level/Severity : " + hediff.Severity);
#endif
        }
    }

    public class HediffCompProperties_SpreadHediffAfterDeath : HediffCompProperties
    {
        public HediffCompProperties_SpreadHediffAfterDeath()
        {
            this.compClass = typeof(HediffComp_SpreadHediffAfterDeath);
        }
        public bool friendlySpread = false;

        public int spreadLevel = 3;

        public float spreadSeverity = 0.2f;

        public HediffDef hediffDef;

        public bool isBrain = true;
    }
}
