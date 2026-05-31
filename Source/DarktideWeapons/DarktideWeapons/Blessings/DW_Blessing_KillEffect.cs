using DarktideWeapons.Util;
using System.Collections.Generic;
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
            Util_Hediff.HediffImpact(attacker, new List<HediffDef> { def.killSelfHediff }, null);
            BlessingLog.Dev($"  [{def.defName}] OnKillPawn → applied [{def.killSelfHediff.defName}] to attacker=[{attacker.Name?.ToStringShort}]  (killed: [{killed?.Name?.ToStringShort}])");
        }
    }
}