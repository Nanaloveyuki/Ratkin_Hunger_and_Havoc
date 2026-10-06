using System.Collections.Generic;
using HungerAndHavoc.Incidents;

namespace HungerAndHavoc.EventMgr
{
    internal static class RHAH_EventChainRuntime
    {
        internal static int SiteId(int mapId, int caravanId)
        {
            if (mapId > 0)
            {
                return mapId;
            }

            if (caravanId > 0)
            {
                return -caravanId;
            }

            return 0;
        }

        internal static int SiteId(RHAH_IncidentTarget kind, int savedTargetId)
        {
            int raw = RHAH_IncidentSchedule.DecodeTargetId(savedTargetId, kind);
            if (raw == RHAH_IncidentSchedule.UnspecifiedTargetId)
            {
                return 0;
            }

            return kind == RHAH_IncidentTarget.Caravan ? -raw : raw;
        }

        internal static RHAH_EventChainSite Site(int siteId, int tile)
        {
            if (siteId > 0)
            {
                return new RHAH_EventChainSite(siteId, 0, tile);
            }

            if (siteId < 0)
            {
                return new RHAH_EventChainSite(0, -siteId, tile);
            }

            return new RHAH_EventChainSite(0, 0, tile);
        }

        internal static int Start(
            List<RHAH_EventChainRecord> records,
            ref int nextId,
            string displayId,
            int siteId,
            int tick,
            int deadlineTick,
            string payload)
        {
            if (records == null || string.IsNullOrEmpty(displayId) || nextId <= 0)
            {
                return 0;
            }

            if (deadlineTick < -1)
            {
                deadlineTick = -1;
            }

            RHAH_EventChainRecord record = new RHAH_EventChainRecord
            {
                id = nextId,
                displayId = displayId,
                siteId = siteId,
                stage = 0,
                startedTick = tick,
                deadlineTick = deadlineTick,
                payload = payload ?? "",
                closed = false,
                end = (int)RHAH_EventChainEnd.None
            };
            records.Add(record);
            nextId++;
            return record.id;
        }

        internal static bool SetStage(List<RHAH_EventChainRecord> records, int instanceId, int stage, int deadlineTick, string payload)
        {
            RHAH_EventChainRecord record = Open(records, instanceId);
            if (record == null || stage < 0)
            {
                return false;
            }

            record.stage = stage;
            if (deadlineTick >= -1)
            {
                record.deadlineTick = deadlineTick;
            }

            if (payload != null)
            {
                record.payload = payload;
            }

            return true;
        }

        internal static bool End(List<RHAH_EventChainRecord> records, int instanceId, int tick, RHAH_EventChainEnd end)
        {
            RHAH_EventChainRecord record = Open(records, instanceId);
            if (record == null || end == RHAH_EventChainEnd.None)
            {
                return false;
            }

            record.closed = true;
            record.end = (int)end;
            Notify(record, tick);
            return true;
        }

        internal static int TickDue(List<RHAH_EventChainRecord> records, int tick)
        {
            if (records == null)
            {
                return 0;
            }

            int closed = 0;
            RHAH_EventChainContext context = tickContext;
            for (int i = 0; i < records.Count; i++)
            {
                RHAH_EventChainRecord record = records[i];
                if (record == null || record.closed)
                {
                    continue;
                }

                IRHAH_EventChain chain = RHAH_EventChains.Find(record.displayId);
                if (chain != null)
                {
                    Fill(context, record, tick);
                    chain.OnTick(records, context);
                }

                if (record.deadlineTick >= 0 && tick >= record.deadlineTick)
                {
                    record.closed = true;
                    record.end = (int)RHAH_EventChainEnd.Expired;
                    Notify(record, tick);
                    closed++;
                }
            }

            return closed;
        }

        internal static int Check(List<RHAH_EventChainRecord> records, ref int nextId, RHAH_EventChainSite site, int tick)
        {
            if (records == null || site == null || nextId <= 0)
            {
                return 0;
            }

            int started = 0;
            int count = RHAH_EventChains.Count;
            for (int i = 0; i < count; i++)
            {
                IRHAH_EventChain chain = RHAH_EventChains.At(i);
                if (chain == null || !chain.CanStart(site, tick))
                {
                    continue;
                }

                int id = Start(records, ref nextId, chain.DisplayId, SiteId(site.MapId, site.CaravanId), tick, -1, "");
                RHAH_EventChainRecord record = Open(records, id);
                if (record == null)
                {
                    continue;
                }

                if (!chain.OnStart(records, Context(record, tick)))
                {
                    record.closed = true;
                    record.end = (int)RHAH_EventChainEnd.Cancelled;
                    continue;
                }
                started++;
            }


            return started;
        }

        internal static int ActiveCount(List<RHAH_EventChainRecord> records, string displayId, int siteId)
        {
            if (records == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < records.Count; i++)
            {
                RHAH_EventChainRecord record = records[i];
                if (record == null || record.closed)
                {
                    continue;
                }

                if ((displayId == null || record.displayId == displayId) && (siteId == int.MinValue || record.siteId == siteId))
                {
                    count++;
                }
            }

            return count;
        }

        internal static void Repair(List<RHAH_EventChainRecord> records, ref int nextId)
        {
            if (records == null)
            {
                nextId = nextId < 1 ? 1 : nextId;
                return;
            }

            int highest = 0;
            for (int i = records.Count - 1; i >= 0; i--)
            {
                RHAH_EventChainRecord record = records[i];
                if (record == null || record.id <= 0 || string.IsNullOrEmpty(record.displayId))
                {
                    records.RemoveAt(i);
                    continue;
                }

                record.Repair();
                if (record.id > highest)
                {
                    highest = record.id;
                }
            }

            if (nextId <= highest)
            {
                nextId = highest + 1;
            }
        }

        static RHAH_EventChainRecord Open(List<RHAH_EventChainRecord> records, int instanceId)
        {
            if (records == null || instanceId <= 0)
            {
                return null;
            }

            for (int i = 0; i < records.Count; i++)
            {
                RHAH_EventChainRecord record = records[i];
                if (record != null && record.id == instanceId && !record.closed)
                {
                    return record;
                }
            }

            return null;
        }

        static readonly RHAH_EventChainContext tickContext = new RHAH_EventChainContext(null, 0, 0, 0, 0, 0, 0, null);

        static void Fill(RHAH_EventChainContext context, RHAH_EventChainRecord record, int tick)
        {
            context.Fill(
                record.displayId,
                record.id,
                record.siteId,
                tick,
                record.stage,
                record.startedTick,
                record.deadlineTick,
                record.payload);
        }

        static RHAH_EventChainContext Context(RHAH_EventChainRecord record, int tick)
        {
            return new RHAH_EventChainContext(
                record.displayId,
                record.id,
                record.siteId,
                tick,
                record.stage,
                record.startedTick,
                record.deadlineTick,
                record.payload);
        }

        static void Notify(RHAH_EventChainRecord record, int tick)
        {
            IRHAH_EventChain chain = RHAH_EventChains.Find(record.displayId);
            if (chain == null)
            {
                return;
            }

            chain.OnEnd(Context(record, tick), (RHAH_EventChainEnd)record.end);
        }
    }
}
