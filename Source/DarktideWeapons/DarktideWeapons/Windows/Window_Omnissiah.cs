using DarktideWeapons.Blessings;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Windows
{
    public class Window_Omnissiah : Window
    {
        private enum Tab { Perks, Blessings }

        private readonly Pawn pawn;
        private readonly Thing weapon;
        private readonly Comp_BlessingSocket socket;

        private Tab currentTab = Tab.Blessings;
        private DW_BlessingDef selectedBlessing;
        private DW_EquipmentPerkDef selectedPerk;
        private Vector2 scrollPos = Vector2.zero;

        private const float LeftPanelWidth = 210f;
        private const float PanelGap = 10f;
        private const float ItemHeight = 62f;
        private const float ItemSpacing = 4f;
        private const float ConfirmHeight = 36f;
        private const float TabBtnHeight = 32f;

        public override Vector2 InitialSize => new Vector2(720f, 520f);

        public Window_Omnissiah(Pawn pawn, Thing weapon, Comp_BlessingSocket socket)
        {
            this.pawn = pawn;
            this.weapon = weapon;
            this.socket = socket;
            this.forcePause = true;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 34f),
                "DW_OmniTitle".Translate(weapon.LabelShortCap));
            Text.Font = GameFont.Small;
            inRect.yMin += 38f;

            Rect leftRect  = new Rect(inRect.x, inRect.y, LeftPanelWidth, inRect.height);
            float divX     = inRect.x + LeftPanelWidth + 4f;
            Rect rightRect = new Rect(divX + 6f, inRect.y, inRect.xMax - divX - 6f, inRect.height);

            Widgets.DrawLineVertical(divX, inRect.y, inRect.height);
            DoLeftPanel(leftRect);
            DoRightPanel(rightRect);
        }

        private void DoLeftPanel(Rect rect)
        {
            float curY = rect.y;

            Rect perksBtn = new Rect(rect.x, curY, rect.width, TabBtnHeight);
            if (currentTab == Tab.Perks)
                Widgets.DrawHighlight(perksBtn);
            if (Widgets.ButtonText(perksBtn, "DW_TabPerks".Translate()))
                SwitchTab(Tab.Perks);
            curY += TabBtnHeight + 4f;

            Rect blessBtn = new Rect(rect.x, curY, rect.width, TabBtnHeight);
            if (currentTab == Tab.Blessings)
                Widgets.DrawHighlight(blessBtn);
            if (Widgets.ButtonText(blessBtn, "DW_TabBlessings".Translate()))
                SwitchTab(Tab.Blessings);
            curY += TabBtnHeight + 8f;

            Widgets.DrawLineHorizontal(rect.x, curY, rect.width);
            curY += 8f;

            Text.Font = GameFont.Tiny;
            if (currentTab == Tab.Perks)
            {
                Widgets.Label(new Rect(rect.x, curY, rect.width, 20f),
                    "DW_InstalledPerks".Translate() + $" ({socket.InstalledPerks.Count}/{socket.MaxPerkSlots})");
                curY += 22f;
                foreach (var perkDef in socket.InstalledPerks)
                {
                    Widgets.Label(new Rect(rect.x + 4f, curY, rect.width - 4f, 20f), "• " + perkDef.LabelCap);
                    curY += 20f;
                }
            }
            else
            {
                Widgets.Label(new Rect(rect.x, curY, rect.width, 20f),
                    "DW_InstalledBlessings".Translate() + $" ({socket.InstalledBlessings.Count}/{socket.MaxBlessingSlots})");
                curY += 22f;
                foreach (var blessDef in socket.InstalledBlessings)
                {
                    string label = blessDef.LabelCap;
                    float hitOverride = socket.GetHitSeverityOverride(blessDef);
                    float killOverride = socket.GetKillSeverityOverride(blessDef);
                    if (hitOverride >= 0f)
                        label += $" (x{hitOverride})";
                    if (killOverride >= 0f)
                        label += $" (kx{killOverride})";
                    Widgets.Label(new Rect(rect.x + 4f, curY, rect.width - 4f, 20f), "• " + label);
                    curY += 20f;
                }
            }
            Text.Font = GameFont.Small;
        }

        private void DoRightPanel(Rect rect)
        {
            bool hasSelection = (currentTab == Tab.Perks    && selectedPerk     != null)
                             || (currentTab == Tab.Blessings && selectedBlessing != null);

            bool canConfirm = (currentTab == Tab.Perks    && selectedPerk     != null && socket.CanAddPerk)
                           || (currentTab == Tab.Blessings && selectedBlessing != null && socket.CanAddBlessing);

            Rect listRect = hasSelection
                ? new Rect(rect.x, rect.y, rect.width, rect.height - ConfirmHeight - 8f)
                : rect;

            IReadOnlyList<DW_BlessingDef>      availableBlessings = socket.AvailableBlessings;
            IReadOnlyList<DW_EquipmentPerkDef> availablePerks     = socket.AvailablePerks;
            int count = currentTab == Tab.Perks ? availablePerks.Count : availableBlessings.Count;
            float viewHeight = Mathf.Max(count * (ItemHeight + ItemSpacing), listRect.height);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, viewHeight);

            Widgets.BeginScrollView(listRect, ref scrollPos, viewRect);

            float curY = 0f;
            if (currentTab == Tab.Perks)
            {
                if (availablePerks.Count == 0)
                    Widgets.Label(new Rect(4f, curY, viewRect.width - 4f, 30f), "DW_NoPerksAvailable".Translate());
                else
                    foreach (var perkDef in availablePerks)
                        DrawPerkItem(perkDef, ref curY, viewRect.width);
            }
            else
            {
                if (availableBlessings.Count == 0)
                    Widgets.Label(new Rect(4f, curY, viewRect.width - 4f, 30f), "DW_NoBlessingsAvailable".Translate());
                else
                    foreach (var blessDef in availableBlessings)
                        DrawBlessingItem(blessDef, ref curY, viewRect.width);
            }

            Widgets.EndScrollView();

            if (hasSelection)
            {
                string confirmLabel;
                if (canConfirm)
                    confirmLabel = "DW_Confirm".Translate();
                else if (currentTab == Tab.Perks)
                    confirmLabel = "DW_PerkSlotsFull".Translate();
                else
                    confirmLabel = "DW_BlessingSlotsFull".Translate();

                Rect confirmRect = new Rect(rect.x, rect.yMax - ConfirmHeight, rect.width, ConfirmHeight);
                if (Widgets.ButtonText(confirmRect, confirmLabel) && canConfirm)
                    ApplySelection();
            }
        }

        private void DrawPerkItem(DW_EquipmentPerkDef perkDef, ref float curY, float width)
        {
            bool installed = socket.InstalledPerks.Contains(perkDef);
            bool selected  = selectedPerk == perkDef;

            Rect itemRect = new Rect(0f, curY, width, ItemHeight);
            DrawItemBackground(itemRect, installed, selected);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(6f, curY + 4f, width - 12f, 22f), perkDef.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(6f, curY + 26f, width - 12f, 32f), perkDef.description);
            Text.Font = GameFont.Small;

            DrawInstalledBadge(itemRect, installed);

            if (!installed && Widgets.ButtonInvisible(itemRect))
                selectedPerk = selected ? null : perkDef;

            TooltipHandler.TipRegion(itemRect, perkDef.LabelCap + "\n" + perkDef.description);
            curY += ItemHeight + ItemSpacing;
        }

        private void DrawBlessingItem(DW_BlessingDef blessDef, ref float curY, float width)
        {
            bool installed = socket.InstalledBlessings.Contains(blessDef);
            bool selected  = selectedBlessing == blessDef;

            Rect itemRect = new Rect(0f, curY, width, ItemHeight);
            DrawItemBackground(itemRect, installed, selected);

            // Build display name with override info
            string displayName = blessDef.LabelCap;
            float hitOverride = socket.GetHitSeverityOverride(blessDef);
            float killOverride = socket.GetKillSeverityOverride(blessDef);
            if (hitOverride >= 0f)
                displayName += $" ({hitOverride})";
            if (killOverride >= 0f)
                displayName += " (" + "DW_KillShort".Translate() + ": " + killOverride + ")";

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(6f, curY + 4f, width - 12f, 22f), displayName);

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(6f, curY + 26f, width - 12f, 32f), blessDef.description);
            Text.Font = GameFont.Small;

            DrawInstalledBadge(itemRect, installed);

            if (!installed && Widgets.ButtonInvisible(itemRect))
                selectedBlessing = selected ? null : blessDef;

            // Build tooltip with severity details
            string tooltip = blessDef.LabelCap + "\n" + blessDef.description;
            if (hitOverride >= 0f)
                tooltip += "\n" + "DW_HitSeverity".Translate() + ": " + hitOverride;
            else
                tooltip += "\n" + "DW_HitSeverity".Translate() + ": " + blessDef.hediffHitSeverity;
            if (killOverride >= 0f)
                tooltip += "\n" + "DW_KillSeverity".Translate() + ": " + killOverride;
            else if (blessDef.killSelfHediff != null)
                tooltip += "\n" + "DW_KillSeverity".Translate() + ": " + blessDef.hediffKillSeverity;
            TooltipHandler.TipRegion(itemRect, tooltip);
            curY += ItemHeight + ItemSpacing;
        }

        private static void DrawItemBackground(Rect rect, bool installed, bool selected)
        {
            if (installed)
                Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.45f, 0.2f, 0.3f));
            else if (selected)
                Widgets.DrawHighlight(rect);
            Widgets.DrawHighlightIfMouseover(rect);
        }

        private static void DrawInstalledBadge(Rect itemRect, bool installed)
        {
            if (!installed) return;
            Text.Anchor = TextAnchor.MiddleRight;
            Text.Font   = GameFont.Tiny;
            Widgets.Label(new Rect(0f, itemRect.y + 4f, itemRect.width - 6f, 22f),
                "DW_Installed".Translate());
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void ApplySelection()
        {
            if (currentTab == Tab.Perks && selectedPerk != null)
            {
                socket.AddPerk(selectedPerk);
                selectedPerk = null;
            }
            else if (currentTab == Tab.Blessings && selectedBlessing != null)
            {
                socket.AddBlessing(selectedBlessing);
                selectedBlessing = null;
            }
        }

        private void SwitchTab(Tab tab)
        {
            currentTab       = tab;
            selectedBlessing = null;
            selectedPerk     = null;
            scrollPos        = Vector2.zero;
        }
    }
}
