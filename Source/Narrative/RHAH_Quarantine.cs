using System.Collections.Generic;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Narrative
{
    internal static class RHAH_Quarantine
    {
        internal const string LetterDefName = "RHAH_QuarantineLetter";

        internal static void Tick(NarrativeState state, int tick)
        {
            SuiyinBook book = state?.Book;
            if (book?.N007 == null)
            {
                return;
            }

            state.PullBook();
            for (int i = 0; i < book.N007.Count; i++)
            {
                SuiyinN007Case record = book.N007[i];
                if (record == null)
                {
                    continue;
                }

                if (record.ChoiceOpen && record.Outcome == SuiyinN007Outcome.Pending)
                {
                    record.ChoiceOpen = false;
                    book.QueueOpened(record.MapId);
                }

                Watch(record, tick);
            }

            state.PushBook();
        }

        internal static bool OpenLetter(int mapId)
        {
            if (Find.LetterStack == null || LetterOpen(mapId))
            {
                return Find.LetterStack != null && LetterOpen(mapId);
            }

            LetterDef def = DefDatabase<LetterDef>.GetNamedSilentFail(LetterDefName);
            if (def == null)
            {
                return false;
            }

            ChoiceLetter_RHAH_Quarantine letter = (ChoiceLetter_RHAH_Quarantine)LetterMaker.MakeLetter(def);
            letter.mapId = mapId;
            letter.Label = "RHAH_Suiyin_N007Choice_Label".Translate();
            letter.Text = "RHAH_Suiyin_N007Choice_Text".Translate();
            Map map = MapOf(mapId);
            if (map != null)
            {
                letter.lookTargets = new LookTargets(map.Center, map);
            }

            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }

        static void Watch(SuiyinN007Case record, int tick)
        {
            if (record.Visitors == null || record.Outcome == SuiyinN007Outcome.Pending || record.Outcome == SuiyinN007Outcome.Defer)
            {
                return;
            }

            int here = 0;
            int sickLeft = 0;
            int recovered = 0;
            int dead = 0;
            int unknown = 0;
            int returnId = 0;
            for (int i = 0; i < record.Visitors.Count; i++)
            {
                SuiyinMember member = record.Visitors[i];
                if (member == null || member.LoadId <= 0)
                {
                    continue;
                }

                Verse.Pawn pawn = RHAH_PawnIndex.Find(member.LoadId, tick);
                if (pawn == null || pawn.Destroyed)
                {
                    member.Presence = SuiyinPresence.Unknown;
                    unknown++;
                    continue;
                }

                if (pawn.Dead)
                {
                    member.Presence = SuiyinPresence.Dead;
                    member.Care = SuiyinCare.Free;
                    dead++;
                    continue;
                }

                bool plague = RHAH_Plague.HasActive(pawn);
                bool onMap = pawn.Spawned && pawn.Map != null && pawn.Map.uniqueID == record.MapId;
                member.Care = plague ? SuiyinCare.Plague : SuiyinCare.Free;
                if (!onMap)
                {
                    member.Presence = SuiyinPresence.Left;
                    if (plague)
                    {
                        sickLeft++;
                    }
                    else if (returnId == 0)
                    {
                        recovered++;
                        returnId = member.LoadId;
                    }
                    else
                    {
                        recovered++;
                    }

                    continue;
                }

                member.Presence = SuiyinPresence.Here;
                here++;
            }

            if (record.Visitors.Count == 0 || here + unknown > 0)
            {
                return;
            }

            if (sickLeft > 0 && record.Outcome == SuiyinN007Outcome.Quarantine)
            {
                record.ReturnPawnId = 0;
                Current.Game?.GetComponent<NarrativeState>()?.Commit(item => item.CloseQuarantine(record, SuiyinN007Outcome.Broken, tick, 0));
                return;
            }

            if (recovered > 0 && dead + recovered == record.Visitors.Count)
            {
                Current.Game?.GetComponent<NarrativeState>()?.Commit(item => item.CloseQuarantine(record, SuiyinN007Outcome.RecoveredLeft, tick, returnId));
            }
        }

        static bool LetterOpen(int mapId)
        {
            List<Letter> letters = Find.LetterStack.LettersListForReading;
            for (int i = 0; i < letters.Count; i++)
            {
                ChoiceLetter_RHAH_Quarantine letter = letters[i] as ChoiceLetter_RHAH_Quarantine;
                if (letter != null && letter.mapId == mapId)
                {
                    return true;
                }
            }

            return false;
        }

        static Map MapOf(int mapId)
        {
            if (Find.Maps == null)
            {
                return null;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                if (Find.Maps[i] != null && Find.Maps[i].uniqueID == mapId)
                {
                    return Find.Maps[i];
                }
            }

            return null;
        }
    }
}
