using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_BatchAttitude
    {
        internal static bool TryShift(Verse.Pawn harmed, bool forcedAway)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(harmed);
            if (comp == null || !comp.State.IsActiveVisitor || harmed.Map == null)
            {
                return false;
            }

            RHAH_Attitude attitude = comp.State.attitude;
            RHAH_AttitudeShift shift = RHAH_AttitudePolicy.React(attitude, forcedAway);
            if (shift == RHAH_AttitudeShift.None)
            {
                return false;
            }

            int batchId = comp.State.spawnBatchId;
            List<Verse.Pawn> members = Collect(harmed.Map, batchId);
            if (members.Count == 0)
            {
                return false;
            }

            if (shift == RHAH_AttitudeShift.Hostile)
            {
                SetHostile(members);
            }

            OrderLeave(members);
            return true;
        }

        static void SetHostile(List<Verse.Pawn> members)
        {
            Faction hostile = RHAH_AttitudeFactions.Resolve(RHAH_Attitude.Hostile);
            Faction player = Faction.OfPlayer;
            if (hostile != null && player != null && !hostile.HostileTo(player))
            {
                player.TryAffectGoodwillWith(
                    hostile,
                    player.GoodwillToMakeHostile(hostile),
                    false,
                    false,
                    null,
                    null);
            }

            for (int i = 0; i < members.Count; i++)
            {
                CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(members[i]);
                comp?.State.SetAttitude(RHAH_Attitude.Hostile);
            }
        }

        static void OrderLeave(List<Verse.Pawn> members)
        {
            Lord lord = members[0].GetLord();
            if (lord != null)
            {
                for (int i = 1; i < members.Count; i++)
                {
                    if (members[i].GetLord() != lord)
                    {
                        lord = null;
                        break;
                    }
                }
            }

            if (lord != null)
            {
                lord.ReceiveMemo("RHAH_Leave");
                return;
            }

            for (int i = 0; i < members.Count; i++)
            {
                Verse.Pawn pawn = members[i];
                if (pawn.Downed || pawn.Map == null)
                {
                    continue;
                }

                Job leave = JobGiver_RHAH_Leave.TryCreate(pawn);
                if (leave != null)
                {
                    pawn.jobs?.StartJob(leave, JobCondition.InterruptForced);
                }
            }
        }

        static List<Verse.Pawn> Collect(Map map, int batchId)
        {
            List<Verse.Pawn> members = new List<Verse.Pawn>();
            IReadOnlyList<Verse.Pawn> spawned = map.mapPawns?.AllPawnsSpawned;
            if (spawned == null || batchId <= 0)
            {
                return members;
            }

            for (int i = 0; i < spawned.Count; i++)
            {
                Verse.Pawn pawn = spawned[i];
                CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
                if (comp != null && comp.State.IsActiveVisitor && comp.State.spawnBatchId == batchId)
                {
                    members.Add(pawn);
                }
            }

            return members;
        }
    }
}
