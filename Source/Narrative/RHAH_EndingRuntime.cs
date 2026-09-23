using System.Collections.Generic;
using HarmonyLib;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Narrative
{
    internal static class RHAH_EndingRuntime
    {
        internal const string NarratorDefName = "RHAH_Suiyin";
        internal const int CheckInterval = 1500;

        internal static bool IsNarrator(string defName)
        {
            return defName == NarratorDefName;
        }

        internal static bool CountsNow()
        {
            return Counts(Find.Storyteller?.def?.defName == NarratorDefName, RHAH_Mod.Settings);
        }

        internal static bool Counts(bool narrator, RHAH_Settings settings)
        {
            return narrator || settings == null || settings.countEndingsWithoutNarrator;
        }

        internal static void Tick(int tick)
        {
            if (tick < 0 || tick % CheckInterval != 0 || Current.Game == null)
            {
                return;
            }

            NarrativeState state = Current.Game.GetComponent<NarrativeState>();
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (state == null)
            {
                return;
            }

            bool narrator = Find.Storyteller?.def?.defName == NarratorDefName;
            if (state.AdultCheckDue(tick))
            {
                state.SetAdultCount(CountAdults(), tick);
            }

            if (!Counts(narrator, settings))
            {
                return;
            }


            RHAH_EndingGoals goals = settings == null ? RHAH_EndingGoals.Defaults() : settings.EndingGoals();
            RHAH_EndingId ending = state.PendingEnding(tick, narrator, goals);
            if (ending != RHAH_EndingId.None)
            {
                state.MarkEnding(ending);
                Show(ending, narrator, state);
            }

            if (state.IdentityDue(tick, narrator, goals))
            {
                RHAH_IdentityTier offer = RHAH_EndingRules.IdentityOffer(state.Snapshot().Trust);
                state.MarkIdentity(offer, false);
                ShowIdentity(offer);
            }
        }

        internal static int CountAdults()
        {
            if (Find.Maps == null)
            {
                return 0;
            }

            int best = 0;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map == null || !map.IsPlayerHome || map.mapPawns == null)
                {
                    continue;
                }

                int count = 0;
                List<Verse.Pawn> colonists = map.mapPawns.FreeColonists;
                for (int pawnIndex = 0; pawnIndex < colonists.Count; pawnIndex++)
                {
                    Verse.Pawn pawn = colonists[pawnIndex];
                    if (pawn != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult && RHAH_Api.IsRatkin(pawn) && RHAH_Api.IsOrigin(pawn))
                    {
                        count++;
                    }
                }

                if (count > best)
                {
                    best = count;
                }
            }

            return best;
        }

        internal static string TextKey(RHAH_EndingId id, bool narrator)
        {
            if (!narrator && (id == RHAH_EndingId.E01 || id == RHAH_EndingId.E02))
            {
                return "RHAH_Ending_Public";
            }

            if (id == RHAH_EndingId.E01)
            {
                return "RHAH_Ending_E01";
            }

            if (id == RHAH_EndingId.E02)
            {
                return "RHAH_Ending_E02";
            }

            if (id == RHAH_EndingId.E03)
            {
                return "RHAH_Ending_E03";
            }

            if (id == RHAH_EndingId.E04)
            {
                return "RHAH_Ending_E04";
            }

            if (id == RHAH_EndingId.E05)
            {
                return "RHAH_Ending_E05";
            }

            return null;
        }

        static void Show(RHAH_EndingId id, bool narrator, NarrativeState state)
        {
            string key = TextKey(id, narrator);
            if (key == null || Current.Game == null)
            {
                return;
            }

            string label = (key + "_Label").Translate();
            string body = (key + "_Text").Translate(state.AidCount, state.BroadcastCount, state.AdultCount);
            GameVictoryUtility.ShowCredits(label + "\n\n" + body, null, false, 5f);
            Find.LetterStack.ReceiveLetter(label, body, LetterDefOf.PositiveEvent);
        }

        static void ShowIdentity(RHAH_IdentityTier tier)
        {
            string key = tier == RHAH_IdentityTier.Full ? "RHAH_Ending_IdentityFull" : "RHAH_Ending_IdentityPartial";
            Find.LetterStack.ReceiveLetter((key + "_Label").Translate(), (key + "_Text").Translate(), LetterDefOf.NeutralEvent);
        }

        internal static string Preview(RHAH_EndingId id, bool narrator, int aid, int broadcasts, int adults)
        {
            string key = id == RHAH_EndingId.None ? "RHAH_Ending_IdentityAsk" : TextKey(id, narrator);
            if (key == null)
            {
                return string.Empty;
            }

            return (key + "_Label").Translate() + "\n\n" + (key + "_Text").Translate(aid, broadcasts, adults);
        }
    }

    // 只改穗音自己的家园 RandomMain 不改其它叙事者、任务袭击、商队和本模组事件池
    [HarmonyPatch(typeof(StorytellerComp_RandomMain), "ChooseRandomCategory")]
    internal static class RHAH_ThreatTempoPatch
    {
        internal static bool Prefix(
            StorytellerComp_RandomMain __instance,
            IIncidentTarget target,
            List<IncidentCategoryDef> skipCategories,
            ref IncidentCategoryDef __result)
        {
            if (Find.Storyteller?.def?.defName != RHAH_EndingRuntime.NarratorDefName || !(target is Map map) || !map.IsPlayerHome)
            {
                return true;
            }

            NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
            int trust = state == null ? 0 : state.Snapshot().Trust;
            if (trust == 0 || __instance?.props is not StorytellerCompProperties_RandomMain props)
            {
                return true;
            }

            float factor = RHAH_EndingRules.ThreatFactor(trust);
            if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig) &&
                Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick > 60000f * props.maxThreatBigIntervalDays / factor)
            {
                __result = IncidentCategoryDefOf.ThreatBig;
            }
            else
            {
                __result = Weighted(props, skipCategories, factor);
            }

            return __result != null ? false : true;
        }

        static IncidentCategoryDef Weighted(StorytellerCompProperties_RandomMain props, List<IncidentCategoryDef> skip, float factor)
        {
            float total = 0f;
            List<IncidentCategoryEntry> weights = props.categoryWeights;
            for (int i = 0; i < weights.Count; i++)
            {
                IncidentCategoryEntry entry = weights[i];
                if (entry?.category == null || skip.Contains(entry.category))
                {
                    continue;
                }

                total += entry.category == IncidentCategoryDefOf.ThreatBig ? entry.weight * factor : entry.weight;
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = Rand.Value * total;
            for (int i = 0; i < weights.Count; i++)
            {
                IncidentCategoryEntry entry = weights[i];
                if (entry?.category == null || skip.Contains(entry.category))
                {
                    continue;
                }

                roll -= entry.category == IncidentCategoryDefOf.ThreatBig ? entry.weight * factor : entry.weight;
                if (roll <= 0f)
                {
                    return entry.category;
                }
            }

            return null;
        }
    }
}
