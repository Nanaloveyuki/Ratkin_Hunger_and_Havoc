using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;
using HungerAndHavoc.Narrative;

namespace HungerAndHavoc.Storyteller.Suiyin
{
    internal static class RHAH_Quarantine
    {
        internal const string LetterDefName = "RHAH_QuarantineLetter";
        internal const int WatchInterval = GenDate.TicksPerHour;

        internal static void Tick(NarrativeState state, int tick)
        {
            SuiyinBook book = state?.Book;
            if (book?.N007 == null)
            {
                return;
            }

            state.PullBook();
            bool watch = RHAH_NarrativePace.Due(tick, WatchInterval, 0);
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

                if (watch && record.Outcome != SuiyinN007Outcome.Defer)
                {
                    Refresh(state, record, tick);
                }
            }

            state.PushBook();
        }

        internal static bool OpenLetter(int mapId)
        {
            SuiyinBook book = Current.Game?.GetComponent<NarrativeState>()?.Book;
            SuiyinN007Case record = null;
            if (book?.N007 != null)
            {
                for (int i = 0; i < book.N007.Count; i++)
                {
                    SuiyinN007Case candidate = book.N007[i];
                    if (candidate != null && candidate.MapId == mapId && candidate.Outcome == SuiyinN007Outcome.Pending)
                    {
                        if (record != null)
                        {
                            return false;
                        }

                        record = candidate;
                    }
                }
            }

            if (record == null)
            {
                return true;
            }

            if (Find.LetterStack == null)
            {
                return false;
            }

            List<Letter> letters = Find.LetterStack.LettersListForReading;
            for (int i = 0; i < letters.Count; i++)
            {
                ChoiceLetter_RHAH_Quarantine existing = letters[i] as ChoiceLetter_RHAH_Quarantine;
                if (existing != null && existing.mapId == mapId && existing.startedTick == record.StartedTick)
                {
                    return true;
                }
            }

            LetterDef def = DefDatabase<LetterDef>.GetNamedSilentFail(LetterDefName);
            if (def == null)
            {
                return false;
            }

            ChoiceLetter_RHAH_Quarantine letter = (ChoiceLetter_RHAH_Quarantine)LetterMaker.MakeLetter(def);
            letter.mapId = mapId;
            letter.startedTick = record.StartedTick;
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

        internal static bool Choose(NarrativeState state, SuiyinN007Case record, SuiyinN007Action action, int tick)
        {
            if (state == null || record == null || record.Outcome != SuiyinN007Outcome.Pending || !Refresh(state, record, tick))
            {
                return false;
            }

            if (!state.Commit(book => book.ChooseQuarantine(record, action)))
            {
                return false;
            }

            ApplyVisitors(record, tick);
            return true;
        }

        internal static bool Refresh(NarrativeState state, SuiyinN007Case record, int tick)
        {
            if (state == null || record == null || record.Outcome == SuiyinN007Outcome.Defer)
            {
                return false;
            }

            if (Closed(record.Outcome))
            {
                ReturnGates(record, tick);
                RemoveLetters(record);
                return false;
            }
            HungerAndHavoc.Core.RHAH_Mod.Settings?.CopyNarrative(state.Book.Config);


            int here = 0;
            int eligible = 0;
            int sickLeft = 0;
            int recoveredLeft = 0;
            int recoveredStayed = 0;
            int dead = 0;
            int unknown = 0;
            int missing = 0;
            int total = 0;
            int returnId = 0;
            bool mapExists = MapOf(record.MapId) != null;
            for (int i = 0; record.Visitors != null && i < record.Visitors.Count; i++)
            {
                SuiyinMember member = record.Visitors[i];
                if (member == null || member.LoadId <= 0)
                {
                    continue;
                }

                total++;
                Verse.Pawn pawn = RHAH_PawnIndex.Find(member.LoadId, tick);
                if (pawn == null || (pawn.Destroyed && !pawn.Dead))
                {
                    if (member.Presence == SuiyinPresence.Dead)
                    {
                        dead++;
                        continue;
                    }

                    if (member.MissingSince < 0)
                    {
                        member.MissingSince = tick;
                    }

                    if (tick - member.MissingSince >= state.Book.Config.MissingDays * 60000)
                    {
                        member.Presence = SuiyinPresence.Missing;
                        missing++;
                    }
                    else
                    {
                        member.Presence = SuiyinPresence.Unknown;
                        unknown++;
                    }

                    continue;
                }

                member.MissingSince = -1;
                if (pawn.Dead)
                {
                    member.Presence = SuiyinPresence.Dead;
                    member.Care = SuiyinCare.Free;
                    dead++;
                    continue;
                }

                bool plague = RHAH_Plague.HasActive(pawn);
                bool onMap = mapExists && pawn.MapHeld != null && pawn.MapHeld.uniqueID == record.MapId;
                member.Care = plague ? SuiyinCare.Plague : SuiyinCare.Free;
                member.Presence = onMap ? SuiyinPresence.Here : SuiyinPresence.Left;
                if (onMap)
                {
                    if (!plague && pawn.Faction == Faction.OfPlayer && !pawn.IsPrisoner && !pawn.IsSlaveOfColony)
                    {
                        recoveredStayed++;
                    }
                    else
                    {
                        here++;
                    }

                    if (Eligible(pawn, record.MapId))
                    {
                        eligible++;
                    }
                }
                else if (plague)
                {
                    sickLeft++;
                }
                else
                {
                    recoveredLeft++;
                    if (returnId == 0)
                    {
                        returnId = member.LoadId;
                    }
                }
            }

            SuiyinN007Outcome? ending = null;
            if (total == 0)
            {
                ending = SuiyinN007Outcome.Missing;
            }
            else if (dead == total)
            {
                ending = SuiyinN007Outcome.AllDead;
            }
            else if (here == 0 && unknown == 0)
            {
                if (record.Outcome == SuiyinN007Outcome.Quarantine && sickLeft > 0)
                {
                    ending = SuiyinN007Outcome.Broken;
                }
                else if (missing > 0 || sickLeft > 0)
                {
                    ending = SuiyinN007Outcome.Missing;
                }
                else if (recoveredStayed > 0)
                {
                    ending = SuiyinN007Outcome.RecoveredStayed;
                }
                else if (recoveredLeft > 0)
                {
                    ending = SuiyinN007Outcome.RecoveredLeft;
                }
            }

            if (ending.HasValue)
            {
                state.Commit(book => book.CloseQuarantine(record, ending.Value, tick, returnId));
                ReturnGates(record, tick);
                RemoveLetters(record);
                return false;
            }

            ApplyVisitors(record, tick);
            return mapExists && eligible > 0;
        }

        static bool Eligible(Verse.Pawn pawn, int mapId)
        {
            return pawn != null && !pawn.Dead && !pawn.Destroyed && pawn.Spawned && pawn.Map?.uniqueID == mapId &&
                RHAH_Api.IsVisitor(pawn) && pawn.Faction != Faction.OfPlayer && !pawn.IsPrisoner && !pawn.IsSlaveOfColony;
        }

        static void ApplyVisitors(SuiyinN007Case record, int tick)
        {
            for (int i = 0; record.Visitors != null && i < record.Visitors.Count; i++)
            {
                SuiyinMember member = record.Visitors[i];
                Verse.Pawn pawn = member == null ? null : RHAH_PawnIndex.Find(member.LoadId, tick);
                if (!Eligible(pawn, record.MapId))
                {
                    RHAH_NarrativePace.ReturnVisitorGates(pawn, RHAH_NarrativePace.QuarantineHoldKey);
                    continue;
                }

                if (record.Outcome == SuiyinN007Outcome.Quarantine)
                {
                    RHAH_Envoy.Hold(pawn, RHAH_NarrativePace.QuarantineHoldKey);
                }
                else if (record.Outcome == SuiyinN007Outcome.Release)
                {
                    RHAH_Envoy.Release(pawn, RHAH_NarrativePace.QuarantineHoldKey);
                }
            }
        }

        static void ReturnGates(SuiyinN007Case record, int tick)
        {
            for (int i = 0; record.Visitors != null && i < record.Visitors.Count; i++)
            {
                SuiyinMember member = record.Visitors[i];
                RHAH_NarrativePace.ReturnVisitorGates(member == null ? null : RHAH_PawnIndex.Find(member.LoadId, tick), RHAH_NarrativePace.QuarantineHoldKey);
            }
        }

        static bool Closed(SuiyinN007Outcome outcome)
        {
            return outcome == SuiyinN007Outcome.RecoveredLeft || outcome == SuiyinN007Outcome.RecoveredStayed ||
                outcome == SuiyinN007Outcome.AllDead || outcome == SuiyinN007Outcome.Missing || outcome == SuiyinN007Outcome.Broken;
        }

        static void RemoveLetters(SuiyinN007Case record)
        {
            List<Letter> letters = Find.LetterStack?.LettersListForReading;
            for (int i = letters == null ? -1 : letters.Count - 1; i >= 0; i--)
            {
                ChoiceLetter_RHAH_Quarantine letter = letters[i] as ChoiceLetter_RHAH_Quarantine;
                if (letter != null && letter.mapId == record.MapId && letter.startedTick == record.StartedTick)
                {
                    Find.LetterStack.RemoveLetter(letter);
                }
            }
        }

        static Map MapOf(int mapId)
        {
            for (int i = 0; Find.Maps != null && i < Find.Maps.Count; i++)
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
