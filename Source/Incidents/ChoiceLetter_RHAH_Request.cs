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
                    enough = false;
                    deliver.Disable("RHAH_Choice_NoBaby".Translate());
                }
                else if (!enough)
                {
                    deliver.Disable("RHAH_Choice_Short".Translate(amount));
                }
                else
                {
                    deliver.action = () => Settle(RHAH_ChoiceAction.Deliver);
                    deliver.resolveTree = true;
                }

                DiaOption reject = new DiaOption("RHAH_Choice_Reject".Translate());
                reject.action = () => Settle(RHAH_ChoiceAction.Reject);
                reject.resolveTree = true;
                DiaOption ignore = new DiaOption("RHAH_Choice_Ignore".Translate());
                ignore.action = () => Settle(RHAH_ChoiceAction.Ignore);
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
            Scribe_Values.Look(ref kind, "kind", RHAH_RequestKind.None);
            Scribe_Values.Look(ref site, "site", RHAH_IntelSiteKind.None);
            Scribe_Values.Look(ref amount, "amount", 0);
            Scribe_Values.Look(ref expireTick, "expireTick", -1);
        }

        void Settle(RHAH_ChoiceAction action)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            RHAH_Settings settings = RHAH_Mod.Settings;
            Map map = ResolveMap();
            bool enabled = settings == null || settings.AllowsRequest(site == RHAH_IntelSiteKind.None ? RHAH_ChoiceKind.Aid : RHAH_ChoiceKind.Intel);
            bool canDeliver = map != null && RHAH_ChoiceRuntime.TryConsume(map, kind, amount);
            if (action != RHAH_ChoiceAction.Deliver)
            {
                canDeliver = true;
            }

            RHAH_ChoiceAction settled = RHAH_ChoiceRuntime.TrySettle(
                game,
                choiceId,
                action,
                Find.TickManager.TicksGame,
                enabled,
                canDeliver);
            if (settled == RHAH_ChoiceAction.None)
            {
                Messages.Message("RHAH_Choice_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            RHAH_ChoiceRecord record = FindRecord(game);
            if (RHAH_RequestRules.CreatesSite(settled, site))
            {
                RHAH_ChoiceRuntime.TryCreateSite(map, site);
            }

            RHAH_ChoiceRuntime.Apply(record);
            if (settled == RHAH_ChoiceAction.Deliver)
            {
                Messages.Message("RHAH_Choice_Delivered".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            Find.LetterStack.RemoveLetter(this);
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
