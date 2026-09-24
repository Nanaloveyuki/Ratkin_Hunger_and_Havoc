extern alias iris;
using System;
using HungerAndHavoc.Generation;
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
        internal static void Quote(Listing_Standard list, string anchor, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            float width = Mathf.Max(1f, list.ColumnWidth - 14f);
            float height = Text.CalcHeight(text, width);
            MenuControls.Anchor(list, anchor, height + CardGap);
            Rect row = list.GetRect(height);
            Widgets.DrawBoxSolid(new Rect(row.x, row.y, 3f, row.height), new Color(0.62f, 0.58f, 0.42f));
            Widgets.Label(new Rect(row.x + 10f, row.y, width, height), text);
            list.Gap(CardGap);
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

        internal static void OccurrenceCurve(
            Listing_Standard list,
            string anchor,
            float averageDays,
            float windowDays,
            Color color)
        {
            const int samples = 48;
            Rect plot = Card(list, anchor, 148f);
            Rect graph = new Rect(plot.x, plot.y, plot.width, plot.height);
            Widgets.DrawBoxSolid(graph, new Color(0.08f, 0.08f, 0.08f, 0.55f));
            float span = RHAH_IncidentSchedule.ClampWindowDays(windowDays);
            float chance = RHAH_IncidentSchedule.DailyOccurrenceChance(averageDays);
            DrawCurve(graph, color, samples, span, chance);
            if (Mouse.IsOver(graph))
            {
                float day = DayAt(graph, Event.current.mousePosition.x, span);
                Widgets.DrawLineVertical(CurvePoint(graph, day, 0f, span, chance).x, graph.y, graph.height);
                TooltipHandler.TipRegion(graph, () => "RHAH_Menu_Frequency_CurveTip".Translate(
                    Mathf.Clamp(Mathf.CeilToInt(day), 1, Mathf.CeilToInt(span)).ToString(),
                    chance.ToString("P2")), anchor.GetHashCode());
            }
        }

        internal static float DayAt(Rect graph, float mouseX, float windowDays)
        {
            if (graph.width <= 0f)
            {
                return 0f;
            }

            float span = RHAH_IncidentSchedule.ClampWindowDays(windowDays);
            float along = Mathf.Clamp01((mouseX - graph.x) / graph.width);
            return along * span;
        }

        static void DrawCurve(Rect graph, Color color, int samples, float span, float chance)
        {
            Vector2 last = CurvePoint(graph, 0f, chance, span, chance);
            for (int i = 1; i <= samples; i++)
            {
                float day = span * i / samples;
                Vector2 next = CurvePoint(graph, day, chance, span, chance);
                Widgets.DrawLine(last, next, color, 1.5f);
                last = next;
            }
        }

        static Vector2 CurvePoint(Rect graph, float day, float chance, float span, float axisChance)
        {
            float x = graph.x + graph.width * Mathf.Clamp01(day / span);
            float scale = axisChance <= 0f ? 1f : axisChance;
            float y = graph.yMax - graph.height * Mathf.Clamp01(chance / scale);
            return new Vector2(x, y);
        }
        internal static void LitterCurve(Listing_Standard list, string anchor, int minimum, int peak, int maximum)
        {
            RHAH_FertilityRules.ClampLitter(ref minimum, ref peak, ref maximum);
            Rect plot = Card(list, anchor, 148f);
            Widgets.DrawBoxSolid(plot, new Color(0.08f, 0.08f, 0.08f, 0.55f));
            int span = Mathf.Max(1, maximum - minimum);
            Vector2 last = LitterPoint(plot, minimum, minimum, peak, maximum, span);
            for (int count = minimum + 1; count <= maximum; count++)
            {
                Vector2 next = LitterPoint(plot, count, minimum, peak, maximum, span);
                Widgets.DrawLine(last, next, new Color(0.78f, 0.62f, 0.38f), 1.5f);
                last = next;
            }

            if (!Mouse.IsOver(plot) || plot.width <= 0f)
            {
                return;
            }

            float along = Mathf.Clamp01((Event.current.mousePosition.x - plot.x) / plot.width);
            int countAt = Mathf.Clamp(Mathf.RoundToInt(minimum + along * span), minimum, maximum);
            Widgets.DrawLineVertical(LitterPoint(plot, countAt, minimum, peak, maximum, span).x, plot.y, plot.height);
            float chance = RHAH_FertilityRules.LitterChance(countAt, minimum, peak, maximum);
            TooltipHandler.TipRegion(plot, () => "RHAH_Menu_Genes_LitterTip".Translate(countAt.ToString(), chance.ToString("P0")), anchor.GetHashCode());
        }

        static Vector2 LitterPoint(Rect graph, int count, int minimum, int peak, int maximum, int span)
        {
            float x = graph.x + graph.width * (count - minimum) / span;
            float chance = RHAH_FertilityRules.LitterChance(count, minimum, peak, maximum);
            return new Vector2(x, graph.yMax - graph.height * Mathf.Clamp01(chance));
        }

    }
}
