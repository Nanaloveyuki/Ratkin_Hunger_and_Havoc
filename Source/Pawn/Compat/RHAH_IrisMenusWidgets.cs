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

        internal static void OccurrenceCurve(Listing_Standard list, string anchor, float markerDays, Color color)
        {
            const int samples = 48;
            Rect plot = Card(list, anchor, 148f);
            Rect graph = new Rect(plot.x, plot.y, plot.width, plot.height);
            Widgets.DrawBoxSolid(graph, new Color(0.08f, 0.08f, 0.08f, 0.55f));
            DrawCurve(graph, color, samples);
            DrawMarker(graph, markerDays, color);
            if (Mouse.IsOver(graph))
            {
                float days = DaysAt(graph, Event.current.mousePosition.x);
                Widgets.DrawLineVertical(CurvePoint(graph, days, 0f, RHAH_IncidentSchedule.MaxDays).x, graph.y, graph.height);
                TooltipHandler.TipRegion(graph, () => "RHAH_Menu_Frequency_CurveTip".Translate(
                    days.ToString("0.#"),
                    RHAH_IncidentSchedule.OccurrenceChance(days).ToString("P2")), anchor.GetHashCode());
            }
        }

        internal static float DaysAt(Rect graph, float mouseX)
        {
            if (graph.width <= 0f)
            {
                return 0f;
            }

            float along = Mathf.Clamp01((mouseX - graph.x) / graph.width);
            return along * RHAH_IncidentSchedule.MaxDays;
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
