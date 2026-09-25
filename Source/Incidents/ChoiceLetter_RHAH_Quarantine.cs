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
        }

        void Choose(SuiyinN007Action action)
        {
            NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
            SuiyinBook book = state?.Book;
            SuiyinN007Case record = FindRecord();
            if (state == null || book == null || record == null || record.Outcome != SuiyinN007Outcome.Pending)
            {
                Messages.Message("RHAH_Quarantine_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            state.PullBook();
            bool chosen = book.ChooseQuarantine(record, action);
            state.PushBook();
            if (!chosen)
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
            option.resolveTree = true;
            option.action = action;
            return option;
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
                if (record != null && record.MapId == mapId && record.Outcome == SuiyinN007Outcome.Pending)
                {
                    return record;
                }
            }

            return null;
        }
    }
}
