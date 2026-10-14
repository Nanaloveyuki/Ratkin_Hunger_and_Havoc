using System.Collections.Generic;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_Request : ChoiceLetter
    {
        public int choiceId;
        public int mapId;
        public RHAH_RequestKind kind;
        public RHAH_IntelSiteKind site;
        public int amount;
        public int expireTick = -1;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || !StillOpen())
                {
                    yield return Option_Close;
                    yield break;
                }

                Map map = ResolveMap();
                RHAH_ChoiceRecord record = FindRecord(Current.Game?.GetComponent<GameComponent_RHAH_Game>());
                if (kind == RHAH_RequestKind.Baby && record != null)
                {
                    List<Verse.Pawn> children = RHAH_ChildExchange.Candidates(record);
                    for (int i = 0; i < children.Count; i++)
                    {
                        Verse.Pawn child = children[i];
                        DiaOption childOption = new DiaOption((record.DisplayId == "I-019" ? "RHAH_ChildExchange_LegacyChoose" : "RHAH_ChildExchange_Choose").Translate(child.LabelShortCap));
                        childOption.action = () => Wrapped(childOption, RHAH_ChoiceAction.Deliver, child, false);
                        childOption.resolveTree = false;
                        if (!RHAH_ChildExchange.CanDeliver(record, child, false)) childOption.Disable("RHAH_ChildExchange_Unavailable".Translate());
                        yield return childOption;
                    }
                    if (children.Count == 0)
                    {
                        DiaOption noChild = new DiaOption("RHAH_ChildExchange_ChooseNone".Translate());
                        noChild.Disable("RHAH_ChildExchange_NoChild".Translate());
                        yield return noChild;
                    }
                }
                bool foodAlternative = kind == RHAH_RequestKind.Baby;
                DiaOption deliver = new DiaOption((foodAlternative ? "RHAH_ChildExchange_Food" :
                    record != null && record.TransactionVersion == RHAH_RequestRules.TransactionVersion ? "RHAH_Transaction_Accept" : "RHAH_Choice_Deliver").Translate());
                bool enough = record != null && (record.TransactionVersion == RHAH_RequestRules.TransactionVersion
                    ? RHAH_FoodTransaction.CanDeliver(record)
                    : Pawn.RHAH_ArmedDemand.IsRequest(record) ? Pawn.RHAH_ArmedDemand.CanPay(record)
                    : foodAlternative ? RHAH_ChildExchange.CanDeliver(record, null, true)
                    : map != null && RHAH_ChoiceRuntime.Stock(map, kind) >= amount);
                if (!enough) deliver.Disable((Pawn.RHAH_ArmedDemand.IsRequest(record) ? "RHAH_ArmedDemand_Unavailable" :
                    foodAlternative ? "RHAH_ChildExchange_Unavailable" :
                    record != null && record.TransactionVersion == RHAH_RequestRules.TransactionVersion ? "RHAH_Transaction_Unavailable" : "RHAH_Choice_Short").Translate(amount));
                else
                {
                    deliver.action = () => Wrapped(deliver, RHAH_ChoiceAction.Deliver, null, foodAlternative);
                    deliver.resolveTree = false;
                }

                DiaOption reject = new DiaOption("RHAH_Choice_Reject".Translate());
                reject.action = () => Wrapped(reject, RHAH_ChoiceAction.Reject);
                reject.resolveTree = false;
                DiaOption ignore = new DiaOption("RHAH_Choice_Ignore".Translate());
                ignore.action = () => Wrapped(ignore, RHAH_ChoiceAction.Ignore);
                ignore.resolveTree = false;

                if (!foodAlternative || record?.DisplayId == "I-013") yield return deliver;
                yield return reject;
                yield return ignore;
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref choiceId, "choiceId", 0);
            Scribe_Values.Look(ref mapId, "mapId", 0);
            Scribe_Values.Look(ref kind, "kind", RHAH_RequestKind.None);
            Scribe_Values.Look(ref site, "site", RHAH_IntelSiteKind.None);
            Scribe_Values.Look(ref amount, "amount", 0);
            Scribe_Values.Look(ref expireTick, "expireTick", -1);
        }

        // 仅当信件真的离开信栈才关闭它自己的窗口
        void Wrapped(DiaOption option, RHAH_ChoiceAction action, Verse.Pawn outgoingChild = null, bool foodAlternative = false)
        {
            bool open = Find.LetterStack != null && Find.LetterStack.LettersListForReading.Contains(this);
            Settle(action, outgoingChild, foodAlternative);
            if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
            {
                option.dialog?.Close();
            }
        }

        void Settle(RHAH_ChoiceAction action, Verse.Pawn outgoingChild, bool foodAlternative)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            RHAH_ChoiceRecord record = FindRecord(game);
            if (record == null || Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)) return;
            bool enabled = RHAH_Mod.Settings == null || RHAH_Mod.Settings.AllowsRequest(record.Choice);
            RHAH_ChoiceAction settled = RHAH_ChoiceRuntime.TrySettleRequest(game, choiceId, action,
                Find.TickManager.TicksGame, enabled, outgoingChild, foodAlternative);
            if (settled == RHAH_ChoiceAction.None)
            {
                Messages.Message((record.TransactionVersion == RHAH_RequestRules.TransactionVersion ? "RHAH_Transaction_Unavailable" :
                    record.Kind == RHAH_RequestKind.Baby ? "RHAH_ChildExchange_Unavailable" : "RHAH_Choice_Stale").Translate(), MessageTypeDefOf.RejectInput);
                return;
            }
            if (settled == RHAH_ChoiceAction.Deliver)
                Messages.Message((record.TransactionVersion == RHAH_RequestRules.TransactionVersion ? "RHAH_Transaction_Completed" :
                    record.Kind == RHAH_RequestKind.Baby ? "RHAH_ChildExchange_Completed" : "RHAH_Choice_Delivered").Translate(), MessageTypeDefOf.PositiveEvent);
            if (!record.Open) Find.LetterStack.RemoveLetter(this);
        }

        bool StillOpen()
        {
            RHAH_ChoiceRecord record = FindRecord(Current.Game?.GetComponent<GameComponent_RHAH_Game>());
            return record != null && record.Open;
        }

        RHAH_ChoiceRecord FindRecord(GameComponent_RHAH_Game game)
        {
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
