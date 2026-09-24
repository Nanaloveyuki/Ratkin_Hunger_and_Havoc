using System.Collections.Generic;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Trade;
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
        RHAH_PawnRole familyRole;
        Verse.Pawn foodReceiver;
        ThingDef foodDef;
        int foodCount;

        public LordJob_RHAH_Visitor()
        {
        }

        public LordJob_RHAH_Visitor(Faction faction, IntVec3 waitSpot, RHAH_PawnRole familyRole)
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
            LordToil_RHAH_WaitFood waitFood = new LordToil_RHAH_WaitFood(this);
            graph.AddToil(waitFood);

            LordToil_RHAH_VisitorLeave leave = new LordToil_RHAH_VisitorLeave();
            graph.AddToil(leave);

            // 空派系只提供敌对检查 不切原版防守或袭击

            Transition arrived = new Transition(travel, seek, false, true);
            arrived.AddTrigger(new Trigger_Memo("TravelArrived"));
            graph.AddTransition(arrived, false);
            Transition toFood = new Transition(seek, waitFood, false, true);
            toFood.AddTrigger(new Trigger_Memo("RHAH_WaitFood"));
            graph.AddTransition(toFood, false);
            Transition foodDone = new Transition(waitFood, seek, false, true);
            foodDone.AddTrigger(new Trigger_Custom(_ => FoodFilled()));
            graph.AddTransition(foodDone, false);

            Transition toLeave = new Transition(seek, leave, false, true);
            toLeave.AddSource(travel);
            toLeave.AddTrigger(new Trigger_Custom(_ => AllReadyToLeave()));
            toLeave.AddTrigger(new Trigger_Memo("RHAH_Leave"));
            toLeave.AddTrigger(new Trigger_Custom(signal => TraderMustLeave(signal)));
            toLeave.AddPostAction(new TransitionAction_Custom((System.Action)(() =>
            {
                if (lord?.ownedPawns == null)
                {
                    return;
                }

                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Compat.RHAH_LeashBridge.ClearDeparture(lord.ownedPawns[i]);
                }
            })));
            toLeave.AddPostAction(new TransitionAction_EndAllJobs());
            graph.AddTransition(toLeave, false);


            return graph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref waitSpot, "waitSpot", IntVec3.Invalid);
            Scribe_Values.Look(ref familyRole, "familyRole", RHAH_PawnRole.Unspecified);
            Scribe_References.Look(ref foodReceiver, "foodReceiver");
            Scribe_Defs.Look(ref foodDef, "foodDef");
            Scribe_Values.Look(ref foodCount, "foodCount", 0);
        }

        internal void BeginFoodWait(Verse.Pawn receiver, int count)
        {
            foodReceiver = receiver;
            foodCount = count;
        }

        internal bool WaitingForFood(Verse.Pawn pawn)
        {
            return RHAH_RequestRules.FoodWaiting(
                foodReceiver == pawn,
                pawn != null && !pawn.Dead,
                pawn != null && pawn.Spawned,
                foodDef == null ? 0 : RHAH_FoodHandoff.Received(pawn, foodDef),
                foodCount);
        }

        internal int FoodStillNeeded(Verse.Pawn pawn)
        {
            if (!WaitingForFood(pawn))
            {
                return 0;
            }

            int received = foodDef == null ? 0 : RHAH_FoodHandoff.Received(pawn, foodDef);
            return foodCount - received;
        }

        internal void NoteFood(ThingDef def)
        {
            if (foodDef == null)
            {
                foodDef = def;
            }
        }

        bool FoodFilled()
        {
            return foodReceiver != null && foodDef != null && !WaitingForFood(foodReceiver);
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
                IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
                if (snapshot == null)
                {
                    return false;
                }

                if (snapshot.Lifecycle == RHAH_Lifecycle.Fed ||
                    snapshot.Lifecycle == RHAH_Lifecycle.Leaving)
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
        bool TraderMustLeave(TriggerSignal signal)
        {
            if (signal.type != TriggerSignalType.Tick || lord?.ownedPawns == null)
            {
                return false;
            }

            int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            if (tick % 197 != 0)
            {
                return false;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            bool ignoreHarsh = settings == null || settings.traderIgnoresHarshEnvironment;
            bool ignoreEnclosed = settings == null || settings.traderIgnoresEnclosedSpace;
            if (ignoreHarsh && ignoreEnclosed)
            {
                return false;
            }

            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                if (RHAH_CaravanStay.MemberMustLeave(pawn, ignoreHarsh, ignoreEnclosed))
                {
                    return true;
                }
            }

            return false;
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

                if (RHAH_Api.Allows(adult, RHAH_BehaviorGate.Carry) &&
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
                    RHAH_DefOf.RHAH_VisitorSeek,
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
                lord.ownedPawns[i].mindState.duty = new PawnDuty(RHAH_DefOf.RHAH_VisitorLeave);
            }
        }
    }

    internal sealed class LordToil_RHAH_WaitFood : LordToil, IWaitForItemsLordToil
    {
        readonly LordJob_RHAH_Visitor job;

        public LordToil_RHAH_WaitFood(LordJob_RHAH_Visitor job)
        {
            this.job = job;
        }

        public int CountRemaining => job == null || job.lord == null ? 0 : job.FoodStillNeeded(Receiver());

        public bool HasAllRequestedItems => CountRemaining <= 0;

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.WanderClose_NoNeeds, lord.ownedPawns[i].Position, 3f);
            }
        }

        public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Verse.Pawn requester, Verse.Pawn current)
        {
            if (!job.WaitingForFood(requester) || current == null || current.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            List<Thing> foods = new List<Thing>();
            RHAH_FoodHandoff.CollectFood(current, foods);
            int left = job.FoodStillNeeded(requester);
            if (foods.Count == 0)
            {
                yield return new FloatMenuOption("RHAH_Choice_NoFood".Translate(left, requester.LabelShort), null);
                yield break;
            }

            for (int i = 0; i < foods.Count; i++)
            {
                Thing food = foods[i];
                yield return GiveOption(food, current, requester, left);
            }
        }

        FloatMenuOption GiveOption(Thing food, Verse.Pawn current, Verse.Pawn requester, int left)
        {
            return new FloatMenuOption("RHAH_Choice_GiveFood".Translate(left + " " + food.def.label, requester.LabelShort), () =>
            {
                job.NoteFood(food.def);
                Job give = JobMaker.MakeJob(JobDefOf.GiveToPawn, food, requester);
                give.haulMode = HaulMode.ToContainer;
                give.count = left;
                current.jobs.TryTakeOrderedJob(give, JobTag.Misc);
            });
        }

        Verse.Pawn Receiver()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                if (job.WaitingForFood(lord.ownedPawns[i]))
                {
                    return lord.ownedPawns[i];
                }
            }

            return null;
        }
    }
}
