using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Career
{
    /// <summary>
    /// Core hediff that tracks a pawn's career choice, skill points, and unlocked nodes.
    /// Applied by using the career-grant item. Persists until manually removed.
    /// Provides gizmos: "Choose Career" before a career is chosen, "Career" icon afterwards.
    /// </summary>
    public class Hediff_CareerSystem : Hediff
    {
        // ── Persistent state ────────────────────────────────────────────────────

        public DW_CareerDef chosenCareer;
        public int skillPoints = 0;

        // Stored as List<string> for save/load; use Contains() for lookup (small list).
        private List<string> unlockedNodeDefNames = new List<string>();

        // ── Dynamic stat stage ──────────────────────────────────────────────────

        private HediffStage dynamicStage = new HediffStage();
        private bool stageDirty = true;

        public override HediffStage CurStage
        {
            get
            {
                if (stageDirty) RebuildStage();
                return dynamicStage;
            }
        }

        // Never auto-remove; the player removes this intentionally (or it stays forever).
        public override bool ShouldRemove => false;

        // ── Node queries ────────────────────────────────────────────────────────

        public bool IsNodeUnlocked(DW_CareerNodeDef node)
            => unlockedNodeDefNames.Contains(node.defName);

        /// <summary>
        /// Returns whether the pawn is allowed to unlock this node right now.
        /// Rules: must have enough skill points, node must not already be unlocked,
        /// and either the node IS the root, or at least one of its parents is already unlocked.
        /// </summary>
        public bool CanUnlockNode(DW_CareerNodeDef node)
        {
            if (chosenCareer == null) return false;
            if (IsNodeUnlocked(node)) return false;
            if (skillPoints < node.skillPointCost) return false;

            // Root is always accessible while a career is set.
            if (chosenCareer.rootNode == node) return true;

            // Otherwise a parent must be unlocked.
            return HasUnlockedParentIn(node, chosenCareer.rootNode);
        }

        // DFS: returns true if `target` is a direct child of an unlocked node.
        private bool HasUnlockedParentIn(DW_CareerNodeDef target, DW_CareerNodeDef current)
        {
            if (current == null || current.childNodes.NullOrEmpty()) return false;
            foreach (var child in current.childNodes)
            {
                if (child == target) return IsNodeUnlocked(current);
                if (HasUnlockedParentIn(target, child)) return true;
            }
            return false;
        }

        // ── Node operations ─────────────────────────────────────────────────────

        public void UnlockNode(DW_CareerNodeDef node)
        {
            if (!CanUnlockNode(node))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Career] UnlockNode REJECTED for '{node.defName}' on {pawn.LabelShort}: " +
                        $"chosenCareer={chosenCareer?.defName ?? "null"}, " +
                        $"alreadyUnlocked={IsNodeUnlocked(node)}, " +
                        $"SP={skillPoints}/{node.skillPointCost}, " +
                        $"hasUnlockedParent={HasUnlockedParentIn(node, chosenCareer?.rootNode)}");
                return;
            }

            int spBefore = skillPoints;
            skillPoints -= node.skillPointCost;
            unlockedNodeDefNames.Add(node.defName);

            if (node.nodeType == CareerNodeType.Skill && node.abilityDef != null)
            {
                pawn.abilities.GainAbility(node.abilityDef);
                if (DebugSettings.godMode)
                    Log.Message($"[DW Career] Granted ability '{node.abilityDef.defName}' to {pawn.LabelShort}");
            }

            if (DebugSettings.godMode)
                Log.Message($"[DW Career] Unlocked node '{node.defName}' ({node.nodeType}) on {pawn.LabelShort}. " +
                    $"SP: {spBefore} -> {skillPoints}. " +
                    $"Total unlocked nodes: {unlockedNodeDefNames.Count}");

            stageDirty = true;
        }

        /// <summary>Refunds all spent skill points and removes all unlocked nodes.</summary>
        public void ResetAllNodes()
        {
            if (DebugSettings.godMode)
                Log.Message($"[DW Career] ResetAllNodes called for {pawn.LabelShort}. " +
                    $"Career: {chosenCareer?.defName ?? "null"}, " +
                    $"Nodes to refund: [{string.Join(", ", unlockedNodeDefNames)}], " +
                    $"Current SP: {skillPoints}");

            int refunded = 0;
            int abilitiesRemoved = 0;
            foreach (var defName in unlockedNodeDefNames)
            {
                var nodeDef = DefDatabase<DW_CareerNodeDef>.GetNamedSilentFail(defName);
                if (nodeDef == null)
                {
                    if (DebugSettings.godMode)
                        Log.Warning($"[DW Career] ResetAllNodes: could not find DW_CareerNodeDef '{defName}' – skipping.");
                    continue;
                }
                refunded += nodeDef.skillPointCost;

                if (nodeDef.nodeType == CareerNodeType.Skill && nodeDef.abilityDef != null)
                {
                    pawn.abilities.RemoveAbility(nodeDef.abilityDef);
                    abilitiesRemoved++;
                    if (DebugSettings.godMode)
                        Log.Message($"[DW Career]   Removed ability '{nodeDef.abilityDef.defName}' from {pawn.LabelShort}");
                }
            }
            skillPoints += refunded;
            unlockedNodeDefNames.Clear();
            stageDirty = true;

            if (DebugSettings.godMode)
                Log.Message($"[DW Career] ResetAllNodes complete for {pawn.LabelShort}. " +
                    $"Refunded {refunded} SP (now {skillPoints}), removed {abilitiesRemoved} abilities.");
        }

        /// <summary>Selects a career. Auto-unlocks the root node for free.</summary>
        public void SelectCareer(DW_CareerDef chosen)
        {
            if (chosenCareer != null)
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Career] SelectCareer ignored for {pawn.LabelShort}: already has career '{chosenCareer.defName}'.");
                return;
            }
            if (chosen == null)
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Career] SelectCareer called with null career for {pawn.LabelShort}.");
                return;
            }
            chosenCareer = chosen;
            if (DebugSettings.godMode)
                Log.Message($"[DW Career] {pawn.LabelShort} selected career '{chosenCareer.defName}'. " +
                    $"Root node: '{chosenCareer.rootNode?.defName ?? "null"}'.");

            // Auto-unlock root at no cost so the tree always has a starting point.
            if (chosenCareer.rootNode != null)
            {
                unlockedNodeDefNames.Add(chosenCareer.rootNode.defName);
                if (DebugSettings.godMode)
                    Log.Message($"[DW Career] Auto-unlocked root node '{chosenCareer.rootNode.defName}' at no SP cost.");
            }
            stageDirty = true;
        }

        // ── Stat stage rebuild ──────────────────────────────────────────────────

        private void RebuildStage()
        {
            var offsets = new List<StatModifier>();

            foreach (var defName in unlockedNodeDefNames)
            {
                var nodeDef = DefDatabase<DW_CareerNodeDef>.GetNamedSilentFail(defName);
                if (nodeDef == null)
                {
                    if (DebugSettings.godMode)
                        Log.Warning($"[DW Career] RebuildStage: missing DW_CareerNodeDef '{defName}' for {pawn?.LabelShort}.");
                    continue;
                }
                if (nodeDef.nodeType != CareerNodeType.Perk) continue;
                if (nodeDef.statOffsets.NullOrEmpty()) continue;

                foreach (var offset in nodeDef.statOffsets)
                {
                    var existing = offsets.Find(s => s.stat == offset.stat);
                    if (existing != null)
                        existing.value += offset.value;
                    else
                        offsets.Add(new StatModifier { stat = offset.stat, value = offset.value });
                }
            }

            dynamicStage.statOffsets = offsets;
            stageDirty = false;
        }

        // ── Gizmos ──────────────────────────────────────────────────────────────

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (chosenCareer == null)
            {
                var chooseCmd = new Command_Action
                {
                    defaultLabel = "DW_Career_ChooseLabel".Translate(),
                    defaultDesc  = "DW_Career_ChooseDesc".Translate(),
                    icon         = BaseContent.BadTex,
                    action       = () => Find.WindowStack.Add(new Window_CareerSelect(this))
                };
                yield return chooseCmd;
            }
            else
            {
                var iconCmd = new Command_Action
                {
                    defaultLabel = chosenCareer.LabelCap,
                    defaultDesc  = "DW_Career_IconDesc".Translate(chosenCareer.LabelCap, skillPoints),
                    icon         = chosenCareer.Icon,
                    action       = () => Find.WindowStack.Add(new Window_CareerSkillTree(this))
                };
                yield return iconCmd;
            }

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Career +5 SP",
                    defaultDesc  = $"Adds 5 skill points.\nCurrent SP: {skillPoints}\nCareer: {chosenCareer?.defName ?? "none"}\nUnlocked nodes: {unlockedNodeDefNames.Count}",
                    action       = () =>
                    {
                        skillPoints += 5;
                        Log.Message($"[DW Career] DEV +5 SP for {pawn.LabelShort}. Total now: {skillPoints}");
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: Career Dump State",
                    defaultDesc  = "Prints full career state to the log.",
                    action       = () => DumpStateToLog()
                };

                if (chosenCareer != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Career Force Reset",
                        defaultDesc  = "Removes the chosen career so a new one can be picked.",
                        action       = () =>
                        {
                            Log.Message($"[DW Career] DEV: force-clearing career from {pawn.LabelShort} (was '{chosenCareer.defName}').");
                            ResetAllNodes();
                            chosenCareer = null;
                            stageDirty = true;
                        }
                    };
                }
            }
        }

        // ── Developer dump ───────────────────────────────────────────────────────

        private void DumpStateToLog()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[DW Career] ===== Career State Dump for {pawn.LabelShort} =====");
            sb.AppendLine($"  Career     : {chosenCareer?.defName ?? "(none selected)"}");
            sb.AppendLine($"  Skill Pts  : {skillPoints}");
            sb.AppendLine($"  Unlocked ({unlockedNodeDefNames.Count}):");
            foreach (var defName in unlockedNodeDefNames)
            {
                var nd = DefDatabase<DW_CareerNodeDef>.GetNamedSilentFail(defName);
                if (nd == null)
                    sb.AppendLine($"    [MISSING DEF] {defName}");
                else
                {
                    string extra = nd.nodeType == CareerNodeType.Skill
                        ? $"ability={nd.abilityDef?.defName ?? "null"}"
                        : $"offsets=[{string.Join(", ", nd.statOffsets?.Select(s => $"{s.stat.defName}:{s.value:+0.##;-0.##}") ?? System.Linq.Enumerable.Empty<string>())}]";
                    sb.AppendLine($"    {defName} ({nd.nodeType}, cost={nd.skillPointCost}, {extra})");
                }
            }
            if (chosenCareer != null)
            {
                sb.AppendLine($"  Available to unlock:");
                foreach (var nd in DefDatabase<DW_CareerNodeDef>.AllDefsListForReading)
                {
                    if (CanUnlockNode(nd))
                        sb.AppendLine($"    {nd.defName} (cost={nd.skillPointCost})");
                }
            }
            sb.AppendLine($"[DW Career] ===== End Dump =====");
            Log.Message(sb.ToString());
        }

        // ── Persistence ──────────────────────────────────────────────────────────

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref chosenCareer, "chosenCareer");
            Scribe_Values.Look(ref skillPoints, "skillPoints", 0);
            Scribe_Collections.Look(ref unlockedNodeDefNames, "unlockedNodeDefNames", LookMode.Value);
            if (unlockedNodeDefNames == null) unlockedNodeDefNames = new List<string>();
        }
    }
}
