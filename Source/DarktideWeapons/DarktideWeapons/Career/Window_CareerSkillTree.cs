using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Career
{
    /// <summary>
    /// Displays the active career's skill tree and lets the player spend skill points.
    /// Mirrors Window_ClassSkillTree from the Classes system.
    /// </summary>
    public class Window_CareerSkillTree : Window
    {
        // ── Constants ───────────────────────────────────────────────────────────
        private const float NodeW    = 84f;
        private const float NodeH    = 84f;
        private const float HorGap   = 24f;
        private const float VerGap   = 56f;
        private const float IconSize = 48f;
        private const float LineWidth = 2f;
        private const float HeaderH  = 36f;

        private static readonly Color ColUnlocked   = new Color(0.35f, 0.85f, 0.35f);
        private static readonly Color ColUnlockable = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color ColLocked     = new Color(0.30f, 0.30f, 0.30f);
        private static readonly Color ColLine       = new Color(0.55f, 0.55f, 0.55f);
        private static readonly Color ColLineActive = new Color(0.45f, 0.85f, 0.45f);

        // ── State ───────────────────────────────────────────────────────────────
        private readonly Hediff_CareerSystem hediff;
        private Vector2 scrollPos = Vector2.zero;

        private Dictionary<DW_CareerNodeDef, Vector2> nodePositions;
        private Vector2 canvasSize;
        private bool layoutDirty = true;

        public override Vector2 InitialSize => new Vector2(720f, 600f);

        public Window_CareerSkillTree(Hediff_CareerSystem hediff)
        {
            this.hediff                  = hediff;
            this.forcePause              = false;
            this.doCloseButton           = true;
            this.doCloseX                = true;
            this.absorbInputAroundWindow  = false;
            this.closeOnClickedOutside   = false;
        }

        // ── Layout ──────────────────────────────────────────────────────────────

        private void EnsureLayout()
        {
            if (!layoutDirty && nodePositions != null) return;
            nodePositions = new Dictionary<DW_CareerNodeDef, Vector2>();
            if (hediff.chosenCareer?.rootNode == null) return;

            float usedWidth = 0f;
            LayoutNode(hediff.chosenCareer.rootNode, 0, 0, ref usedWidth);
            float maxDepth = MaxDepth(hediff.chosenCareer.rootNode, 0);
            canvasSize = new Vector2(usedWidth, (maxDepth + 1) * (NodeH + VerGap));
            layoutDirty = false;
        }

        private float LayoutNode(DW_CareerNodeDef node, float x, int depth, ref float cursor)
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
                LayoutNode(child, localCursor, depth + 1, ref localCursor);
                localCursor += HorGap;
            }
            localCursor -= HorGap;

            float subtreeWidth = localCursor - subtreeStart;
            float centerX = subtreeStart + (subtreeWidth - NodeW) * 0.5f;
            nodePositions[node] = new Vector2(centerX, depth * (NodeH + VerGap));
            cursor = localCursor;
            return subtreeWidth;
        }

        private int MaxDepth(DW_CareerNodeDef node, int depth)
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
            if (hediff.chosenCareer == null) { Close(); return; }
            EnsureLayout();

            DrawHeader(new Rect(inRect.x, inRect.y, inRect.width, HeaderH));
            inRect.yMin += HeaderH + 6f;

            float viewW = Mathf.Max(inRect.width, canvasSize.x + 20f);
            float viewH = Mathf.Max(inRect.height, canvasSize.y + 20f);
            Rect scrollOuter = inRect;
            Rect scrollView  = new Rect(0f, 0f, viewW, viewH);

            bool wantRebuild = false;

            Widgets.BeginScrollView(scrollOuter, ref scrollPos, scrollView);
            Vector2 origin = new Vector2(10f, 10f);

            DrawConnections(hediff.chosenCareer.rootNode, origin);
            DrawNodes(hediff.chosenCareer.rootNode, origin, ref wantRebuild);

            Widgets.EndScrollView();

            if (wantRebuild) layoutDirty = true;
        }

        private void DrawHeader(Rect rect)
        {
            Text.Font = GameFont.Medium;
            float nameW = rect.width * 0.45f;
            Widgets.Label(new Rect(rect.x, rect.y, nameW, rect.height), hediff.chosenCareer.LabelCap);
            Text.Font = GameFont.Small;

            string spLabel = "DW_Career_SP".Translate(hediff.skillPoints);
            float spW = Text.CalcSize(spLabel).x + 10f;
            Widgets.Label(new Rect(rect.x + nameW, rect.y + (rect.height - Text.LineHeight) * 0.5f, spW, rect.height),
                spLabel);

            float btnW = 120f;
            Rect resetBtn = new Rect(rect.xMax - btnW, rect.y + (rect.height - 28f) * 0.5f, btnW, 28f);
            if (Widgets.ButtonText(resetBtn, "DW_Career_ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "DW_Career_ResetConfirm".Translate(),
                    () =>
                    {
                        hediff.ResetAllNodes();
                        layoutDirty = true;
                    }));
            }
        }

        private void DrawConnections(DW_CareerNodeDef node, Vector2 origin)
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

        private void DrawNodes(DW_CareerNodeDef node, Vector2 origin, ref bool wantRebuild)
        {
            if (!nodePositions.TryGetValue(node, out var pos)) return;

            Rect nodeRect = new Rect(origin.x + pos.x, origin.y + pos.y, NodeW, NodeH);

            bool unlocked   = hediff.IsNodeUnlocked(node);
            bool unlockable = !unlocked && hediff.CanUnlockNode(node);

            Color bg = unlocked ? ColUnlocked : (unlockable ? ColUnlockable : ColLocked);
            Widgets.DrawBoxSolid(nodeRect, bg * 0.4f);
            Widgets.DrawBox(nodeRect, 1);

            Rect iconRect = new Rect(
                nodeRect.x + (NodeW - IconSize) * 0.5f,
                nodeRect.y + 4f,
                IconSize, IconSize);
            GUI.color = unlocked ? Color.white : (unlockable ? new Color(1f, 1f, 1f, 0.75f) : new Color(1f, 1f, 1f, 0.3f));
            GUI.DrawTexture(iconRect, node.Icon);
            GUI.color = Color.white;

            Text.Font = GameFont.Tiny;
            string badge = node.nodeType == CareerNodeType.Skill ? "ACT" : "PSV";
            GUI.color = node.nodeType == CareerNodeType.Skill ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.85f, 0.4f);
            Widgets.Label(new Rect(nodeRect.x + 2f, nodeRect.y + 2f, 28f, 14f), badge);
            GUI.color = Color.white;

            float labelY = iconRect.yMax + 2f;
            Widgets.Label(new Rect(nodeRect.x, labelY, NodeW, NodeH - (labelY - nodeRect.y)),
                node.LabelCap.Truncate(NodeW - 4f));
            Text.Font = GameFont.Small;

            string costStr = node.skillPointCost == 1
                ? "DW_Career_CostSingle".Translate()
                : "DW_Career_CostPlural".Translate(node.skillPointCost);
            string tooltip = node.description + "\n\n" + costStr;
            if (unlocked)
                tooltip += "\n(" + "DW_Career_Unlocked".Translate() + ")";
            else if (!unlockable)
                tooltip += "\n(" + "DW_Career_Locked".Translate() + ")";
            TooltipHandler.TipRegion(nodeRect, tooltip);

            if (Widgets.ButtonInvisible(nodeRect))
            {
                if (unlocked)
                    Messages.Message("DW_Career_AlreadyUnlocked".Translate(node.LabelCap), MessageTypeDefOf.RejectInput);
                else if (unlockable)
                {
                    hediff.UnlockNode(node);
                    wantRebuild = true;
                }
                else
                {
                    if (hediff.skillPoints < node.skillPointCost)
                        Messages.Message("DW_Career_NotEnoughSP".Translate(node.LabelCap,
                            node.skillPointCost, hediff.skillPoints), MessageTypeDefOf.RejectInput);
                    else
                        Messages.Message("DW_Career_NoParent".Translate(node.LabelCap), MessageTypeDefOf.RejectInput);
                }
            }

            if (!node.childNodes.NullOrEmpty())
                foreach (var child in node.childNodes)
                    DrawNodes(child, origin, ref wantRebuild);
        }
    }
}
