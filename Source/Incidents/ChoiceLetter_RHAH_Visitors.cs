using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
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
                bool quarantine = Quarantined(present);
                string displayId = record.DisplayId;
                if (RHAH_RequestRules.ShowsRecruit(displayId, true))
                {
                    yield return Gated("RHAH_Choice_Recruit", RHAH_ChoiceAction.Recruit, HasAllowed(present, RHAH_BehaviorGate.JoinColony), quarantine, durationDays: StayDays(false));
                }

                if (RHAH_RequestRules.ShowsJoin(displayId, true) || choice == RHAH_ChoiceKind.Abandoned || choice == RHAH_ChoiceKind.Kinship)
                {
                    yield return Gated("RHAH_Choice_Join", RHAH_ChoiceAction.Join, HasAllowed(present, RHAH_BehaviorGate.JoinColony), quarantine);
                }

                if (RHAH_RequestRules.ShowsHire(displayId, true))
                {
                    yield return Gated("RHAH_Choice_Hire", RHAH_ChoiceAction.Hire, HasAllowed(present, RHAH_BehaviorGate.Hire), quarantine, durationDays: StayDays(true));
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
                        ideology && RHAH_VisitorBatch.HasEligible(present, record.MapId, true),
                        false,
                        ideology ? "RHAH_Choice_NoCustodyTarget" : "RHAH_Choice_NoIdeology");
                }

                if (RHAH_RequestRules.ShowsCapture(true))
                {
                    yield return Gated("RHAH_Choice_Capture", RHAH_ChoiceAction.Capture,
                        RHAH_VisitorBatch.HasEligible(present, record.MapId, false), false, "RHAH_Choice_NoCustodyTarget");
                }

                if (RHAH_RequestRules.ShowsAttack(true))
                {
                    yield return Gated("RHAH_Choice_Attack", RHAH_ChoiceAction.Attack, present.Count > 0, false, "RHAH_Choice_NoPresent");
                }


                if (RHAH_RequestRules.ShowsAlly(present.Count > 0, RHAH_ChoiceRuntime.HasAllyDestination()))
                {
                    yield return AllyOption(present, quarantine);
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

        DiaOption AllyOption(List<Verse.Pawn> present, bool quarantine)
        {
            RimWorld.Faction faction = RHAH_ChoiceRuntime.AllyDestination();
            string name = faction == null ? string.Empty : faction.Name;
            string label = "RHAH_Choice_Ally".Translate(name).ToString();
            DiaOption option = new DiaOption(label);
            if (quarantine)
            {
                option.Disable("RHAH_Choice_Quarantine".Translate());
                return option;
            }

            if (faction == null)
            {
                option.Disable("RHAH_Choice_NoAlly".Translate());
                return option;
            }

            if (!HasAllowed(present, RHAH_BehaviorGate.Transfer))
            {
                option.Disable("RHAH_Choice_NoTransfer".Translate());
                return option;
            }

            Bind(option, RHAH_ChoiceAction.Ally);
            return option;
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

            Bind(option, action);
            return option;
        }

        void Bind(DiaOption option, RHAH_ChoiceAction action)
        {
            option.resolveTree = false;
            option.action = () =>
            {
                if (Settle(action))
                {
                    option.dialog?.Close();
                }
            };
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

        bool Settle(RHAH_ChoiceAction action)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            RHAH_Settings settings = RHAH_Mod.Settings;
            bool enabled = settings == null || settings.visitorChoicesEnabled;
            string unavailable = UnavailableReason(action);
            if (unavailable != null)
            {
                Messages.Message(unavailable.Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            RHAH_ChoiceAction settled = RHAH_ChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                true);
            if (settled == RHAH_ChoiceAction.None)
            {
                RHAH_ChoiceRecord current = FindRecord();
                string key = current != null && current.Open && enabled &&
                    (RHAH_RequestRules.Captures(action) || RHAH_RequestRules.Enslaves(action))
                    ? "RHAH_Choice_CustodyFailed" : "RHAH_Choice_Stale";
                Messages.Message(key.Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            RHAH_ChoiceRecord record = FindRecord();
            RHAH_ChoiceRuntime.Apply(record);
            if (settled == RHAH_ChoiceAction.Feed)
            {
                ShowFoodHint(settings);
                Messages.Message("RHAH_Choice_FeedWaiting".Translate(), MessageTypeDefOf.NeutralEvent);
            }
            else if (settled == RHAH_ChoiceAction.Ally)
            {
                RimWorld.Faction faction = record == null ? null : RHAH_ChoiceRuntime.FindFaction(record.AllyFactionId);
                int count = record == null ? 0 : record.AllyPawnIds.Count;
                string name = faction == null ? string.Empty : faction.Name;
                Messages.Message("RHAH_Choice_AllySent".Translate(count, name), MessageTypeDefOf.NeutralEvent);
            }

            Find.LetterStack.RemoveLetter(this);
            return true;
        }

        string UnavailableReason(RHAH_ChoiceAction action)
        {
            RHAH_ChoiceRecord record = FindRecord();
            if (record == null || !record.Open || Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this))
            {
                return "RHAH_Choice_Stale";
            }

            List<Verse.Pawn> present = RHAH_ChoiceRuntime.Present(record);
            if (RHAH_RequestRules.Captures(action) || RHAH_RequestRules.Enslaves(action))
            {
                bool enslave = RHAH_RequestRules.Enslaves(action);
                if (enslave && !ModsConfig.IdeologyActive)
                {
                    return "RHAH_Choice_NoIdeology";
                }

                return RHAH_VisitorBatch.HasEligible(present, record.MapId, enslave)
                    ? null : "RHAH_Choice_NoCustodyTarget";
            }

            if (action == RHAH_ChoiceAction.Join || action == RHAH_ChoiceAction.Hire ||
                action == RHAH_ChoiceAction.Recruit || action == RHAH_ChoiceAction.Ally)
            {
                if (Quarantined(present))
                {
                    return "RHAH_Choice_Quarantine";
                }

                RHAH_BehaviorGate gate = action == RHAH_ChoiceAction.Join || action == RHAH_ChoiceAction.Recruit
                    ? RHAH_BehaviorGate.JoinColony
                    : action == RHAH_ChoiceAction.Ally ? RHAH_BehaviorGate.Transfer : RHAH_BehaviorGate.Hire;
                if (!HasAllowed(present, gate))
                {
                    return action == RHAH_ChoiceAction.Ally ? "RHAH_Choice_NoTransfer" : "RHAH_Choice_NoRecruit";
                }

                if (action == RHAH_ChoiceAction.Ally && !RHAH_ChoiceRuntime.HasAllyDestination())
                {
                    return "RHAH_Choice_NoAlly";
                }
            }

            return action == RHAH_ChoiceAction.Attack && present.Count == 0 ? "RHAH_Choice_NoPresent" : null;
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

        static bool HasAllowed(List<Verse.Pawn> pawns, RHAH_BehaviorGate gate)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (RHAH_Api.Allows(pawns[i], gate) && (gate != RHAH_BehaviorGate.Transfer || RHAH_Api.Allows(pawns[i], RHAH_BehaviorGate.ExitMap)))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Quarantined(List<Verse.Pawn> pawns)
        {
            if (RHAH_Mod.Settings != null && !RHAH_Mod.Settings.plagueQuarantineBlocksJoin)
            {
                return false;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                if (RHAH_PlagueRuntime.IsQuarantined(pawns[i]))
                {
                    return true;
                }
            }

            return false;
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
