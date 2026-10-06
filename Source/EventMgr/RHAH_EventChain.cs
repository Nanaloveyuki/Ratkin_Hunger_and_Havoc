using System.Collections.Generic;

namespace HungerAndHavoc.EventMgr
{
    internal enum RHAH_EventChainEnd
    {
        None = 0,
        Completed = 1,
        Failed = 2,
        Expired = 3,
        Cancelled = 4
    }

    internal sealed class RHAH_EventChainSite
    {
        internal int MapId { get; }
        internal int CaravanId { get; }
        internal int Tile { get; }

        internal RHAH_EventChainSite(int mapId, int caravanId, int tile)
        {
            MapId = mapId;
            CaravanId = caravanId;
            Tile = tile;
        }

        internal bool IsWorld => MapId == 0 && CaravanId == 0;

        internal bool Same(RHAH_EventChainSite other)
        {
            if (other == null)
            {
                return false;
            }

            if (IsWorld || other.IsWorld)
            {
                return IsWorld && other.IsWorld && Tile == other.Tile;
            }

            return MapId == other.MapId && CaravanId == other.CaravanId;
        }
    }

    internal sealed class RHAH_EventChainContext
    {
        internal string DisplayId { get; private set; }
        internal int InstanceId { get; private set; }
        internal int SiteId { get; private set; }
        internal int Tick { get; private set; }
        internal int Stage { get; private set; }
        internal int StartedTick { get; private set; }
        internal int DeadlineTick { get; private set; }
        internal string Payload { get; private set; }

        internal RHAH_EventChainContext(
            string displayId,
            int instanceId,
            int siteId,
            int tick,
            int stage,
            int startedTick,
            int deadlineTick,
            string payload)
        {
            Fill(displayId, instanceId, siteId, tick, stage, startedTick, deadlineTick, payload);
        }

        internal void Fill(
            string displayId,
            int instanceId,
            int siteId,
            int tick,
            int stage,
            int startedTick,
            int deadlineTick,
            string payload)
        {
            DisplayId = displayId;
            InstanceId = instanceId;
            SiteId = siteId;
            Tick = tick;
            Stage = stage;
            StartedTick = startedTick;
            DeadlineTick = deadlineTick;
            Payload = payload;
        }
    }

    internal interface IRHAH_EventChain
    {
        string DisplayId { get; }

        bool CanStart(RHAH_EventChainSite site, int tick);

        bool OnStart(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context);

        bool OnTick(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context);

        void OnEnd(RHAH_EventChainContext context, RHAH_EventChainEnd end);

        bool Owns(string displayId);
    }

    internal static class RHAH_EventChains
    {
        static readonly List<IRHAH_EventChain> chains = new List<IRHAH_EventChain>();
        static readonly Dictionary<string, IRHAH_EventChain> byDisplayId = new Dictionary<string, IRHAH_EventChain>();
        static readonly string[] IncidentIds = BuildIncidentIds();

        static string[] BuildIncidentIds()
        {
            string[] ids = new string[51];
            for (int id = 1; id <= ids.Length; id++)
            {
                ids[id - 1] = "I-" + id.ToString("000");
            }

            return ids;
        }

        internal static void Register(IRHAH_EventChain chain)
        {
            if (chain == null || string.IsNullOrEmpty(chain.DisplayId))
            {
                return;
            }

            for (int i = 0; i < chains.Count; i++)
            {
                if (chains[i].DisplayId == chain.DisplayId)
                {
                    chains[i] = chain;
                    Rebuild();
                    return;
                }
            }

            chains.Add(chain);
            Rebuild();
        }

        internal static void Clear()
        {
            chains.Clear();
            byDisplayId.Clear();
        }

        internal static IRHAH_EventChain Find(string displayId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return null;
            }

            byDisplayId.TryGetValue(displayId, out IRHAH_EventChain chain);
            return chain;
        }

        static void Rebuild()
        {
            byDisplayId.Clear();
            for (int i = 0; i < chains.Count; i++)
            {
                IRHAH_EventChain chain = chains[i];
                if (chain == null || string.IsNullOrEmpty(chain.DisplayId))
                {
                    continue;
                }

                byDisplayId[chain.DisplayId] = chain;
                if (chain.Owns("I-001") && chain.Owns("I-051"))
                {
                    for (int id = 0; id < IncidentIds.Length; id++)
                    {
                        byDisplayId[IncidentIds[id]] = chain;
                    }
                }
            }
        }

        internal static int Count => chains.Count;

        internal static IRHAH_EventChain At(int index)
        {
            return index < 0 || index >= chains.Count ? null : chains[index];
        }
    }
}
