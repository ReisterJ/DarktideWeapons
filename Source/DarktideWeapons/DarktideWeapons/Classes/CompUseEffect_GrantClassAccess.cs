using RimWorld;
using Verse;

namespace DarktideWeapons.Classes
{
    /// <summary>
    /// CompUseEffect for the class-grant item.
    /// Applies Hediff_ClassSystem to the user with a configurable starting skill point count.
    /// Does nothing if the pawn already has the hediff.
    /// </summary>
    public class CompUseEffect_GrantClassAccess : CompUseEffect
    {
        public new CompProperties_GrantClassAccess Props => (CompProperties_GrantClassAccess)props;

        public override TaggedString ConfirmMessage(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(DW_ClassHediffDefOf.DW_ClassSystem))
                return "DW_Class_AlreadyGranted".Translate(pawn.LabelShort);
            return null;
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (p.health.hediffSet.HasHediff(DW_ClassHediffDefOf.DW_ClassSystem))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Class] CanBeUsedBy REJECTED for {p.LabelShort}: already has DW_ClassSystem hediff.");
                return "DW_Class_AlreadyGranted".Translate(p.LabelShort);
            }
            if (DebugSettings.godMode)
                Log.Message($"[DW Class] CanBeUsedBy OK for {p.LabelShort}.");
            return AcceptanceReport.WasAccepted;
        }

        public override void DoEffect(Pawn usedBy)
        {
            if (usedBy.health.hediffSet.HasHediff(DW_ClassHediffDefOf.DW_ClassSystem))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Class] DoEffect skipped for {usedBy.LabelShort}: hediff already present.");
                return;
            }

            if (DebugSettings.godMode)
                Log.Message($"[DW Class] DoEffect: applying DW_ClassSystem to {usedBy.LabelShort} " +
                    $"with {Props.startingSkillPoints} starting SP.");

            var hediff = (Hediff_ClassSystem)HediffMaker.MakeHediff(
                DW_ClassHediffDefOf.DW_ClassSystem, usedBy);
            hediff.skillPoints = Props.startingSkillPoints;
            hediff.Severity    = 1f;
            usedBy.health.AddHediff(hediff);

            if (DebugSettings.godMode)
                Log.Message($"[DW Class] DW_ClassSystem hediff added to {usedBy.LabelShort} successfully.");

            Messages.Message(
                "DW_Class_AccessGranted".Translate(usedBy.LabelShort, Props.startingSkillPoints),
                usedBy, MessageTypeDefOf.PositiveEvent);
        }
    }

    public class CompProperties_GrantClassAccess : CompProperties
    {
        /// <summary>Skill points awarded when the item is used.</summary>
        public int startingSkillPoints = 5;

        public CompProperties_GrantClassAccess()
        {
            compClass = typeof(CompUseEffect_GrantClassAccess);
        }
    }

    /// <summary>
    /// DefOf referencing the class hediff by defName so it can be used in code without magic strings.
    /// </summary>
    [DefOf]
    public static class DW_ClassHediffDefOf
    {
        public static HediffDef DW_ClassSystem;

        static DW_ClassHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DW_ClassHediffDefOf));
        }
    }
}
