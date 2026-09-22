using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HungerAndHavoc.Pawn
{
    // 自有访客图 不走 Ideology 门槛的原版乞讨 Lord
    public class LordJob_RHAH_Visitor : LordJob
    {
        Faction faction;
        IntVec3 waitSpot = IntVec3.Invalid;
        HungerPawnRole familyRole;

        public LordJob_RHAH_Visitor()
        {
        }

        public LordJob_RHAH_Visitor(Faction faction, IntVec3 waitSpot, HungerPawnRole familyRole)
        {
            this.faction = faction;
            this.waitSpot = waitSpot;
            this.familyRole = familyRole;
        }

        public override bool LostImportantReferenceDuringLoading
        {
            get
            {
                if (faction == null)
                {
                    return true;
                }

                if (lord != null && (lord.lordManager == null || lord.lordManager.map == null))
                {
                    return true;
                }

                return false;
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph graph = new StateGraph();
            LordToil_Travel travel = new LordToil_Travel(waitSpot);
            graph.AddToil(travel);
            graph.StartingToil = travel;

            LordToil_RHAH_VisitorSeek seek = new LordToil_RHAH_VisitorSeek(waitSpot);
            graph.AddToil(seek);

            LordToil_RHAH_VisitorLeave leave = new LordToil_RHAH_VisitorLeave();
            graph.AddToil(leave);

            // 空派系只提供敌对检查 不切原版防守或袭击

            Transition arrived = new Transition(travel, seek, false, true);
            arrived.AddTrigger(new Trigger_Memo("TravelArrived"));
            graph.AddTransition(arrived, false);

            Transition toLeave = new Transition(seek, leave, false, true);
            toLeave.AddSource(travel);
            toLeave.AddTrigger(new Trigger_Custom(_ => AllReadyToLeave()));
            toLeave.AddTrigger(new Trigger_Memo("RHAH_Leave"));
            toLeave.AddPostAction(new TransitionAction_EndAllJobs());
            graph.AddTransition(toLeave, false);


            return graph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref waitSpot, "waitSpot", IntVec3.Invalid);
            Scribe_Values.Look(ref familyRole, "familyRole", HungerPawnRole.Unspecified);
        }

        bool AllReadyToLeave()
        {
            if (lord == null || lord.ownedPawns == null)
            {
                return false;
            }

            int count = lord.ownedPawns.Count;
            if (count == 0)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
                if (snapshot == null)
                {
                    return false;
                }

                if (snapshot.Lifecycle == HungerLifecycle.Fed ||
                    snapshot.Lifecycle == HungerLifecycle.Leaving)
                {
                    continue;
                }

                if (!Compat.RHAH_ChildMovement.CanWalkOut(pawn) &&
                    (pawn.CarriedBy != null || HasCarrier(pawn)))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        bool HasCarrier(Verse.Pawn child)
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn adult = lord.ownedPawns[i];
                if (adult == null || adult == child || adult.Downed || adult.CarriedBy != null)
                {
                    continue;
                }

                if (HungerAndHavocApi.Allows(adult, HungerBehaviorGate.Carry) &&
                    Compat.RHAH_ChildMovement.CanWalkOut(adult))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed class LordToil_RHAH_VisitorSeek : LordToil
    {
        readonly IntVec3 waitSpot;

        public LordToil_RHAH_VisitorSeek(IntVec3 waitSpot)
        {
            this.waitSpot = waitSpot;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(
                    HungerAndHavocDefOf.RHAH_VisitorSeek,
                    waitSpot,
                    10f);
            }
        }
    }

    internal sealed class LordToil_RHAH_VisitorLeave : LordToil
    {
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(HungerAndHavocDefOf.RHAH_VisitorLeave);
            }
        }
    }
}
