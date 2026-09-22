using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Pawn;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class HungerChoiceRuntime
    {
        internal static HungerChoiceRecord Open(GameComponent_HungerAndHavoc game, HungerChoiceRecord record)
        {
            if (game == null || record == null || string.IsNullOrEmpty(record.DisplayId))
            {
                return null;
            }

            List<HungerChoiceRecord> records = Records(game);
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Open && records[i].BatchId == record.BatchId && records[i].MapId == record.MapId)
                {
                    return records[i];
                }
            }

            record.Id = NextId(game);
            records.Add(record);
            return record;
        }

        internal static HungerChoiceAction TrySettle(
            GameComponent_HungerAndHavoc game,
            int choiceId,
            HungerChoiceAction requested,
            int tick,
            bool enabled,
            bool canDeliver)
        {
            HungerChoiceRecord record = FindRecord(game, choiceId);
            if (record == null)
            {
                return HungerChoiceAction.None;
            }

            HungerChoiceAction action = requested;
            if (record.ExpireTick >= 0 && tick >= record.ExpireTick && record.Open)
            {
                action = HungerChoiceAction.Timeout;
            }

            HungerChoiceAction settled = HungerRequestRules.Settle(action, enabled, !record.Open, canDeliver);
            if (settled == HungerChoiceAction.None)
            {
                return HungerChoiceAction.None;
            }

            record.Settled = settled;
            return settled;
        }

        internal static void Tick(GameComponent_HungerAndHavoc game, int tick, bool enabled)
        {
            if (game == null)
            {
                return;
            }

            List<HungerChoiceRecord> records = Records(game);
            for (int i = 0; i < records.Count; i++)
            {
                HungerChoiceRecord record = records[i];
                if (!record.Open || record.ExpireTick < 0 || tick < record.ExpireTick)
                {
                    continue;
                }

                TrySettle(game, record.Id, HungerChoiceAction.Timeout, tick, enabled, false);
                if (record.Settled == HungerChoiceAction.Timeout)
                {
                    Leave(record);
                }
            }
        }

        internal static bool TryConsume(Map map, HungerRequestKind kind, int amount)
        {
            if (map == null || amount <= 0)
            {
                return false;
            }

            if (kind == HungerRequestKind.Baby)
            {
                return false;
            }

            if (kind == HungerRequestKind.Medicine)
            {
                return ConsumeMedicine(map, amount);
            }

            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(HungerRequestRules.ThingDefName(kind));
            return def != null && Consume(map.listerThings.ThingsOfDef(def), amount);
        }

        internal static int Stock(Map map, HungerRequestKind kind)
        {
            if (map == null || kind == HungerRequestKind.None || kind == HungerRequestKind.Baby)
            {
                return 0;
            }

            if (kind == HungerRequestKind.Medicine)
            {
                return CountMedicine(map);
            }

            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(HungerRequestRules.ThingDefName(kind));
            if (def == null)
            {
                return 0;
            }

            return Count(map.listerThings.ThingsOfDef(def));
        }

        internal static bool TryCreateSite(Map map, HungerIntelSiteKind kind)
        {
            string partName = HungerRequestRules.SitePartDefName(kind);
            if (map == null || Find.World == null || string.IsNullOrEmpty(partName))
            {
                return false;
            }

            SitePartDef part = DefDatabase<SitePartDef>.GetNamedSilentFail(partName);
            PlanetTile tile;
            if (part == null || !TileFinder.TryFindNewSiteTile(out tile, 5, 22))
            {
                return false;
            }

            Faction faction = kind == HungerIntelSiteKind.Treasure ? null : Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            Site site = SiteMaker.MakeSite(part, tile, faction, true, null, null);
            if (site == null)
            {
                return false;
            }

            Find.WorldObjects.Add(site);
            Find.LetterStack.ReceiveLetter(
                "RHAH_Intel_Label".Translate(),
                "RHAH_Intel_Text".Translate(),
                LetterDefOf.PositiveEvent,
                site);
            return true;
        }

        internal static void Apply(HungerChoiceRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (HungerRequestRules.Joins(record.Settled))
            {
                Release(record, HungerReleaseReason.JoinedPlayerFaction);
                return;
            }

            if (HungerRequestRules.Hires(record.Settled))
            {
                Release(record, HungerReleaseReason.Recruited);
                return;
            }

            if (HungerRequestRules.Leaves(record.Settled))
            {
                Leave(record);
            }
        }

        static void Release(HungerChoiceRecord record, HungerReleaseReason reason)
        {
            List<Verse.Pawn> pawns = Pawns(record);
            for (int i = 0; i < pawns.Count; i++)
            {
                if (HungerAndHavocApi.Allows(pawns[i], reason == HungerReleaseReason.Recruited ? HungerBehaviorGate.Hire : HungerBehaviorGate.JoinColony))
                {
                    HungerAndHavocApi.ReleaseToColony(pawns[i], reason);
                    pawns[i].SetFaction(Faction.OfPlayer);
                }
            }
        }

        static void Leave(HungerChoiceRecord record)
        {
            List<Verse.Pawn> pawns = Pawns(record);
            if (pawns.Count == 0)
            {
                return;
            }

            RHAH_BatchAttitude.TryShift(pawns[0], true);
        }

        static List<Verse.Pawn> Pawns(HungerChoiceRecord record)
        {
            List<Verse.Pawn> result = new List<Verse.Pawn>();
            if (record.PawnLoadIds == null)
            {
                return result;
            }

            for (int i = 0; i < record.PawnLoadIds.Count; i++)
            {
                Verse.Pawn pawn = FindPawn(record.PawnLoadIds[i]);
                if (pawn != null && !pawn.Dead && !pawn.Destroyed)
                {
                    result.Add(pawn);
                }
            }

            return result;
        }

        static Verse.Pawn FindPawn(int loadId)
        {
            if (loadId <= 0)
            {
                return null;
            }

            List<Verse.Pawn> pawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != null && pawns[i].thingIDNumber == loadId)
                {
                    return pawns[i];
                }
            }

            return null;
        }

        static bool ConsumeMedicine(Map map, int amount)
        {
            List<Thing> medicines = new List<Thing>();
            List<Thing> things = map.listerThings.AllThings;
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing != null && thing.Spawned && thing.def != null && thing.def.IsMedicine && thing.stackCount > 0)
                {
                    medicines.Add(thing);
                }
            }

            return Consume(medicines, amount);
        }

        static int CountMedicine(Map map)
        {
            int total = 0;
            List<Thing> things = map.listerThings.AllThings;
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing != null && thing.Spawned && thing.def != null && thing.def.IsMedicine)
                {
                    total += thing.stackCount;
                }
            }

            return total;
        }

        static int Count(List<Thing> things)
        {
            int total = 0;
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] != null && things[i].Spawned)
                {
                    total += things[i].stackCount;
                }
            }

            return total;
        }

        static bool Consume(List<Thing> things, int amount)
        {
            if (Count(things) < amount)
            {
                return false;
            }

            int remaining = amount;
            for (int i = 0; i < things.Count && remaining > 0; i++)
            {
                Thing thing = things[i];
                if (thing == null || !thing.Spawned || thing.stackCount <= 0)
                {
                    continue;
                }

                int take = thing.stackCount < remaining ? thing.stackCount : remaining;
                if (take >= thing.stackCount)
                {
                    remaining -= thing.stackCount;
                    thing.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    thing.SplitOff(take).Destroy(DestroyMode.Vanish);
                    remaining -= take;
                }
            }

            return remaining == 0;
        }

        static HungerChoiceRecord FindRecord(GameComponent_HungerAndHavoc game, int choiceId)
        {
            List<HungerChoiceRecord> records = Records(game);
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].Id == choiceId)
                {
                    return records[i];
                }
            }

            return null;
        }

        static List<HungerChoiceRecord> Records(GameComponent_HungerAndHavoc game)
        {
            return (List<HungerChoiceRecord>)game.OpenChoices;
        }

        static int NextId(GameComponent_HungerAndHavoc game)
        {
            int id = game.NextChoiceId;
            game.NextChoiceId = id + 1;
            return id;
        }
    }
}
