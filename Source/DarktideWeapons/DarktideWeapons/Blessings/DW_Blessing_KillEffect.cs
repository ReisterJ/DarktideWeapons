using RimWorld;
using Verse;

namespace DarktideWeapons.Blessings
{
    public class DW_Blessing_KillEffect : DW_Blessing
    {
        public override bool HasKillEffect => true;

        public override void OnKillPawn(Pawn attacker, Pawn killed, Thing weapon, float severityOverride = -1f)
        {
            if (def?.killSelfHediff == null || attacker == null || attacker.Dead)
            {
                BlessingLog.Dev($"  [{def?.defName}] OnKillPawn skipped – killSelfHediff={def?.killSelfHediff?.defName ?? "null"} attacker={(attacker == null ? "null" : attacker.Dead ? "dead" : attacker.Name?.ToStringShort)}");
                return;
            }
            float severity = severityOverride >= 0f ? severityOverride : def.hediffKillSeverity;
            Hediff hediff = HediffMaker.MakeHediff(def.killSelfHediff, attacker);
            hediff.Severity = severity;
            attacker.health.AddHediff(hediff);
            BlessingLog.Dev($"  [{def.defName}] OnKillPawn → added [{def.killSelfHediff.defName}] (severity={severity}) to attacker=[{attacker.Name?.ToStringShort}]  (killed: [{killed?.Name?.ToStringShort}])");
        }
    }
}