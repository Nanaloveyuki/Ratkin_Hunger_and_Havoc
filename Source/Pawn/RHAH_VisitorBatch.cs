using Verse.AI.Group;
using System.Collections.Generic;
using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_VisitorBatch
    {
        internal static bool HasPrisonCell(Map map)
        {
            return NextCell(map, null).IsValid;
        }

        internal static void Enslave(List<Verse.Pawn> pawns)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (!Ready(pawn) || !RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Imprison))
                {
                    continue;
                }

                ClearLord(pawn);
                pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
                RHAH_Api.ReleaseToColony(pawn, RHAH_ReleaseReason.Enslaved);
            }
        }

        internal static void Capture(List<Verse.Pawn> pawns, bool moveToCell)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (!Ready(pawn) || !RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Imprison))
                {
                    continue;
                }

                IntVec3 cell = moveToCell ? NextCell(pawn.Map, pawn) : IntVec3.Invalid;
                if (moveToCell && !cell.IsValid)
                {
                    continue;
                }

                ClearLord(pawn);
                if (pawn.Faction == Faction.OfPlayer)
                {
                    pawn.SetFaction(null);
                }

                pawn.guest.CapturedBy(Faction.OfPlayer);
                if (cell.IsValid && pawn.Spawned)
                {
                    pawn.DeSpawn();
                    GenSpawn.Spawn(pawn, cell, pawn.MapHeld ?? pawn.Map);
                }
            }
        }

        static bool Ready(Verse.Pawn pawn)
        {
            return pawn != null && !pawn.Dead && pawn.Spawned && pawn.guest != null && pawn.Map != null;
        }

        static void ClearLord(Verse.Pawn pawn)
        {
            pawn.GetLord()?.RemovePawn(pawn);
            pawn.jobs?.StopAll();
            if (pawn.mindState != null)
            {
                pawn.mindState.duty = null;
            }
        }

        static IntVec3 NextCell(Map map, Verse.Pawn ignore)
        {
            if (map == null || map.areaManager == null)
            {
                return IntVec3.Invalid;
            }

            Area area = map.areaManager.Home;
            foreach (IntVec3 cell in map.AllCells)
            {
                Room room = cell.GetRoom(map);
                if (room == null || !room.IsPrisonCell)
                {
                    continue;
                }

                if (!cell.Standable(map) || cell.GetEdifice(map) is Building_Bed)
                {
                    continue;
                }

                if (area != null && !area[cell])
                {
                    continue;
                }

                if (Occupied(map, cell, ignore))
                {
                    continue;
                }

                return cell;
            }

            return IntVec3.Invalid;
        }

        static bool Occupied(Map map, IntVec3 cell, Verse.Pawn ignore)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                Verse.Pawn pawn = things[i] as Verse.Pawn;
                if (pawn != null && pawn != ignore)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
