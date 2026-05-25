using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Classes
{
    /// <summary>
    /// Simple scrollable window that lists every DW_ClassDef.
    /// Clicking one selects it and closes this window.
    /// </summary>
    public class Window_ClassSelect : Window
    {
        private readonly Hediff_ClassSystem hediff;
        private Vector2 scrollPos = Vector2.zero;

        private const float RowHeight = 74f;
        private const float IconSize  = 56f;
        private const float Padding   = 10f;

        public override Vector2 InitialSize => new Vector2(480f, 540f);

        public Window_ClassSelect(Hediff_ClassSystem hediff)
        {
            this.hediff        = hediff;
            this.forcePause    = true;
            this.doCloseButton = true;
            this.doCloseX      = true;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Text.Font = GameFont.Medium;
            float titleH = Text.LineHeight + 6f;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, titleH),
                "DW_Class_SelectTitle".Translate());
            Text.Font = GameFont.Small;
            inRect.yMin += titleH + 4f;

            List<DW_ClassDef> all = DefDatabase<DW_ClassDef>.AllDefsListForReading;
            float viewHeight = all.Count * (RowHeight + 4f);

            Rect scrollOuter = inRect;
            Rect scrollView  = new Rect(0f, 0f, scrollOuter.width - 16f, viewHeight);

            Widgets.BeginScrollView(scrollOuter, ref scrollPos, scrollView);
            float y = 0f;
            for (int i = 0; i < all.Count; i++)
            {
                DW_ClassDef classDef = all[i];
                Rect rowRect = new Rect(0f, y, scrollView.width, RowHeight);

                // Highlight on hover
                if (Mouse.IsOver(rowRect))
                    Widgets.DrawHighlight(rowRect);

                // Icon
                Rect iconRect = new Rect(Padding, y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
                GUI.DrawTexture(iconRect, classDef.Icon);

                // Label + description
                float textX = iconRect.xMax + Padding;
                Widgets.Label(new Rect(textX, y + Padding, scrollView.width - textX - Padding, Text.LineHeight),
                    classDef.LabelCap);

                GUI.color = Color.gray;
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(textX, y + Padding + Text.LineHeight + 2f,
                    scrollView.width - textX - Padding, RowHeight - Padding - Text.LineHeight - 2f),
                    classDef.description);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;

                Widgets.DrawBox(rowRect, 1);

                if (Widgets.ButtonInvisible(rowRect))
                {
                    hediff.SelectClass(classDef);
                    Close();
                }

                y += RowHeight + 4f;
            }
            Widgets.EndScrollView();
        }
    }
}
