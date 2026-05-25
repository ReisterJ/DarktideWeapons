using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace DarktideWeapons.Blessings
{
    /// <summary>
    /// Weapon comp that registers a weapon for blessing and perk modification.
    /// Add this comp via XML to any weapon that should be modifiable at the Omnissiah's Forge.
    /// Stores two lists: installed blessings (DW_BlessingDef) and installed perks (DW_EquipmentPerkDef).
    /// Slot limits are configurable in XML (default 2 each).
    /// </summary>
    public class Comp_BlessingSocket : DW_WeaponComp
    {
        private List<DW_BlessingDef> installedBlessings = new List<DW_BlessingDef>();
        private List<DW_EquipmentPerkDef> installedPerks = new List<DW_EquipmentPerkDef>();

        public new CompProperties_BlessingSocket Props => (CompProperties_BlessingSocket)this.props;

        public IReadOnlyList<DW_BlessingDef> InstalledBlessings => installedBlessings;
        public IReadOnlyList<DW_EquipmentPerkDef> InstalledPerks => installedPerks;

        public int MaxBlessingSlots => Props.maxBlessingSlots;
        public int MaxPerkSlots => Props.maxPerkSlots;

        public bool CanAddBlessing => installedBlessings.Count < MaxBlessingSlots;
        public bool CanAddPerk => installedPerks.Count < MaxPerkSlots;

        public IReadOnlyList<DW_BlessingDef> AvailableBlessings
        {
            get
            {
                var pool = Props.availableBlessings != null && Props.availableBlessings.Count > 0
                    ? Props.availableBlessings
                    : DefDatabase<DW_BlessingDef>.AllDefsListForReading;
                return pool.Where(d => d != null && !installedBlessings.Contains(d)).ToList();
            }
        }

        public IReadOnlyList<DW_EquipmentPerkDef> AvailablePerks
        {
            get
            {
                var pool = Props.availablePerks != null && Props.availablePerks.Count > 0
                    ? Props.availablePerks
                    : DefDatabase<DW_EquipmentPerkDef>.AllDefsListForReading;
                return pool.Where(d => d != null && !installedPerks.Contains(d)).ToList();
            }
        }

        public float GetHitSeverityOverride(DW_BlessingDef blessDef)
        {
            if (blessDef == null) return -1f;
            var overrides = Props.overrideBlessingDefs;
            if (overrides != null)
            {
                int idx = overrides.IndexOf(blessDef);
                if (idx >= 0 && Props.overrideHitSeverities != null && idx < Props.overrideHitSeverities.Count)
                    return Props.overrideHitSeverities[idx];
            }
            return blessDef.hitVictimHediff != null ? blessDef.hediffHitSeverity : -1f;
        }

        public float GetKillSeverityOverride(DW_BlessingDef blessDef)
        {
            if (blessDef == null) return -1f;
            var overrides = Props.overrideBlessingDefs;
            if (overrides != null)
            {
                int idx = overrides.IndexOf(blessDef);
                if (idx >= 0 && Props.overrideKillSeverities != null && idx < Props.overrideKillSeverities.Count)
                    return Props.overrideKillSeverities[idx];
            }
            return blessDef.killSelfHediff != null ? blessDef.hediffKillSeverity : -1f;
        }

        // ─── Blessing management ───────────────────────────────────────────────

        public bool AddBlessing(DW_BlessingDef def)
        {
            if (def == null)
            {
                BlessingLog.Warn($"AddBlessing called with null def on weapon [{parent.Label}]");
                return false;
            }
            if (!CanAddBlessing)
            {
                BlessingLog.Dev($"AddBlessing FAILED – slots full ({installedBlessings.Count}/{MaxBlessingSlots}) on [{parent.Label}], tried to add [{def.defName}]");
                return false;
            }
            if (installedBlessings.Contains(def))
            {
                BlessingLog.Dev($"AddBlessing FAILED – [{def.defName}] already installed on [{parent.Label}]");
                return false;
            }
            installedBlessings.Add(def);
            BlessingLog.Dev($"AddBlessing OK   – [{def.defName}] installed on [{parent.Label}]  ({installedBlessings.Count}/{MaxBlessingSlots} slots used)");
            return true;
        }

        public bool RemoveBlessing(DW_BlessingDef def)
        {
            bool removed = installedBlessings.Remove(def);
            if (removed)
                BlessingLog.Dev($"RemoveBlessing OK   – [{def?.defName}] removed from [{parent.Label}]  ({installedBlessings.Count}/{MaxBlessingSlots} slots remaining)");
            else
                BlessingLog.Dev($"RemoveBlessing FAILED – [{def?.defName}] not found on [{parent.Label}]");
            return removed;
        }

        // ─── Perk management ──────────────────────────────────────────────────

        public bool AddPerk(DW_EquipmentPerkDef def)
        {
            if (def == null)
            {
                BlessingLog.Warn($"AddPerk called with null def on weapon [{parent.Label}]");
                return false;
            }
            if (!CanAddPerk)
            {
                BlessingLog.Dev($"AddPerk FAILED – slots full ({installedPerks.Count}/{MaxPerkSlots}) on [{parent.Label}], tried to add [{def.defName}]");
                return false;
            }
            if (installedPerks.Contains(def))
            {
                BlessingLog.Dev($"AddPerk FAILED – [{def.defName}] already installed on [{parent.Label}]");
                return false;
            }
            installedPerks.Add(def);
            def.GetWorker().OnInstalled(parent);
            BlessingLog.Dev($"AddPerk OK   – [{def.defName}] installed on [{parent.Label}]  ({installedPerks.Count}/{MaxPerkSlots} slots used)");
            return true;
        }

        public bool RemovePerk(DW_EquipmentPerkDef def)
        {
            if (!installedPerks.Remove(def))
            {
                BlessingLog.Dev($"RemovePerk FAILED – [{def?.defName}] not found on [{parent.Label}]");
                return false;
            }
            def.GetWorker().OnRemoved(parent);
            BlessingLog.Dev($"RemovePerk OK   – [{def?.defName}] removed from [{parent.Label}]  ({installedPerks.Count}/{MaxPerkSlots} slots remaining)");
            return true;
        }

        // ─── Effect triggers ──────────────────────────────────────────────────

        /// <summary>
        /// Call this from weapon verbs or projectile hits when the weapon strikes a pawn.
        /// Fires all installed blessings that have hit effects.
        /// </summary>
        public void Notify_HitPawn(Pawn attacker, Pawn victim)
        {
            if (attacker == null || victim == null) return;
            BlessingLog.Dev($"Notify_HitPawn – attacker=[{attacker.Name?.ToStringShort}] victim=[{victim.Name?.ToStringShort}] weapon=[{parent.Label}]  blessings installed: {installedBlessings.Count}");
            foreach (var blessDef in installedBlessings)
            {
                var worker = blessDef.GetWorker();
                if (!worker.HasHitEffect)
                {
                    BlessingLog.Dev($"  Blessing [{blessDef.defName}] has no hit effect, skipping");
                    continue;
                }
                BlessingLog.Dev($"  Blessing [{blessDef.defName}] firing OnHitVictim → victim=[{victim.Name?.ToStringShort}]");
                worker.OnHitVictim(attacker, victim, parent);
                BlessingLog.Dev($"  Blessing [{blessDef.defName}] firing OnHitSelf   → attacker=[{attacker.Name?.ToStringShort}]");
                worker.OnHitSelf(attacker, victim, parent);
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            Pawn attacker = PawnOwner;
            if (attacker == null)
            {
                BlessingLog.Dev($"Notify_KilledPawn – [{parent.Label}] PawnOwner is null, cannot trigger kill blessings (killed: [{pawn?.Name?.ToStringShort}])");
                return;
            }
            BlessingLog.Dev($"Notify_KilledPawn – attacker=[{attacker.Name?.ToStringShort}] killed=[{pawn?.Name?.ToStringShort}] weapon=[{parent.Label}]  blessings installed: {installedBlessings.Count}");
            foreach (var blessDef in installedBlessings)
            {
                var worker = blessDef.GetWorker();
                if (!worker.HasKillEffect)
                {
                    BlessingLog.Dev($"  Blessing [{blessDef.defName}] has no kill effect, skipping");
                    continue;
                }
                BlessingLog.Dev($"  Blessing [{blessDef.defName}] firing OnKillPawn → attacker=[{attacker.Name?.ToStringShort}]");
                worker.OnKillPawn(attacker, pawn, parent);
            }
        }

        // ─── Save / Load ──────────────────────────────────────────────────────

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref installedBlessings, "installedBlessings", LookMode.Def);
            Scribe_Collections.Look(ref installedPerks, "installedPerks", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (installedBlessings == null) installedBlessings = new List<DW_BlessingDef>();
                if (installedPerks == null)     installedPerks     = new List<DW_EquipmentPerkDef>();
            }
        }

        // ─── Inspect string ───────────────────────────────────────────────────

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Perks: {installedPerks.Count}/{MaxPerkSlots}");
            foreach (var p in installedPerks)
                sb.AppendLine($"  [{p.LabelCap}]");
            sb.AppendLine($"Blessings: {installedBlessings.Count}/{MaxBlessingSlots}");
            foreach (var b in installedBlessings)
                sb.AppendLine($"  [{b.LabelCap}]");
            return sb.ToString().TrimEnd();
        }
    }

    public class CompProperties_BlessingSocket : CompProperties
    {
        /// <summary>Maximum number of blessings that can be installed. Default: 2.</summary>
        public int maxBlessingSlots = 2;

        /// <summary>Maximum number of perks that can be installed. Default: 2.</summary>
        public int maxPerkSlots = 2;

        /// <summary>
        /// Blessings the player can choose from for this weapon. If empty, all blessings in the
        /// DefDatabase are available (minus already-installed ones).
        /// </summary>
        public List<DW_BlessingDef> availableBlessings;

        /// <summary>
        /// Perks the player can choose from for this weapon. If empty, all perks in the
        /// DefDatabase are available (minus already-installed ones).
        /// </summary>
        public List<DW_EquipmentPerkDef> availablePerks;

        /// <summary>
        /// Parallel lists for per-blessing severity overrides.
        /// overrideBlessingDefs[i] maps to overrideHitSeverities[i] / overrideKillSeverities[i].
        /// Use -1 to indicate "no override" for a particular blessing.
        /// </summary>
        public List<DW_BlessingDef> overrideBlessingDefs;
        public List<float> overrideHitSeverities;
        public List<float> overrideKillSeverities;

        public CompProperties_BlessingSocket()
        {
            this.compClass = typeof(Comp_BlessingSocket);
        }
    }
}
