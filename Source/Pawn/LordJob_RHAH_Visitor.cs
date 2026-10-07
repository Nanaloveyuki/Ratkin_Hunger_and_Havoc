using System.Collections.Generic;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
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
        List<Verse.Pawn> foodReceivers;
        ThingDef foodDef;
        int foodCount;

        internal bool PreserveForAttitudeChange { get; set; }

        public override bool ShouldRemovePawn(Verse.Pawn pawn, PawnLostCondition reason)
        {
            return reason != PawnLostCondition.Incapped &&
                !(reason == PawnLostCondition.ChangedFaction && PreserveForAttitudeChange);
        }

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
            LordToil_Travel travel = new LordToil_RHAH_VisitorTravel(waitSpot);
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
            toFood.AddSource(travel);
            toFood.AddTrigger(new Trigger_Memo("RHAH_WaitFood"));
            graph.AddTransition(toFood, false);
            Transition toLeave = new Transition(seek, leave, false, true);
            toLeave.AddSource(travel);
            toLeave.AddSource(waitFood);
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
            Transition foodDone = new Transition(waitFood, seek, false, true);
            foodDone.AddTrigger(new Trigger_Custom(_ => FoodFilled()));
            foodDone.AddTrigger(new Trigger_Custom(_ => FoodWaitExpired()));
            graph.AddTransition(foodDone, false);


            return graph;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                MigrateFoodReceiver(Scribe.loader.curXmlParent);
            }
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref waitSpot, "waitSpot", IntVec3.Invalid);
            Scribe_Values.Look(ref familyRole, "familyRole", RHAH_PawnRole.Unspecified);
            Scribe_Collections.Look(ref foodReceivers, "foodReceivers", LookMode.Reference);
            Scribe_Defs.Look(ref foodDef, "foodDef");
            Scribe_Values.Look(ref foodCount, "foodCount", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && foodReceivers == null)
            {
                foodReceivers = new List<Verse.Pawn>();
            }
        }

        // 只预处理当前 Lord 的旧键 引用仍交给原版解析
        static void MigrateFoodReceiver(System.Xml.XmlNode node)
        {
            System.Xml.XmlElement legacy = node?["foodReceiver"];
            if (legacy == null)
            {
                return;
            }

            if (node["foodReceivers"] == null)
            {
                System.Xml.XmlElement receivers = node.OwnerDocument.CreateElement("foodReceivers");
                string reference = legacy.InnerText.Trim();
                if (legacy.GetAttribute("IsNull") != "True" && reference.Length > 0 && reference != "null")
                {
                    System.Xml.XmlElement item = node.OwnerDocument.CreateElement("li");
                    item.InnerText = reference;
                    receivers.AppendChild(item);
                }
                node.InsertBefore(receivers, legacy);
            }
            node.RemoveChild(legacy);
        }

        internal bool BeginFoodWait(Verse.Pawn receiver, int count)
        {
            if (receiver == null || count <= 0)
            {
                return false;
            }

            if (foodReceivers == null)
            {
                foodReceivers = new List<Verse.Pawn>();
            }

            bool added = !foodReceivers.Contains(receiver);
            if (added)
            {
                foodReceivers.Add(receiver);
            }

            foodCount = count;
            return added;
        }

        internal bool WaitingForFood(Verse.Pawn pawn)
        {
            return RHAH_RequestRules.FoodWaiting(
                ListedForFood(pawn),
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

        internal int FoodLeftToCollect(Verse.Pawn pawn)
        {
            int needed = FoodStillNeeded(pawn);
            if (needed <= 0)
            {
                return 0;
            }

            return needed - RHAH_FoodHandoff.BeingHauledTo(pawn);
        }

        internal ThingDef RequestedFood
        {
            get
            {
                return foodDef;
            }
        }

        internal void NoteFood(ThingDef def)
        {
            if (foodDef == null)
            {
                foodDef = def;
            }
        }

        internal bool FoodFilled()
        {
            if (foodReceivers == null || foodReceivers.Count == 0 || foodDef == null)
            {
                return false;
            }

            for (int i = 0; i < foodReceivers.Count; i++)
            {
                if (WaitingForFood(foodReceivers[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool ListedForFood(Verse.Pawn pawn)
        {
            return pawn != null && foodReceivers != null && foodReceivers.Contains(pawn);
        }

        bool FoodWaitExpired()
        {
            if (foodReceivers == null)
            {
                return false;
            }

            for (int i = 0; i < foodReceivers.Count; i++)
            {
                Verse.Pawn receiver = foodReceivers[i];
                if (FoodWaitExpired(receiver, receiver == null ? null : RHAH_Api.Get(receiver)))
                {
                    return true;
                }
            }

            return false;
        }

        static bool FoodWaitExpired(Verse.Pawn pawn, IRHAH_Pawn snapshot)
        {
            if (pawn == null || snapshot == null || Find.TickManager == null)
            {
                return false;
            }

            return RHAH_VisitorRules.NoFoodWaitExpired(
                true,
                snapshot.HasBeenFed,
                Find.TickManager.TicksGame,
                FoodWaitDeadline(pawn));
        }

        static int FoodWaitDeadline(Verse.Pawn pawn)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            return comp == null ? -1 : comp.State.foodWaitUntilTick;
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
                if (snapshot == null || !RHAH_BatchAttitude.CanOrderLeave(pawn))
                {
                    return false;
                }

                if (ReadyToLeave(pawn, snapshot))
                {
                    continue;
                }


                return false;
            }

            return true;
        }
        internal static bool ReadyToLeave(Verse.Pawn pawn, IRHAH_Pawn snapshot)
        {
            if (snapshot.Lifecycle == RHAH_Lifecycle.Leaving)
            {
                return true;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            int now = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            return (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.LeaveAfterFed) &&
                RHAH_VisitorRules.FedLeaveDue(settings == null || settings.leaveAfterFed,
                    snapshot.HasBeenFed, now, snapshot.LeaveAfterGameTick, pawn.Downed)) ||
                FoodWaitExpired(pawn, snapshot);
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

    }

    internal sealed class LordToil_RHAH_VisitorTravel : LordToil_Travel
    {
        internal LordToil_RHAH_VisitorTravel(IntVec3 destination) : base(destination)
        {
        }

        public override void UpdateAllDuties()
        {
            // TravelOrLeave 到不了点就出图 入场点在边缘时会立刻离开
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                PawnDuty duty = RHAH_Api.Get(pawn)?.Lifecycle == RHAH_Lifecycle.Leaving
                    ? new PawnDuty(RHAH_DefOf.RHAH_VisitorLeave)
                    : new PawnDuty(RimWorld.DutyDefOf.TravelOrWait, Data.dest, -1f);
                pawn.mindState.duty = duty;
            }
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
                Verse.Pawn pawn = lord.ownedPawns[i];
                pawn.mindState.duty = RHAH_Api.Get(pawn)?.Lifecycle == RHAH_Lifecycle.Leaving
                    ? new PawnDuty(RHAH_DefOf.RHAH_VisitorLeave)
                    : new PawnDuty(RHAH_DefOf.RHAH_VisitorSeek, waitSpot, 10f);
            }
        }
    }

    internal sealed class LordToil_RHAH_VisitorLeave : LordToil
    {
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                if (!RHAH_BatchAttitude.CanOrderLeave(pawn))
                {
                    continue;
                }
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving);
                pawn.mindState.duty = new PawnDuty(RHAH_DefOf.RHAH_VisitorLeave);
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

        public int CountRemaining => job == null ? 0 : job.FoodLeftToCollect(Receiver());

        public bool HasAllRequestedItems => job != null && job.FoodFilled();

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                pawn.mindState.duty = RHAH_Api.Get(pawn)?.Lifecycle == RHAH_Lifecycle.Leaving
                    ? new PawnDuty(RHAH_DefOf.RHAH_VisitorLeave)
                    : new PawnDuty(DutyDefOf.WanderClose_NoNeeds, pawn.Position, 3f);
            }
        }

        public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Verse.Pawn requester, Verse.Pawn current)
        {
            yield break;
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
