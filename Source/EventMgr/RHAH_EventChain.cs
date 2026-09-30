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
        internal string DisplayId { get; }
        internal int InstanceId { get; }
        internal int SiteId { get; }
        internal int Tick { get; }
        internal int Stage { get; }
        internal int StartedTick { get; }
        internal int DeadlineTick { get; }
        internal string Payload { get; }

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
                    return;
                }
            }

            chains.Add(chain);
        }

        internal static void Clear()
        {
            chains.Clear();
        }

        internal static IRHAH_EventChain Find(string displayId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return null;
            }

            for (int i = 0; i < chains.Count; i++)
            {
                if (chains[i].Owns(displayId))
                {
                    return chains[i];
                }
            }

            return null;
        }

        internal static int Count => chains.Count;

        internal static IRHAH_EventChain At(int index)
        {
            return index < 0 || index >= chains.Count ? null : chains[index];
        }
    }
}
