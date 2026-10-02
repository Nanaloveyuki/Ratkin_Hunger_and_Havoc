using System.Collections.Generic;
using HungerAndHavoc.Narrative;
using HungerAndHavoc.Storyteller.Suiyin;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_Envoy : ChoiceLetter
    {
        public int mapId;
        public int pawnId;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                SuiyinN008Case record = FindRecord();
                if (ArchivedOnly || record == null || !RHAH_Envoy.OpenCase(record.Outcome))
                {
                    yield return Option_Close;
                    yield break;
                }

                Map map = ResolveMap();
                bool meals = map != null && RHAH_ChoiceRuntime.Stock(map, RHAH_RequestKind.SimpleMeal) >= (Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost);
                DiaOption trade = new DiaOption("RHAH_Envoy_Trade".Translate(Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost));
                if (!meals)
                {
                    trade.Disable("RHAH_Envoy_NoMeals".Translate(Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost));
                }
                else
                {
                    trade.action = () => Wrapped(trade, Trade);
                    trade.resolveTree = false;
                }

                yield return trade;
                yield return Act("RHAH_Envoy_Proof", Proof);
                yield return Act("RHAH_Envoy_Refuse", Refuse);
                DiaOption drive = new DiaOption("RHAH_Envoy_Drive".Translate(Core.RHAH_Mod.Settings == null ? -2 : Core.RHAH_Mod.Settings.trustEnvoyFail));
                drive.action = () => Wrapped(drive, Drive);
                drive.resolveTree = false;
                yield return drive;
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
            Scribe_Values.Look(ref pawnId, "pawnId", 0);
        }

        void Trade()
        {
            Map map = ResolveMap();
            NarrativeState state = State();
            SuiyinN008Case record = FindRecord();
            if (map == null || state == null || record == null || !RHAH_Envoy.OpenCase(record.Outcome))
            {
                return;
            }

            if (RHAH_ChoiceRuntime.Stock(map, RHAH_RequestKind.SimpleMeal) < (Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost))
            {
                Messages.Message("RHAH_Envoy_NoMeals".Translate(Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost), MessageTypeDefOf.RejectInput);
                return;
            }

            if (state.Book.N009 == null && !state.Book.RelicClue)
            {
                record.MealsReady = true;
                if (!state.Commit(book => book.ChooseEnvoy(record, SuiyinN008Action.Trade, Now())))
                {
                    record.MealsReady = false;
                    return;
                }

                RHAH_ChoiceRuntime.TryConsume(map, RHAH_RequestKind.SimpleMeal, (Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost));
                Find.LetterStack.RemoveLetter(this);
                return;
            }

            if (!RHAH_ChoiceRuntime.TryCreateSite(map, RHAH_IntelSiteKind.Treasure))
            {
                return;
            }

            record.MealsReady = true;
            if (!state.Commit(book => book.ChooseEnvoy(record, SuiyinN008Action.Trade, Now())))
            {
                record.MealsReady = false;
                return;
            }

            RHAH_ChoiceRuntime.TryConsume(map, RHAH_RequestKind.SimpleMeal, (Core.RHAH_Mod.Settings == null ? RHAH_Envoy.MealCost : Core.RHAH_Mod.Settings.envoyMealCost));
            Find.LetterStack.RemoveLetter(this);
        }

        void Proof()
        {
            NarrativeState state = State();
            SuiyinN008Case record = FindRecord();
            if (state == null || record == null || record.Outcome != SuiyinN008Outcome.Waiting)
            {
                return;
            }

            record.ProofAvailable = RHAH_Envoy.HasProof(state.Book);
            if (!state.Commit(book => book.ChooseEnvoy(record, SuiyinN008Action.Proof, Now())))
            {
                record.ProofAvailable = false;
                return;
            }

            Find.LetterStack.RemoveLetter(this);
        }

        void Refuse()
        {
            Choose(SuiyinN008Action.Refuse);
        }

        void Drive()
        {
            Choose(SuiyinN008Action.Drive);
        }

        void Choose(SuiyinN008Action action)
        {
            NarrativeState state = State();
            SuiyinN008Case record = FindRecord();
            if (state == null || record == null || !RHAH_Envoy.OpenCase(record.Outcome))
            {
                return;
            }

            if (!state.Commit(book => book.ChooseEnvoy(record, action, Now())))
            {
                return;
            }

            Find.LetterStack.RemoveLetter(this);
        }


        static int Now()
        {
            return Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
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
            SuiyinN008Case record = FindRecord();
            if (State() == null || record == null || !RHAH_Envoy.OpenCase(record.Outcome))
            {
                Messages.Message("RHAH_Envoy_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            action();
            if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
            {
                option.dialog?.Close();
            }
        }

        SuiyinN008Case FindRecord()
        {
            SuiyinBook book = Book();
            if (book?.N008 == null)
            {
                return null;
            }

            for (int i = 0; i < book.N008.Count; i++)
            {
                SuiyinN008Case record = book.N008[i];
                if (record != null && record.MapId == mapId && record.PawnId == pawnId)
                {
                    return record;
                }
            }

            return book.N008.Count == 0 ? null : book.N008[0];
        }

        static NarrativeState State()
        {
            return Current.Game?.GetComponent<NarrativeState>();
        }

        static SuiyinBook Book()
        {
            return State()?.Book;
        }

        Map ResolveMap()
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
