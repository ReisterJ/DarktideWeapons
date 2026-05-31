using DarktideWeapons.Util;
using System.Collections.Generic;
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
            Util_Hediff.HediffImpact(victim, new List<HediffDef> { def.hitVictimHediff }, null);
            BlessingLog.Dev($"  [{def.defName}] OnHitVictim → applied [{def.hitVictimHediff.defName}] to victim=[{victim.Name?.ToStringShort}]");
        }

        public override void OnHitSelf(Pawn attacker, Pawn victim, Thing weapon, float severityOverride = -1f)
        {
            if (def?.hitSelfHediff == null || attacker == null || attacker.Dead)
            {
                BlessingLog.Dev($"  [{def?.defName}] OnHitSelf skipped – hitSelfHediff={def?.hitSelfHediff?.defName ?? "null"} attacker={(attacker == null ? "null" : attacker.Dead ? "dead" : attacker.Name?.ToStringShort)}");
                return;
            }
            Util_Hediff.HediffImpact(attacker, new List<HediffDef> { def.hitSelfHediff }, null);
            BlessingLog.Dev($"  [{def.defName}] OnHitSelf   → applied [{def.hitSelfHediff.defName}] to attacker=[{attacker.Name?.ToStringShort}]");
        }
    }
}