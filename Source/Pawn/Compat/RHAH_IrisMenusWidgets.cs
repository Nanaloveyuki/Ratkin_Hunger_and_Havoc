extern alias iris;
using System;
using HungerAndHavoc.Incidents;
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

        internal static float TunedValue(
            Listing_Standard list,
            string label,
            float value,
            ref string buffer,
            float min,
            float max,
            string format,
            string tooltip)
        {
            float height = 30f;
            Rect row = list.GetRect(height);
            float result = TunedValue(row, label, value, ref buffer, min, max, format, tooltip);
            list.Gap(CardGap);
            return result;
        }

        internal static float TunedValue(
            Rect row,
            string label,
            float value,
            ref string buffer,
            float min,
            float max,
            string format,
            string tooltip)
        {
            Rect labelRect = new Rect(row.x, row.y, row.width * 0.34f, row.height);
            Widgets.Label(labelRect, label);
            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(labelRect, tooltip);
            }

            float fieldWidth = 72f;
            Rect field = new Rect(row.xMax - fieldWidth, row.y, fieldWidth, row.height);
            Rect slider = new Rect(labelRect.xMax + 8f, row.y, Mathf.Max(1f, field.x - labelRect.xMax - 16f), row.height);
            float slid = Widgets.HorizontalSlider(slider, value, min, max);
            bool sliderMoved = Mathf.Abs(slid - value) > 0.001f;
            if (sliderMoved)
            {
                value = slid;
                buffer = value.ToString(format);
            }

            string typed = buffer;
            Widgets.TextFieldNumeric(field, ref value, ref buffer, min, max);
            if (!sliderMoved && SameNumber(typed, buffer))
            {
                buffer = typed;
            }

            return value;
        }

        internal static bool SameNumber(string typed, string parsed)
        {
            float left;
            float right;
            return float.TryParse(typed, out left) && float.TryParse(parsed, out right) && Mathf.Abs(left - right) <= 0.001f;
        }

        internal static void OccurrenceCurve(Listing_Standard list, string anchor, float positiveDays, float negativeDays)
        {
            const int samples = 48;
            Rect plot = Card(list, anchor, 168f);
            Widgets.Label(new Rect(plot.x, plot.y, plot.width * 0.5f, 18f), "RHAH_Menu_Frequency_Positive".Translate());
            Widgets.Label(new Rect(plot.x + plot.width * 0.5f, plot.y, plot.width * 0.5f, 18f), "RHAH_Menu_Frequency_Negative".Translate());
            Rect graph = new Rect(plot.x, plot.y + 22f, plot.width, plot.height - 22f);
            Widgets.DrawBoxSolid(graph, new Color(0.08f, 0.08f, 0.08f, 0.55f));
            DrawCurve(graph, new Color(0.45f, 0.78f, 0.48f), samples);
            DrawMarker(graph, positiveDays, new Color(0.45f, 0.78f, 0.48f));
            DrawMarker(graph, negativeDays, new Color(0.86f, 0.42f, 0.36f));
            TooltipHandler.TipRegion(graph, "RHAH_Menu_Frequency_CurveTip".Translate(
                RHAH_IncidentSchedule.OccurrenceChance(positiveDays).ToString("P1"),
                RHAH_IncidentSchedule.OccurrenceChance(negativeDays).ToString("P1")));
        }

        static void DrawCurve(Rect graph, Color color, int samples)
        {
            float span = RHAH_IncidentSchedule.MaxDays;
            Vector2 last = CurvePoint(graph, 0f, 0f, span);
            for (int i = 1; i <= samples; i++)
            {
                float sampleDays = span * i / samples;
                Vector2 next = CurvePoint(graph, sampleDays, RHAH_IncidentSchedule.OccurrenceChance(sampleDays), span);
                Widgets.DrawLine(last, next, color, 1.5f);
                last = next;
            }
        }

        static void DrawMarker(Rect graph, float days, Color color)
        {
            if (days <= 0f)
            {
                return;
            }

            Vector2 point = CurvePoint(
                graph,
                days,
                RHAH_IncidentSchedule.OccurrenceChance(days),
                RHAH_IncidentSchedule.MaxDays);
            Widgets.DrawBoxSolid(new Rect(point.x - 2f, point.y - 2f, 4f, 4f), color);
        }

        static Vector2 CurvePoint(Rect graph, float days, float chance, float span)
        {
            float x = graph.x + graph.width * Mathf.Clamp01(days / span);
            float y = graph.yMax - graph.height * Mathf.Clamp01(chance / AxisChance);
            return new Vector2(x, y);
        }

        const float AxisChance = 1000f / (1f * 60000f);

    }
}
