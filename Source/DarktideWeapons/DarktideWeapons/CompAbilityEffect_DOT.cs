using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarktideWeapons
{
    public class CompAbilityEffect_DOT : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            ApplyDOT(target.Pawn);
        }

        public new CompProperties_AbilityEffect_DOT Props
        {
            get
            {
                return (CompProperties_AbilityEffect_DOT)props;
            }
        }
        protected void ApplyDOT(Pawn victim)
        {
            if (victim == null || victim.Dead || victim.health == null || victim.health.hediffSet == null)
            {
                return;
            }
            if (victim.Faction == Faction.OfPlayer && !Props.friendlyFire)
            {
                return;
            }
            
            Hediff hediff = victim.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                HediffAdd(ref hediff);
            }
            else
            {
                BodyPartRecord bodyPartRecord = Props.isBrain ? victim.health.hediffSet.GetBrain() : null;
                victim.health.AddHediff(Props.hediffDef, bodyPartRecord, null, null);
                Hediff newhediff = victim.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                HediffAdd(ref newhediff);
            }

        }

        public virtual void HediffAdd(ref Hediff hediff)
        {
            if(hediff is Hediff_DOT dot)
            {
                int addlevel = Props.addLevel;
                this.parent.pawn.equipment.Primary.TryGetComp<Comp_DarktideForceStaff>()?.DOT_QualityOffset(ref addlevel);
                dot.ChangeLevel(addlevel);
                return;
            }
            else
            {
                hediff.Severity += Props.addSeverity;
            }
        }
    }

    public class CompProperties_AbilityEffect_DOT : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityEffect_DOT()
        {
            this.compClass = typeof(CompAbilityEffect_DOT);
        }
        public HediffDef hediffDef;

        public int addLevel = 1;

        public float addSeverity = 0.1f;

        public bool friendlyFire = false;

        public bool isBrain = true;
    }
}
