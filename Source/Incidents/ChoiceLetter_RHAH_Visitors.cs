using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_Visitors : ChoiceLetter
    {
        public int choiceId;
        public HungerChoiceKind choice = HungerChoiceKind.Visitors;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                HungerChoiceRecord record = FindRecord();
                if (ArchivedOnly || record == null || !record.Open)
                {
                    yield return Option_Close;
                    yield break;
                }

                yield return Action("RHAH_Choice_Join", HungerChoiceAction.Join);
                yield return Action("RHAH_Choice_Hire", HungerChoiceAction.Hire);
                yield return Action("RHAH_Choice_Feed", HungerChoiceAction.Feed);
                yield return Action("RHAH_Choice_Reject", HungerChoiceAction.Reject);
                yield return Action("RHAH_Choice_Ignore", HungerChoiceAction.Ignore);
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref choiceId, "choiceId", 0);
            Scribe_Values.Look(ref choice, "choice", HungerChoiceKind.Visitors);
        }

        DiaOption Action(string key, HungerChoiceAction action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.action = () => Settle(action);
            option.resolveTree = true;
            return option;
        }

        void Settle(HungerChoiceAction action)
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            bool enabled = settings == null || settings.visitorChoicesEnabled;
            HungerChoiceAction settled = HungerChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                true);
            if (settled == HungerChoiceAction.None)
            {
                Messages.Message("RHAH_Choice_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            HungerChoiceRuntime.Apply(FindRecord());
            if (settled == HungerChoiceAction.Feed)
            {
                Messages.Message("RHAH_Choice_Fed".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            Find.LetterStack.RemoveLetter(this);
        }

        HungerChoiceRecord FindRecord()
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (game == null)
            {
                return null;
            }

            IReadOnlyList<HungerChoiceRecord> records = game.OpenChoices;
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Id == choiceId)
                {
                    return records[i];
                }
            }

            return null;
        }
    }
}
