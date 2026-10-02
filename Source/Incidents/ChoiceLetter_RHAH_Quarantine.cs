using System.Collections.Generic;
using HungerAndHavoc.Narrative;
using HungerAndHavoc.Storyteller.Suiyin;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_Quarantine : ChoiceLetter
    {
        public int mapId;
        public int startedTick = -1;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                SuiyinN007Case record = FindRecord();
                if (ArchivedOnly || record == null || record.Outcome != SuiyinN007Outcome.Pending)
                {
                    yield return Option_Close;
                    yield break;
                }
                NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
                int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
                if (!RHAH_Quarantine.Refresh(state, record, tick))
                {
                    yield return Option_Close;
                    yield return Option_Postpone;
                    yield break;
                }


                yield return Act("RHAH_Quarantine_Hold", () => Choose(SuiyinN007Action.Quarantine));
                yield return Act("RHAH_Quarantine_Release", () => Choose(SuiyinN007Action.Release));
                yield return Act("RHAH_Quarantine_Defer", () => Choose(SuiyinN007Action.Defer));
                if (lookTargets.IsValid())
                {
                    yield return Option_JumpToLocationAndPostpone;
                }

                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mapId, "mapId", 0);
            Scribe_Values.Look(ref startedTick, "startedTick", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && startedTick == -1)
            {
                MigrateBinding();
            }
        }

        void Choose(SuiyinN007Action action)
        {
            NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
            SuiyinN007Case record = FindRecord();
            int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            if (!RHAH_Quarantine.Choose(state, record, action, tick))
            {
                Messages.Message("RHAH_Quarantine_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }


            if (Find.LetterStack != null)
            {
                Find.LetterStack.RemoveLetter(this);
            }
        }

        DiaOption Act(string key, System.Action action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.resolveTree = false;
            option.action = () => Wrapped(option, action);
            return option;
        }

        // 仅当信件真的离开信栈才关闭它自己的窗口
        void Wrapped(DiaOption option, System.Action action)
        {
            bool open = Find.LetterStack != null && Find.LetterStack.LettersListForReading.Contains(this);
            if (!open)
            {
                Messages.Message("RHAH_Quarantine_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            action();
            if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
            {
                option.dialog?.Close();
            }
        }

        SuiyinN007Case FindRecord()
        {
            SuiyinBook book = Current.Game?.GetComponent<NarrativeState>()?.Book;
            if (book?.N007 == null)
            {
                return null;
            }

            for (int i = 0; i < book.N007.Count; i++)
            {
                SuiyinN007Case record = book.N007[i];
                if (record != null && record.MapId == mapId && record.StartedTick == startedTick)
                {
                    return record;
                }
            }

            return null;
        }

        void MigrateBinding()
        {
            SuiyinBook book = Current.Game?.GetComponent<NarrativeState>()?.Book;
            SuiyinN007Case candidate = null;
            for (int i = 0; book?.N007 != null && i < book.N007.Count; i++)
            {
                SuiyinN007Case record = book.N007[i];
                if (record == null || record.MapId != mapId || record.StartedTick > arrivalTick)
                {
                    continue;
                }

                if (candidate != null)
                {
                    startedTick = -2;
                    return;
                }

                candidate = record;
            }

            startedTick = candidate == null ? -2 : candidate.StartedTick;
        }
    }
}
