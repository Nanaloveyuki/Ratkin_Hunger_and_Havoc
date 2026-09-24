using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Pawn;
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

                List<Verse.Pawn> present = RHAH_ChoiceRuntime.Present(record);
                bool recruitable = HasRecruitable(present);
                bool quarantine = Quarantined(present);
                string displayId = record.DisplayId;
                if (RHAH_RequestRules.ShowsRecruit(displayId, true))
                {
                    yield return Gated("RHAH_Choice_Recruit", RHAH_ChoiceAction.Recruit, recruitable, quarantine, durationDays: StayDays(false));
                }

                if (RHAH_RequestRules.ShowsJoin(displayId, true) || choice == RHAH_ChoiceKind.Abandoned || choice == RHAH_ChoiceKind.Kinship)
                {
                    yield return Gated("RHAH_Choice_Join", RHAH_ChoiceAction.Join, present.Count > 0, quarantine);
                }

                if (RHAH_RequestRules.ShowsHire(displayId, true))
                {
                    yield return Gated("RHAH_Choice_Hire", RHAH_ChoiceAction.Hire, recruitable, quarantine, durationDays: StayDays(true));
                }
                if (RHAH_RequestRules.ShowsFoodGive(choice))
                {
                    yield return Action("RHAH_Choice_Feed", RHAH_ChoiceAction.Feed);
                }

                if (RHAH_RequestRules.ShowsEnslave(true, true))
                {
                    bool ideology = ModsConfig.IdeologyActive;
                    yield return Gated(
                        "RHAH_Choice_Enslave",
                        RHAH_ChoiceAction.Enslave,
                        ideology && recruitable,
                        quarantine,
                        ideology ? "RHAH_Choice_NoRecruit" : "RHAH_Choice_NoIdeology");
                }

                if (RHAH_RequestRules.ShowsCapture(true))
                {
                    yield return Gated("RHAH_Choice_Capture", RHAH_ChoiceAction.Capture, recruitable, quarantine);
                }

                if (RHAH_RequestRules.ShowsAttack(true))
                {
                    yield return Gated("RHAH_Choice_Attack", RHAH_ChoiceAction.Attack, recruitable, false);
                }

                Map map = ResolveMap(present);
                bool cells = map != null && RHAH_VisitorBatch.HasPrisonCell(map);
                if (RHAH_RequestRules.ShowsPrison(displayId, true, true, true))
                {
                    yield return Gated(
                        "RHAH_Choice_Prison",
                        RHAH_ChoiceAction.Prison,
                        present.Count > 0 && cells,
                        quarantine,
                        cells ? "RHAH_Choice_NoRecruit" : "RHAH_Choice_NoPrison");
                }

                yield return Action("RHAH_Choice_Reject", RHAH_ChoiceAction.Reject);
                yield return Action("RHAH_Choice_Ignore", RHAH_ChoiceAction.Ignore);
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
            Scribe_Values.Look(ref choiceId, "choiceId", 0);
            Scribe_Values.Look(ref choice, "choice", RHAH_ChoiceKind.Visitors);
        }

        DiaOption Gated(string key, RHAH_ChoiceAction action, bool allowed, bool quarantine, string emptyKey = "RHAH_Choice_NoRecruit", int durationDays = 0)
        {
            string label = durationDays > 0 ? key.Translate(RHAH_VisitorRules.StayLabel(durationDays)).ToString() : key.Translate().ToString();
            DiaOption option = new DiaOption(label);
            if (quarantine && action != RHAH_ChoiceAction.Attack && action != RHAH_ChoiceAction.Reject)
            {
                option.Disable("RHAH_Choice_Quarantine".Translate());
                return option;
            }

            if (!allowed)
            {
                option.Disable(emptyKey.Translate());
                return option;
            }

            option.action = () => Settle(action);
            option.resolveTree = true;
            return option;
        }
        static int StayDays(bool hire)
        {
            RHAH_Settings settings = RHAH_Mod.Settings;
            return hire
                ? (settings == null ? RHAH_VisitorRules.DefaultHireDays : settings.hireDays)
                : (settings == null ? RHAH_VisitorRules.DefaultShelterDays : settings.shelterDays);
        }

        DiaOption Action(string key, RHAH_ChoiceAction action)
        {
            return Gated(key, action, true, false);
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

            RHAH_ChoiceRecord record = FindRecord();
            RHAH_ChoiceRuntime.Apply(record);
            if (settled == RHAH_ChoiceAction.Feed)
            {
                ShowFoodHint(settings);
                Messages.Message("RHAH_Choice_FeedWaiting".Translate(), MessageTypeDefOf.NeutralEvent);
            }

            Find.LetterStack.RemoveLetter(this);
        }

        static void ShowFoodHint(RHAH_Settings settings)
        {
            bool dismissed = settings != null && settings.foodGiveHintDismissed;
            if (!RHAH_RequestRules.ShowsFoodHint(dismissed, true))
            {
                return;
            }

            DiaNode node = new DiaNode("RHAH_Choice_FeedHint".Translate());
            DiaOption dismiss = new DiaOption("RHAH_Choice_FeedHintDismiss".Translate());
            dismiss.action = () =>
            {
                if (RHAH_Mod.Settings != null)
                {
                    RHAH_Mod.Settings.foodGiveHintDismissed = true;
                    RHAH_Mod.Settings.Write();
                }
            };
            dismiss.resolveTree = true;
            DiaOption close = new DiaOption("Close".Translate());
            close.resolveTree = true;
            node.options.Add(dismiss);
            node.options.Add(close);
            Find.WindowStack.Add(new Dialog_NodeTree(node, true, false, "RHAH_Choice_Feed".Translate()));
        }

        static bool HasRecruitable(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (RHAH_Api.Allows(pawns[i], RHAH_BehaviorGate.Hire) ||
                    RHAH_Api.Allows(pawns[i], RHAH_BehaviorGate.JoinColony))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Quarantined(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (RHAH_Api.IsVisitor(pawns[i]) &&
                    !RHAH_Api.Allows(pawns[i], RHAH_BehaviorGate.JoinColony) &&
                    !RHAH_Api.Allows(pawns[i], RHAH_BehaviorGate.Hire))
                {
                    return true;
                }
            }

            return false;
        }

        static Map ResolveMap(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].Map != null)
                {
                    return pawns[i].Map;
                }
            }

            return null;
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
