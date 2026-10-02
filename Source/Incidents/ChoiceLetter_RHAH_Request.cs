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

                DiaOption deliver = new DiaOption("RHAH_Choice_Deliver".Translate());
                Map map = ResolveMap();
                bool enough = map != null && RHAH_ChoiceRuntime.Stock(map, kind) >= amount;
                if (kind == RHAH_RequestKind.Baby)
                {
                    RHAH_ChoiceRecord record = FindRecord(Current.Game?.GetComponent<GameComponent_RHAH_Game>());
                    bool food = record != null &&
                        RHAH_RequestRules.CanSubstituteFood(
                            RHAH_Mod.Settings == null || RHAH_Mod.Settings.childExchangeFoodSubstitution,
                            record.Choice,
                            map == null ? 0 : RHAH_ChoiceRuntime.Stock(map, RHAH_RequestKind.SimpleMeal),
                            record.PawnLoadIds.Count);
                    if (food)
                    {
                        enough = true;
                        deliver.action = () => Wrapped(deliver, RHAH_ChoiceAction.Deliver);
                        deliver.resolveTree = false;
                    }
                    else
                    {
                        enough = false;
                        deliver.Disable("RHAH_Choice_NoBaby".Translate());
                    }
                }
                else if (!enough)
                {
                    deliver.Disable("RHAH_Choice_Short".Translate(amount));
                }
                else
                {
                    deliver.action = () => Wrapped(deliver, RHAH_ChoiceAction.Deliver);
                    deliver.resolveTree = false;
                }

                DiaOption reject = new DiaOption("RHAH_Choice_Reject".Translate());
                reject.action = () => Wrapped(reject, RHAH_ChoiceAction.Reject);
                reject.resolveTree = false;
                DiaOption ignore = new DiaOption("RHAH_Choice_Ignore".Translate());
                ignore.action = () => Wrapped(ignore, RHAH_ChoiceAction.Ignore);
                ignore.resolveTree = false;

                yield return deliver;
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
        void Wrapped(DiaOption option, RHAH_ChoiceAction action)
        {
            bool open = Find.LetterStack != null && Find.LetterStack.LettersListForReading.Contains(this);
            Settle(action);
            if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
            {
                option.dialog?.Close();
            }
        }

        void Settle(RHAH_ChoiceAction action)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            RHAH_Settings settings = RHAH_Mod.Settings;
            Map map = ResolveMap();
            RHAH_ChoiceRecord open = FindRecord(game);
            bool enabled = settings == null || settings.AllowsRequest(open == null ? (site == RHAH_IntelSiteKind.None ? RHAH_ChoiceKind.Aid : RHAH_ChoiceKind.Intel) : open.Choice);
            bool foodForChild = open != null && open.Choice == RHAH_ChoiceKind.ChildExchange && kind == RHAH_RequestKind.Baby &&
                (settings == null || settings.childExchangeFoodSubstitution);
            int foodAmount = foodForChild ? RHAH_RequestRules.FoodForChildren(open.PawnLoadIds.Count) : 0;
            bool canDeliver = action == RHAH_ChoiceAction.Deliver && map != null && (foodForChild
                ? RHAH_ChoiceRuntime.Stock(map, RHAH_RequestKind.SimpleMeal) >= foodAmount && foodAmount > 0
                : RHAH_RequestRules.CanDeliver(kind, RHAH_ChoiceRuntime.Stock(map, kind), amount, false));

            RHAH_ChoiceAction settled = RHAH_ChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                canDeliver);
            if (settled == RHAH_ChoiceAction.None)
            {
                TaggedString reason = action == RHAH_ChoiceAction.Deliver && open != null && open.Open && enabled && map != null && !canDeliver
                    ? kind == RHAH_RequestKind.Baby ? "RHAH_Choice_NoBaby".Translate()
                        : "RHAH_Choice_Short".Translate(amount)
                    : "RHAH_Choice_Stale".Translate();
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                return;
            }

            RHAH_ChoiceRecord record = FindRecord(game);
            RHAH_RequestKind consumed = foodForChild ? RHAH_RequestKind.SimpleMeal : kind;
            int consumedAmount = foodForChild ? foodAmount : amount;
            if (settled == RHAH_ChoiceAction.Deliver &&
                RHAH_RequestRules.RemovesStock(settled, consumed) &&
                !RHAH_ChoiceRuntime.TryConsume(map, consumed, consumedAmount))
            {
                if (record != null && record.Settled == RHAH_ChoiceAction.Deliver)
                {
                    record.Settled = RHAH_ChoiceAction.None;
                }

                Messages.Message("RHAH_Choice_Short".Translate(consumedAmount), MessageTypeDefOf.RejectInput);
                return;
            }

            if (RHAH_RequestRules.CreatesSite(settled, site))
            {
                RHAH_ChoiceRuntime.TryCreateSite(map, site);
            }

            RHAH_ChoiceRuntime.Apply(record);
            if (settled == RHAH_ChoiceAction.Deliver)
            {
                Messages.Message("RHAH_Choice_Delivered".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            if (Find.LetterStack != null)
            {
                Find.LetterStack.RemoveLetter(this);
            }
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
