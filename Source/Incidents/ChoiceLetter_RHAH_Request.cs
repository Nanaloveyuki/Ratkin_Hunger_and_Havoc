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
        public HungerRequestKind kind;
        public HungerIntelSiteKind site;
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
                bool enough = map != null && HungerChoiceRuntime.Stock(map, kind) >= amount;
                if (kind == HungerRequestKind.Baby)
                {
                    enough = false;
                    deliver.Disable("RHAH_Choice_NoBaby".Translate());
                }
                else if (!enough)
                {
                    deliver.Disable("RHAH_Choice_Short".Translate(amount));
                }
                else
                {
                    deliver.action = () => Settle(HungerChoiceAction.Deliver);
                    deliver.resolveTree = true;
                }

                DiaOption reject = new DiaOption("RHAH_Choice_Reject".Translate());
                reject.action = () => Settle(HungerChoiceAction.Reject);
                reject.resolveTree = true;
                DiaOption ignore = new DiaOption("RHAH_Choice_Ignore".Translate());
                ignore.action = () => Settle(HungerChoiceAction.Ignore);
                ignore.resolveTree = true;

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
            Scribe_Values.Look(ref kind, "kind", HungerRequestKind.None);
            Scribe_Values.Look(ref site, "site", HungerIntelSiteKind.None);
            Scribe_Values.Look(ref amount, "amount", 0);
            Scribe_Values.Look(ref expireTick, "expireTick", -1);
        }

        void Settle(HungerChoiceAction action)
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            Map map = ResolveMap();
            bool enabled = settings == null || settings.AllowsRequest(site == HungerIntelSiteKind.None ? HungerChoiceKind.Aid : HungerChoiceKind.Intel);
            bool canDeliver = map != null && HungerChoiceRuntime.TryConsume(map, kind, amount);
            if (action != HungerChoiceAction.Deliver)
            {
                canDeliver = true;
            }

            HungerChoiceAction settled = HungerChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                canDeliver);
            if (settled == HungerChoiceAction.None)
            {
                Messages.Message("RHAH_Choice_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            HungerChoiceRecord record = FindRecord(game);
            if (HungerRequestRules.CreatesSite(settled, site))
            {
                HungerChoiceRuntime.TryCreateSite(map, site);
            }

            HungerChoiceRuntime.Apply(record);
            if (settled == HungerChoiceAction.Deliver)
            {
                Messages.Message("RHAH_Choice_Delivered".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            Find.LetterStack.RemoveLetter(this);
        }

        bool StillOpen()
        {
            HungerChoiceRecord record = FindRecord(Current.Game?.GetComponent<GameComponent_HungerAndHavoc>());
            return record != null && record.Open;
        }

        HungerChoiceRecord FindRecord(GameComponent_HungerAndHavoc game)
        {
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
