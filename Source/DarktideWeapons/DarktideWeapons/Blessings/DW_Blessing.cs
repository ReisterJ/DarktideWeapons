using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace DarktideWeapons.Blessings
{
    /// <summary>
    /// Abstract stateless worker for weapon blessings.
    /// Handles two scenarios: on-hit effects (targeting victim or self) and on-kill effects (targeting self only).
    /// </summary>
    public abstract class DW_Blessing
    {
        public DW_BlessingDef def;

        public string Label => def?.label ?? string.Empty;
        public string Description => def?.description ?? string.Empty;

        public virtual bool HasHitEffect => false;
        public virtual bool HasKillEffect => false;

        /// <summary>Called when the equipped weapon hits a pawn. Effect may target the victim.</summary>
        public virtual void OnHitVictim(Pawn attacker, Pawn victim, Thing weapon, float severityOverride = -1f) { }

        /// <summary>Called when the equipped weapon hits a pawn. Effect may target the attacker (self).</summary>
        public virtual void OnHitSelf(Pawn attacker, Pawn victim, Thing weapon, float severityOverride = -1f) { }

        /// <summary>Called when the equipped weapon kills a pawn. Effect targets the attacker (self) only.</summary>
        public virtual void OnKillPawn(Pawn attacker, Pawn killed, Thing weapon, float severityOverride = -1f) { }
    }

    /// <summary>
    /// XML-defined Def for a weapon blessing.
    /// Set workerClass to control which DW_Blessing subclass handles effects.
    /// </summary>
    public class DW_BlessingDef : Def
    {
        /// <summary>Worker class (must extend DW_Blessing).</summary>
        public Type workerClass = typeof(DW_Blessing_HitEffect);

        // --- Hit effect fields ---
        /// <summary>Hediff applied to the hit victim.</summary>
        public HediffDef hitVictimHediff;
        /// <summary>Hediff applied to the attacker (self) on hit.</summary>
        public HediffDef hitSelfHediff;
        /// <summary>Severity of hit hediffs.</summary>
        public float hediffHitSeverity = 0.1f;

        public int hediffLevelAddPerHit = 1;

        // --- Kill effect fields ---
        /// <summary>Hediff applied to the attacker (self) on kill.</summary>
        public HediffDef killSelfHediff;
        /// <summary>Severity of kill hediff.</summary>
        public float hediffKillSeverity = 0.2f;

        public int hediffLevelAddPerKill = 1;
        
        private DW_Blessing cachedWorker;

        public DW_Blessing GetWorker()
        {
            if (cachedWorker == null)
            {
                cachedWorker = (DW_Blessing)Activator.CreateInstance(workerClass);
                cachedWorker.def = this;
            }
            return cachedWorker;
        }
    }

    /// <summary>
    /// Abstract stateless worker for weapon perks.
    /// Perks use DW_WeaponComp as the modification mechanism (RimWorld 1.6 WeaponComp pattern).
    /// </summary>
    public abstract class DW_EquipmentPerk
    {
        public DW_EquipmentPerkDef def;

        public string Label => def?.label ?? string.Empty;
        public string Description => def?.description ?? string.Empty;

        /// <summary>Called when this perk is installed on a weapon.</summary>
        public virtual void OnInstalled(Thing weapon) { }

        /// <summary>Called when this perk is removed from a weapon.</summary>
        public virtual void OnRemoved(Thing weapon) { }
    }

    /// <summary>Default perk worker with no special logic.</summary>
    public class DW_EquipmentPerkDefault : DW_EquipmentPerk { }

    /// <summary>
    /// XML-defined Def for a weapon perk.
    /// Uses DW_WeaponCompProperties as the modification mechanism, following RimWorld 1.6 WeaponComp patterns.
    /// </summary>
    public class DW_EquipmentPerkDef : Def
    {
        /// <summary>Worker class (must extend DW_EquipmentPerk).</summary>
        public Type perkClass = typeof(DW_EquipmentPerkDefault);

        /// <summary>
        /// Weapon stat modifications this perk provides.
        /// Using DW_WeaponComp modification system as the modification item.
        /// </summary>
        public List<StatModifier> statOffsets;

        /// <summary>
        /// Optional WeaponComp properties for passive comp-based effects.
        /// Based on RimWorld 1.6 WeaponComp modification pattern.
        /// </summary>
        public DW_WeaponCompProperties compProperties;

        private DW_EquipmentPerk cachedWorker;

        public DW_EquipmentPerk GetWorker()
        {
            if (cachedWorker == null)
            {
                cachedWorker = (DW_EquipmentPerk)Activator.CreateInstance(perkClass);
                cachedWorker.def = this;
            }
            return cachedWorker;
        }
    }
}
