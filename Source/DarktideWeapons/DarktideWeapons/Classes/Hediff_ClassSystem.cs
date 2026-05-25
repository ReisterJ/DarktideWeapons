using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Classes
{
    /// <summary>
    /// Core hediff that tracks a pawn's class choice, skill points, and unlocked nodes.
    /// Applied by using the class-grant item. Persists until manually removed.
    /// Provides gizmos: "Choose Class" before a class is chosen, "Class" icon afterwards.
    /// </summary>
    public class Hediff_ClassSystem : Hediff
    {
        // ── Persistent state ────────────────────────────────────────────────────

        public DW_ClassDef chosenClass;
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

        public bool IsNodeUnlocked(DW_ClassNodeDef node)
            => unlockedNodeDefNames.Contains(node.defName);

        /// <summary>
        /// Returns whether the pawn is allowed to unlock this node right now.
        /// Rules: must have enough skill points, node must not already be unlocked,
        /// and either the node IS the root, or at least one of its parents is already unlocked.
        /// </summary>
        public bool CanUnlockNode(DW_ClassNodeDef node)
        {
            if (chosenClass == null) return false;
            if (IsNodeUnlocked(node)) return false;
            if (skillPoints < node.skillPointCost) return false;

            // Root is always accessible while a class is set.
            if (chosenClass.rootNode == node) return true;

            // Otherwise a parent must be unlocked.
            return HasUnlockedParentIn(node, chosenClass.rootNode);
        }

        // DFS: returns true if `target` is a direct child of an unlocked node.
        private bool HasUnlockedParentIn(DW_ClassNodeDef target, DW_ClassNodeDef current)
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

        public void UnlockNode(DW_ClassNodeDef node)
        {
            if (!CanUnlockNode(node))
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Class] UnlockNode REJECTED for '{node.defName}' on {pawn.LabelShort}: " +
                        $"chosenClass={chosenClass?.defName ?? "null"}, " +
                        $"alreadyUnlocked={IsNodeUnlocked(node)}, " +
                        $"SP={skillPoints}/{node.skillPointCost}, " +
                        $"hasUnlockedParent={HasUnlockedParentIn(node, chosenClass?.rootNode)}");
                return;
            }

            int spBefore = skillPoints;
            skillPoints -= node.skillPointCost;
            unlockedNodeDefNames.Add(node.defName);

            if (node.nodeType == ClassNodeType.Skill && node.abilityDef != null)
            {
                pawn.abilities.GainAbility(node.abilityDef);
                if (DebugSettings.godMode)
                    Log.Message($"[DW Class] Granted ability '{node.abilityDef.defName}' to {pawn.LabelShort}");
            }

            if (DebugSettings.godMode)
                Log.Message($"[DW Class] Unlocked node '{node.defName}' ({node.nodeType}) on {pawn.LabelShort}. " +
                    $"SP: {spBefore} -> {skillPoints}. " +
                    $"Total unlocked nodes: {unlockedNodeDefNames.Count}");

            stageDirty = true;
        }

        /// <summary>Refunds all spent skill points and removes all unlocked nodes.</summary>
        public void ResetAllNodes()
        {
            if (DebugSettings.godMode)
                Log.Message($"[DW Class] ResetAllNodes called for {pawn.LabelShort}. " +
                    $"Class: {chosenClass?.defName ?? "null"}, " +
                    $"Nodes to refund: [{string.Join(", ", unlockedNodeDefNames)}], " +
                    $"Current SP: {skillPoints}");

            int refunded = 0;
            int abilitiesRemoved = 0;
            foreach (var defName in unlockedNodeDefNames)
            {
                var nodeDef = DefDatabase<DW_ClassNodeDef>.GetNamedSilentFail(defName);
                if (nodeDef == null)
                {
                    if (DebugSettings.godMode)
                        Log.Warning($"[DW Class] ResetAllNodes: could not find DW_ClassNodeDef '{defName}' – skipping.");
                    continue;
                }
                refunded += nodeDef.skillPointCost;

                if (nodeDef.nodeType == ClassNodeType.Skill && nodeDef.abilityDef != null)
                {
                    pawn.abilities.RemoveAbility(nodeDef.abilityDef);
                    abilitiesRemoved++;
                    if (DebugSettings.godMode)
                        Log.Message($"[DW Class]   Removed ability '{nodeDef.abilityDef.defName}' from {pawn.LabelShort}");
                }
            }
            skillPoints += refunded;
            unlockedNodeDefNames.Clear();
            stageDirty = true;

            if (DebugSettings.godMode)
                Log.Message($"[DW Class] ResetAllNodes complete for {pawn.LabelShort}. " +
                    $"Refunded {refunded} SP (now {skillPoints}), removed {abilitiesRemoved} abilities.");
        }

        /// <summary>Selects a class. Auto-unlocks the root node for free.</summary>
        public void SelectClass(DW_ClassDef chosen)
        {
            if (chosenClass != null)
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Class] SelectClass ignored for {pawn.LabelShort}: already has class '{chosenClass.defName}'.");
                return;
            }
            if (chosen == null)
            {
                if (DebugSettings.godMode)
                    Log.Warning($"[DW Class] SelectClass called with null class for {pawn.LabelShort}.");
                return;
            }
            chosenClass = chosen;
            if (DebugSettings.godMode)
                Log.Message($"[DW Class] {pawn.LabelShort} selected class '{chosenClass.defName}'. " +
                    $"Root node: '{chosenClass.rootNode?.defName ?? "null"}'.");

            // Auto-unlock root at no cost so the tree always has a starting point.
            if (chosenClass.rootNode != null)
            {
                unlockedNodeDefNames.Add(chosenClass.rootNode.defName);
                if (DebugSettings.godMode)
                    Log.Message($"[DW Class] Auto-unlocked root node '{chosenClass.rootNode.defName}' at no SP cost.");
            }
            stageDirty = true;
        }

        // ── Stat stage rebuild ──────────────────────────────────────────────────

        private void RebuildStage()
        {
            var offsets = new List<StatModifier>();

            foreach (var defName in unlockedNodeDefNames)
            {
                var nodeDef = DefDatabase<DW_ClassNodeDef>.GetNamedSilentFail(defName);
                if (nodeDef == null)
                {
                    if (DebugSettings.godMode)
                        Log.Warning($"[DW Class] RebuildStage: missing DW_ClassNodeDef '{defName}' for {pawn?.LabelShort}.");
                    continue;
                }
                if (nodeDef.nodeType != ClassNodeType.Perk) continue;
                if (nodeDef.statOffsets.NullOrEmpty()) continue;

                foreach (var offset in nodeDef.statOffsets)
                {
                    var existing = offsets.Find(s => s.stat == offset.stat);
                    if (existing != null)
                    {
                        if (DebugSettings.godMode)
                            Log.Message($"[DW Class]   RebuildStage: stacking {offset.stat.defName} +{offset.value} " +
                                $"(from '{nodeDef.defName}') -> new total: {existing.value + offset.value}");
                        existing.value += offset.value;
                    }
                    else
                    {
                        if (DebugSettings.godMode)
                            Log.Message($"[DW Class]   RebuildStage: adding {offset.stat.defName} +{offset.value} " +
                                $"(from '{nodeDef.defName}')");
                        offsets.Add(new StatModifier { stat = offset.stat, value = offset.value });
                    }
                }
            }

            if (DebugSettings.godMode)
                Log.Message($"[DW Class] RebuildStage for {pawn?.LabelShort}: " +
                    $"{offsets.Count} active stat offsets from {unlockedNodeDefNames.Count} unlocked nodes.");

            dynamicStage.statOffsets = offsets;
            stageDirty = false;
        }

        // ── Gizmos ──────────────────────────────────────────────────────────────

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (chosenClass == null)
            {
                // ── Choose Class ──
                var chooseCmd = new Command_Action
                {
                    defaultLabel = "DW_Class_ChooseLabel".Translate(),
                    defaultDesc  = "DW_Class_ChooseDesc".Translate(),
                    icon         = BaseContent.BadTex,
                    action       = () => Find.WindowStack.Add(new Window_ClassSelect(this))
                };
                yield return chooseCmd;
            }
            else
            {
                // ── Class icon ──
                var iconCmd = new Command_Action
                {
                    defaultLabel = chosenClass.LabelCap,
                    defaultDesc  = "DW_Class_IconDesc".Translate(chosenClass.LabelCap, skillPoints),
                    icon         = chosenClass.Icon,
                    action       = () => Find.WindowStack.Add(new Window_ClassSkillTree(this))
                };
                yield return iconCmd;
            }

            // Developer commands (only visible in god mode).
            if (DebugSettings.godMode)
            {
                var devAddSP = new Command_Action
                {
                    defaultLabel = "DEV: Class +5 SP",
                    defaultDesc  = $"Adds 5 skill points.\nCurrent SP: {skillPoints}\nClass: {chosenClass?.defName ?? "none"}\nUnlocked nodes: {unlockedNodeDefNames.Count}",
                    action       = () =>
                    {
                        skillPoints += 5;
                        Log.Message($"[DW Class] DEV +5 SP for {pawn.LabelShort}. Total now: {skillPoints}");
                    }
                };
                yield return devAddSP;

                var devDump = new Command_Action
                {
                    defaultLabel = "DEV: Class Dump State",
                    defaultDesc  = "Prints full class state to the log.",
                    action       = () => DumpStateToLog()
                };
                yield return devDump;

                if (chosenClass != null)
                {
                    var devReset = new Command_Action
                    {
                        defaultLabel = "DEV: Class Force Reset Class",
                        defaultDesc  = "Removes the chosen class so a new one can be picked.",
                        action       = () =>
                        {
                            Log.Message($"[DW Class] DEV: force-clearing class from {pawn.LabelShort} (was '{chosenClass.defName}').");
                            ResetAllNodes();
                            chosenClass = null;
                            stageDirty = true;
                        }
                    };
                    yield return devReset;
                }
            }
        }

        // ── Developer dump ───────────────────────────────────────────────────────

        private void DumpStateToLog()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[DW Class] ===== Class State Dump for {pawn.LabelShort} =====");
            sb.AppendLine($"  Class      : {chosenClass?.defName ?? "(none selected)"}");
            sb.AppendLine($"  Skill Pts  : {skillPoints}");
            sb.AppendLine($"  Unlocked ({unlockedNodeDefNames.Count}):");
            foreach (var defName in unlockedNodeDefNames)
            {
                var nd = DefDatabase<DW_ClassNodeDef>.GetNamedSilentFail(defName);
                if (nd == null)
                    sb.AppendLine($"    [MISSING DEF] {defName}");
                else
                {
                    string extra = nd.nodeType == ClassNodeType.Skill
                        ? $"ability={nd.abilityDef?.defName ?? "null"}"
                        : $"offsets=[{string.Join(", ", nd.statOffsets?.Select(s => $"{s.stat.defName}:{s.value:+0.##;-0.##}") ?? System.Linq.Enumerable.Empty<string>())}]";
                    sb.AppendLine($"    {defName} ({nd.nodeType}, cost={nd.skillPointCost}, {extra})");
                }
            }
            if (chosenClass != null)
            {
                sb.AppendLine($"  Available to unlock:");
                foreach (var nd in DefDatabase<DW_ClassNodeDef>.AllDefsListForReading)
                {
                    if (CanUnlockNode(nd))
                        sb.AppendLine($"    {nd.defName} (cost={nd.skillPointCost})");
                }
            }
            sb.AppendLine($"  Active stat offsets from CurStage:");
            if (CurStage.statOffsets != null)
                foreach (var s in CurStage.statOffsets)
                    sb.AppendLine($"    {s.stat.defName}: {s.value:+0.###;-0.###}");
            sb.AppendLine($"[DW Class] ===== End Dump =====");
            Log.Message(sb.ToString());
        }

        // ── Persistence ──────────────────────────────────────────────────────────

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref chosenClass, "chosenClass");
            Scribe_Values.Look(ref skillPoints, "skillPoints", 0);
            Scribe_Collections.Look(ref unlockedNodeDefNames, "unlockedNodeDefNames", LookMode.Value);
            if (unlockedNodeDefNames == null) unlockedNodeDefNames = new List<string>();
        }
    }
}
