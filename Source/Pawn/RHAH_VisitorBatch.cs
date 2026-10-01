using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_VisitorBatch
    {
        internal static bool HasEligible(List<Verse.Pawn> pawns, int mapId, bool enslave)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (CanConvert(pawns[i], mapId, enslave))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool CanConvert(Verse.Pawn pawn, int mapId, bool enslave)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed || !pawn.Spawned ||
                pawn.guest == null || pawn.Map == null || pawn.Map.uniqueID != mapId ||
                !RHAH_Api.IsVisitor(pawn) || pawn.Faction == Faction.OfPlayer ||
                pawn.IsPrisoner || pawn.IsSlave || pawn.InMentalState ||
                RHAH_PlagueRuntime.IsQuarantined(pawn) ||
                !RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Imprison))
            {
                return false;
            }

            return enslave ? ModsConfig.IdeologyActive : IsSecurePrison(pawn.GetRoom());
        }

        internal static bool IsSecurePrison(Room room)
        {
            if (room == null || !room.IsPrisonCell || room.TouchesMapEdge)
            {
                return false;
            }

            List<Region> regions = room.Regions;
            if (regions.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < regions.Count; i++)
            {
                Region region = regions[i];
                for (int j = 0; j < region.links.Count; j++)
                {
                    Region neighbor = region.links[j].GetOtherRegion(region);
                    if (neighbor != null && neighbor.valid && neighbor.door != null && neighbor.door.FreePassage)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static int Enslave(List<Verse.Pawn> pawns, int mapId)
        {
            int converted = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (!CanConvert(pawn, mapId, true) ||
                    !RHAH_Api.ReleaseToColony(pawn, RHAH_ReleaseReason.Enslaved))
                {
                    continue;
                }

                if (pawn.Faction != null && pawn.Faction.Hidden)
                {
                    pawn.SetFactionDirect(null);
                }

                bool everEnslaved = pawn.guest.EverEnslaved;
                pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
                if (!pawn.IsSlaveOfColony)
                {
                    continue;
                }

                pawn.apparel?.UnlockAll();
                // 选择信没有实际执行者 不伪造殖民者的个人戒律记录
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(
                    HistoryEventDefOf.EnslavedPrisoner, pawn.Named(HistoryEventArgsNames.Victim)));
                if (!everEnslaved)
                {
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(
                        HistoryEventDefOf.EnslavedPrisonerNotPreviouslyEnslaved, pawn.Named(HistoryEventArgsNames.Victim)));
                }

                converted++;
            }

            return converted;
        }

        internal static int Capture(List<Verse.Pawn> pawns, int mapId)
        {
            int converted = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (!CanConvert(pawn, mapId, false))
                {
                    continue;
                }

                pawn.guest.CapturedBy(Faction.OfPlayer);
                if (pawn.IsPrisonerOfColony)
                {
                    converted++;
                }
            }

            return converted;
        }
    }
}
