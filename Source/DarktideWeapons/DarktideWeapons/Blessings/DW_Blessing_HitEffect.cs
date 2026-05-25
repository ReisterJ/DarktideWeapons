using RimWorld;
using Verse;

namespace DarktideWeapons.Blessings
{
    public class DW_Blessing_HitEffect : DW_Blessing
    {
        public override bool HasHitEffect => true;

        public override void OnHitVictim(Pawn attacker, Pawn victim, Thing weapon, float severityOverride = -1f)
        {
            if (def?.hitVictimHediff == null || victim == null || victim.Dead)
            {
                BlessingLog.Dev($"  [{def?.defName}] OnHitVictim skipped – hitVictimHediff={def?.hitVictimHediff?.defName ?? "null"} victim={(victim == null ? "null" : victim.Dead ? "dead" : victim.Name?.ToStringShort)}");
                return;
            }
            float severity = severityOverride >= 0f ? severityOverride : def.hediffHitSeverity;
            Hediff hediff = HediffMaker.MakeHediff(def.hitVictimHediff, victim);
            hediff.Severity = severity;
            victim.health.AddHediff(hediff);
            BlessingLog.Dev($"  [{def.defName}] OnHitVictim → added [{def.hitVictimHediff.defName}] (severity={severity}) to victim=[{victim.Name?.ToStringShort}]");
        }

        public override void OnHitSelf(Pawn attacker, Pawn victim, Thing weapon, float severityOverride = -1f)
        {
            if (def?.hitSelfHediff == null || attacker == null || attacker.Dead)
            {
                BlessingLog.Dev($"  [{def?.defName}] OnHitSelf skipped – hitSelfHediff={def?.hitSelfHediff?.defName ?? "null"} attacker={(attacker == null ? "null" : attacker.Dead ? "dead" : attacker.Name?.ToStringShort)}");
                return;
            }
            float severity = severityOverride >= 0f ? severityOverride : def.hediffHitSeverity;
            Hediff hediff = HediffMaker.MakeHediff(def.hitSelfHediff, attacker);
            hediff.Severity = severity;
            attacker.health.AddHediff(hediff);
            BlessingLog.Dev($"  [{def.defName}] OnHitSelf   → added [{def.hitSelfHediff.defName}] (severity={severity}) to attacker=[{attacker.Name?.ToStringShort}]");
        }
    }
}