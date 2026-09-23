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
        public RHAH_ChoiceKind choice = RHAH_ChoiceKind.Visitors;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                RHAH_ChoiceRecord record = FindRecord();
                if (ArchivedOnly || record == null || !record.Open)
                {
                    yield return Option_Close;
                    yield break;
                }

                yield return Action("RHAH_Choice_Join", RHAH_ChoiceAction.Join);
                yield return Action("RHAH_Choice_Hire", RHAH_ChoiceAction.Hire);
                yield return Action("RHAH_Choice_Feed", RHAH_ChoiceAction.Feed);
                yield return Action("RHAH_Choice_Reject", RHAH_ChoiceAction.Reject);
                yield return Action("RHAH_Choice_Ignore", RHAH_ChoiceAction.Ignore);
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref choiceId, "choiceId", 0);
            Scribe_Values.Look(ref choice, "choice", RHAH_ChoiceKind.Visitors);
        }

        DiaOption Action(string key, RHAH_ChoiceAction action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.action = () => Settle(action);
            option.resolveTree = true;
            return option;
        }

        void Settle(RHAH_ChoiceAction action)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            RHAH_Settings settings = RHAH_Mod.Settings;
            bool enabled = settings == null || settings.visitorChoicesEnabled;
            RHAH_ChoiceAction settled = RHAH_ChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                true);
            if (settled == RHAH_ChoiceAction.None)
            {
                Messages.Message("RHAH_Choice_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            RHAH_ChoiceRuntime.Apply(FindRecord());
            if (settled == RHAH_ChoiceAction.Feed)
            {
                Messages.Message("RHAH_Choice_Fed".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            Find.LetterStack.RemoveLetter(this);
        }

        RHAH_ChoiceRecord FindRecord()
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                return null;
            }

            IReadOnlyList<RHAH_ChoiceRecord> records = game.OpenChoices;
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
