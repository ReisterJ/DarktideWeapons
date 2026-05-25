using RimWorld;
using Verse;

namespace DarktideWeapons.Career
{
    /// <summary>
    /// CompUseEffect for the career-grant item.
    /// Applies Hediff_CareerSystem to the user with a configurable starting skill point count.
    /// Does nothing if the pawn already has the hediff.
    /// </summary>
    public class CompUseEffect_GrantCareerAccess : CompUseEffect
    {
        public new CompProperties_GrantCareerAccess Props => (CompProperties_GrantCareerAccess)props;

        public override TaggedString ConfirmMessage(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(DW_CareerHediffDefOf.DW_CareerSystem))
                return "DW_Career_AlreadyGranted".Translate(pawn.LabelShort);
            return null;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.health.hediffSet.HasHediff(DW_CareerHediffDefOf.DW_CareerSystem))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Career] CanBeUsedBy REJECTED for {p.LabelShort}: already has DW_CareerSystem hediff.");
                return "DW_Career_AlreadyGranted".Translate(p.LabelShort);
            }
            if (DebugSettings.godMode)
                Log.Message($"[DW Career] CanBeUsedBy OK for {p.LabelShort}.");
            return AcceptanceReport.WasAccepted;
        }

        public override void DoEffect(Pawn usedBy)
        {
            if (usedBy.health.hediffSet.HasHediff(DW_CareerHediffDefOf.DW_CareerSystem))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Career] DoEffect skipped for {usedBy.LabelShort}: hediff already present.");
                return;
            }

            if (DebugSettings.godMode)
                Log.Message($"[DW Career] DoEffect: applying DW_CareerSystem to {usedBy.LabelShort} " +
                    $"with {Props.startingSkillPoints} starting SP.");

            var hediff = (Hediff_CareerSystem)HediffMaker.MakeHediff(
                DW_CareerHediffDefOf.DW_CareerSystem, usedBy);
            hediff.skillPoints = Props.startingSkillPoints;
            hediff.Severity    = 1f;
            usedBy.health.AddHediff(hediff);

            if (DebugSettings.godMode)
                Log.Message($"[DW Career] DW_CareerSystem hediff added to {usedBy.LabelShort} successfully.");

            Messages.Message(
                "DW_Career_AccessGranted".Translate(usedBy.LabelShort, Props.startingSkillPoints),
                usedBy, MessageTypeDefOf.PositiveEvent);
        }
    }

    public class CompProperties_GrantCareerAccess : CompProperties
    {
        /// <summary>Skill points awarded when the item is used.</summary>
        public int startingSkillPoints = 5;

        public CompProperties_GrantCareerAccess()
        {
            compClass = typeof(CompUseEffect_GrantCareerAccess);
        }
    }

    /// <summary>
    /// DefOf referencing the career hediff by defName so it can be used in code without magic strings.
    /// </summary>
    [DefOf]
    public static class DW_CareerHediffDefOf
    {
        public static HediffDef DW_CareerSystem;

        static DW_CareerHediffDefOf() { DefOfHelper.EnsureInitializedInCtor(typeof(DW_CareerHediffDefOf)); }
    }
}
