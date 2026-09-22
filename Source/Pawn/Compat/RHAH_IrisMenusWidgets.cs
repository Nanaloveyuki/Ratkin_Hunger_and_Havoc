extern alias iris;
using System;
using iris::IrisMenus;
using UnityEngine;
using Verse;

namespace HungerAndHavoc.Pawn.Compat
{
    internal static class RHAH_IrisMenusWidgets
    {
        internal const float CardPad = 4f;
        internal const float CardGap = 4f;

        internal static Rect Card(Listing_Standard list, string anchor, float height)
        {
            MenuControls.Anchor(list, anchor, height + CardGap);
            Rect card = list.GetRect(height);
            Widgets.DrawBox(card);
            list.Gap(CardGap);
            return card.ContractedBy(CardPad);
        }

        internal static float ShareBar(Listing_Standard list, string label, float share, float labelFraction, out Rect row)
        {
            row = list.GetRect(34f);
            float labelWidth = row.width * labelFraction;
            Widgets.Label(new Rect(row.x, row.y, labelWidth, row.height), label);
            Rect bar = new Rect(row.x + labelWidth + 8f, row.y + 8f, Mathf.Max(1f, row.width - labelWidth - 8f), 16f);
            Widgets.FillableBar(bar, Mathf.Clamp01(share));
            list.Gap(CardGap);
            return DragValue(bar);
        }

        internal static void ShareChart(Listing_Standard list, string anchor, string[] labels, float[] shares, int count)
        {
            if (labels == null || shares == null || count <= 0 || count > labels.Length || count > shares.Length)
            {
                return;
            }

            float height = count * 22f + 8f;
            Rect plot = Card(list, anchor, height);
            float max = 0f;
            for (int i = 0; i < count; i++)
            {
                if (shares[i] > max)
                {
                    max = shares[i];
                }
            }

            if (max <= 0f)
            {
                max = 1f;
            }

            float rowHeight = (plot.height - 4f) / count;
            float labelWidth = Mathf.Min(96f, plot.width * 0.34f);
            for (int i = 0; i < count; i++)
            {
                float y = plot.y + i * rowHeight;
                Widgets.Label(new Rect(plot.x, y, labelWidth, rowHeight), labels[i]);
                Rect bar = new Rect(plot.x + labelWidth + 6f, y + 3f, Mathf.Max(1f, plot.width - labelWidth - 6f), Mathf.Max(8f, rowHeight - 6f));
                Widgets.FillableBar(bar, Mathf.Clamp01(shares[i] / max));
            }
        }

        static float DragValue(Rect bar)
        {
            if (!Mouse.IsOver(bar) || Event.current.type != EventType.MouseDrag || bar.width <= 0f)
            {
                return -1f;
            }

            return Mathf.Clamp01((Event.current.mousePosition.x - bar.x) / bar.width);
        }
    }
}
