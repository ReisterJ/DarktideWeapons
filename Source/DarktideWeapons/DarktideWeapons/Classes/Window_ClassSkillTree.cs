using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Classes
{
    /// <summary>
    /// Displays the active class's skill tree and lets the player spend skill points.
    ///
    /// Layout (top-to-bottom, scrollable vertically):
    ///   Header row: class name | skill-point count | Reset All button
    ///   Scroll area: tree nodes drawn with connecting lines
    ///
    /// Node states &amp; colours:
    ///   Unlocked   – bright green tint
    ///   Unlockable – white / normal
    ///   Locked     – dark / greyed out
    ///
    /// Clicking an unlockable node spends skill points and unlocks it.
    /// Clicking an already-unlocked or locked node shows a tooltip reason.
    /// </summary>
    public class Window_ClassSkillTree : Window
    {
        // ── Constants ───────────────────────────────────────────────────────────
        private const float NodeW     = 84f;
        private const float NodeH     = 84f;
        private const float HorGap    = 24f;
        private const float VerGap    = 56f;
        private const float IconSize  = 48f;
        private const float LineWidth = 2f;
        private const float HeaderH   = 36f;

        private static readonly Color ColUnlocked   = new Color(0.35f, 0.85f, 0.35f);
        private static readonly Color ColUnlockable = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color ColLocked     = new Color(0.30f, 0.30f, 0.30f);
        private static readonly Color ColLine       = new Color(0.55f, 0.55f, 0.55f);
        private static readonly Color ColLineActive = new Color(0.45f, 0.85f, 0.45f);

        // ── State ───────────────────────────────────────────────────────────────
        private readonly Hediff_ClassSystem hediff;
        private Vector2 scrollPos = Vector2.zero;

        // Computed layout: node → top-left position within the virtual canvas.
        private Dictionary<DW_ClassNodeDef, Vector2> nodePositions;
        private Vector2 canvasSize;
        private bool layoutDirty = true;

        public override Vector2 InitialSize => new Vector2(720f, 600f);

        public Window_ClassSkillTree(Hediff_ClassSystem hediff)
        {
            this.hediff          = hediff;
            this.forcePause      = false;
            this.doCloseButton   = true;
            this.doCloseX        = true;
            this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside   = false;
        }

        // ── Layout computation ──────────────────────────────────────────────────

        private void EnsureLayout()
        {
            if (!layoutDirty && nodePositions != null) return;
            nodePositions = new Dictionary<DW_ClassNodeDef, Vector2>();
            if (hediff.chosenClass?.rootNode == null) return;

            float usedWidth = 0f;
            LayoutNode(hediff.chosenClass.rootNode, 0, 0, ref usedWidth);
            float maxDepth = MaxDepth(hediff.chosenClass.rootNode, 0);
            canvasSize = new Vector2(usedWidth, (maxDepth + 1) * (NodeH + VerGap));
            layoutDirty = false;
        }

        // Returns the width consumed by this subtree.
        private float LayoutNode(DW_ClassNodeDef node, float x, int depth, ref float cursor)
        {
            if (node.childNodes.NullOrEmpty())
            {
                nodePositions[node] = new Vector2(x, depth * (NodeH + VerGap));
                cursor = x + NodeW;
                return NodeW;
            }

            float subtreeStart = x;
            float localCursor  = x;
            foreach (var child in node.childNodes)
            {
                float w = LayoutNode(child, localCursor, depth + 1, ref localCursor);
                localCursor += HorGap;
            }
            localCursor -= HorGap; // remove trailing gap

            float subtreeWidth = localCursor - subtreeStart;
            float centerX = subtreeStart + (subtreeWidth - NodeW) * 0.5f;
            nodePositions[node] = new Vector2(centerX, depth * (NodeH + VerGap));
            cursor = localCursor;
            return subtreeWidth;
        }

        private int MaxDepth(DW_ClassNodeDef node, int depth)
        {
            if (node.childNodes.NullOrEmpty()) return depth;
            int max = depth;
            foreach (var c in node.childNodes)
                max = System.Math.Max(max, MaxDepth(c, depth + 1));
            return max;
        }

        // ── Drawing ─────────────────────────────────────────────────────────────

        public override void DoWindowContents(Rect inRect)
        {
            if (hediff.chosenClass == null) { Close(); return; }
            EnsureLayout();

            // Header
            DrawHeader(new Rect(inRect.x, inRect.y, inRect.width, HeaderH));
            inRect.yMin += HeaderH + 6f;

            // Scroll view
            float viewW = Mathf.Max(inRect.width, canvasSize.x + 20f);
            float viewH = Mathf.Max(inRect.height, canvasSize.y + 20f);
            Rect scrollOuter = inRect;
            Rect scrollView  = new Rect(0f, 0f, viewW, viewH);

            bool wantRebuild = false;

            Widgets.BeginScrollView(scrollOuter, ref scrollPos, scrollView);
            Vector2 origin = new Vector2(10f, 10f);

            // Draw lines first (below nodes)
            DrawConnections(hediff.chosenClass.rootNode, origin);

            // Draw nodes, collect click
            DrawNodes(hediff.chosenClass.rootNode, origin, ref wantRebuild);

            Widgets.EndScrollView();

            if (wantRebuild) layoutDirty = true;
        }

        private void DrawHeader(Rect rect)
        {
            // Class name
            Text.Font = GameFont.Medium;
            float nameW = rect.width * 0.45f;
            Widgets.Label(new Rect(rect.x, rect.y, nameW, rect.height),
                hediff.chosenClass.LabelCap);
            Text.Font = GameFont.Small;

            // Skill points
            string spLabel = "DW_Class_SP".Translate(hediff.skillPoints);
            float spW = Text.CalcSize(spLabel).x + 10f;
            Widgets.Label(new Rect(rect.x + nameW, rect.y + (rect.height - Text.LineHeight) * 0.5f, spW, rect.height),
                spLabel);

            // Reset button
            float btnW = 120f;
            Rect resetBtn = new Rect(rect.xMax - btnW, rect.y + (rect.height - 28f) * 0.5f, btnW, 28f);
            if (Widgets.ButtonText(resetBtn, "DW_Class_ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "DW_Class_ResetConfirm".Translate(),
                    () =>
                    {
                        hediff.ResetAllNodes();
                        layoutDirty = true;
                    }));
            }
        }

        private void DrawConnections(DW_ClassNodeDef node, Vector2 origin)
        {
            if (node.childNodes.NullOrEmpty()) return;
            if (!nodePositions.TryGetValue(node, out var parentPos)) return;

            Vector2 parentBottom = new Vector2(
                origin.x + parentPos.x + NodeW * 0.5f,
                origin.y + parentPos.y + NodeH);

            bool parentUnlocked = hediff.IsNodeUnlocked(node);

            foreach (var child in node.childNodes)
            {
                if (!nodePositions.TryGetValue(child, out var childPos)) continue;
                Vector2 childTop = new Vector2(
                    origin.x + childPos.x + NodeW * 0.5f,
                    origin.y + childPos.y);

                Color lineColor = (parentUnlocked && hediff.IsNodeUnlocked(child))
                    ? ColLineActive : ColLine;
                Widgets.DrawLine(parentBottom, childTop, lineColor, LineWidth);

                DrawConnections(child, origin);
            }
        }

        private void DrawNodes(DW_ClassNodeDef node, Vector2 origin, ref bool wantRebuild)
        {
            if (!nodePositions.TryGetValue(node, out var pos)) return;

            Rect nodeRect = new Rect(origin.x + pos.x, origin.y + pos.y, NodeW, NodeH);

            bool unlocked   = hediff.IsNodeUnlocked(node);
            bool unlockable = !unlocked && hediff.CanUnlockNode(node);

            // Background
            Color bg = unlocked ? ColUnlocked : (unlockable ? ColUnlockable : ColLocked);
            Widgets.DrawBoxSolid(nodeRect, bg * 0.4f);
            Widgets.DrawBox(nodeRect, 1);

            // Icon
            Rect iconRect = new Rect(
                nodeRect.x + (NodeW - IconSize) * 0.5f,
                nodeRect.y + 4f,
                IconSize, IconSize);
            GUI.color = unlocked ? Color.white : (unlockable ? new Color(1f, 1f, 1f, 0.75f) : new Color(1f, 1f, 1f, 0.3f));
            GUI.DrawTexture(iconRect, node.Icon);
            GUI.color = Color.white;

            // Type badge (top-left corner)
            Text.Font = GameFont.Tiny;
            string badge = node.nodeType == ClassNodeType.Skill ? "ACT" : "PSV";
            GUI.color = node.nodeType == ClassNodeType.Skill ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.85f, 0.4f);
            Widgets.Label(new Rect(nodeRect.x + 2f, nodeRect.y + 2f, 28f, 14f), badge);
            GUI.color = Color.white;

            // Label
            Text.Font = GameFont.Tiny;
            float labelY = iconRect.yMax + 2f;
            Widgets.Label(new Rect(nodeRect.x, labelY, NodeW, NodeH - (labelY - nodeRect.y)),
                node.LabelCap.Truncate(NodeW - 4f));
            Text.Font = GameFont.Small;

            // Tooltip
            string costStr = node.skillPointCost == 1
                ? "DW_Class_CostSingle".Translate()
                : "DW_Class_CostPlural".Translate(node.skillPointCost);
            string tooltip = node.description + "\n\n" + costStr;
            if (unlocked)                    tooltip += "\n(" + "DW_Class_Unlocked".Translate() + ")";
            else if (!unlockable && !unlocked) tooltip += "\n(" + "DW_Class_Locked".Translate() + ")";
            TooltipHandler.TipRegion(nodeRect, tooltip);

            // Click interaction
            if (Widgets.ButtonInvisible(nodeRect))
            {
                if (unlocked)
                {
                    Messages.Message("DW_Class_AlreadyUnlocked".Translate(node.LabelCap),
                        MessageTypeDefOf.RejectInput);
                }
                else if (unlockable)
                {
                    hediff.UnlockNode(node);
                    wantRebuild = true;
                }
                else
                {
                    if (hediff.skillPoints < node.skillPointCost)
                        Messages.Message("DW_Class_NotEnoughSP".Translate(node.LabelCap,
                            node.skillPointCost, hediff.skillPoints), MessageTypeDefOf.RejectInput);
                    else
                        Messages.Message("DW_Class_NoParent".Translate(node.LabelCap),
                            MessageTypeDefOf.RejectInput);
                }
            }

            // Recurse children
            if (!node.childNodes.NullOrEmpty())
                foreach (var child in node.childNodes)
                    DrawNodes(child, origin, ref wantRebuild);
        }
    }
}
