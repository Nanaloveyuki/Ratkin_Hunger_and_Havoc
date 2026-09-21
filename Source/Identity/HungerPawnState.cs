using System.Collections.Generic;
using HungerAndHavoc.Api;

namespace HungerAndHavoc.Identity
{
    // 无 Verse 的可测状态 非法转换不改值
    internal sealed class HungerPawnState
    {
        internal string sourceIncidentDisplayId;
        internal int spawnBatchId;
        internal int relationshipGroupId;
        internal HungerPawnRole role;
        internal HungerLifecycle lifecycle = HungerLifecycle.Arriving;
        internal bool hasBeenFed;
        internal int leaveAfterGameTick = -1;
        internal bool carriesPlague;
        internal HungerAttitude attitudeAtArrival = HungerAttitude.Neutral;
        internal int parentPawnLoadId;
        internal List<int> childPawnLoadIds = new List<int>();
        internal Dictionary<HungerBehaviorGate, bool> gateOverrides = new Dictionary<HungerBehaviorGate, bool>();
        internal Dictionary<string, string> extraData = new Dictionary<string, string>();

        internal bool IsReleased => lifecycle == HungerLifecycle.Released;

        internal bool IsActiveVisitor =>
            lifecycle != HungerLifecycle.Released && lifecycle != HungerLifecycle.Dead;

        internal void ApplySeed(HungerPawnSeed seed)
        {
            if (seed == null)
            {
                return;
            }

            sourceIncidentDisplayId = seed.SourceIncidentDisplayId;
            spawnBatchId = seed.SpawnBatchId;
            relationshipGroupId = seed.RelationshipGroupId;
            role = seed.Role;
            lifecycle = seed.Lifecycle;
            carriesPlague = seed.CarriesPlague;
            attitudeAtArrival = seed.AttitudeAtArrival;
            leaveAfterGameTick = seed.LeaveAfterGameTick;
            parentPawnLoadId = seed.ParentPawnLoadId;
            childPawnLoadIds = seed.ChildPawnLoadIds == null
                ? new List<int>()
                : new List<int>(seed.ChildPawnLoadIds);
            gateOverrides = new Dictionary<HungerBehaviorGate, bool>();
            if (seed.GateOverrides != null)
            {
                foreach (KeyValuePair<HungerBehaviorGate, bool> pair in seed.GateOverrides)
                {
                    gateOverrides[pair.Key] = pair.Value;
                }
            }

            EnsureCollections();
        }

        internal bool TrySetLifecycle(HungerLifecycle next)
        {
            if (next == lifecycle)
            {
                return false;
            }

            if (!IsLegalTransition(lifecycle, next))
            {
                return false;
            }

            lifecycle = next;
            if (next == HungerLifecycle.Fed)
            {
                hasBeenFed = true;
            }

            return true;
        }

        // 覆盖优先 再行为 再默认 无 Pawn 时行为收到 null
        internal bool Allows(HungerBehaviorGate gate)
        {
            bool? gateOverride = GetGateOverride(gate);
            if (gateOverride.HasValue)
            {
                return gateOverride.Value;
            }

            IHungerPawn snapshot = ToSnapshot();
            bool? behavior = HungerPawnBehaviors.Query(
                handler => handler.Allows(null, snapshot, gate));
            return behavior ?? HungerPawnDefaults.Allows(snapshot, gate);
        }

        internal bool ReleaseToColony(HungerReleaseReason reason)
        {
            if (IsReleased)
            {
                return true;
            }

            IHungerPawn current = ToSnapshot();
            bool? decision = HungerPawnBehaviors.Query(
                handler => handler.ShouldReleaseToColony(null, current, reason));
            if (decision == false)
            {
                return false;
            }

            if (!TrySetLifecycle(HungerLifecycle.Released))
            {
                return false;
            }

            IHungerPawn snapshot = ToSnapshot();
            HungerAndHavocApi.RaiseLifecycleChanged(null, snapshot, HungerLifecycle.Released);
            HungerAndHavocApi.RaiseReleasedToColony(null, snapshot, reason);
            return true;
        }

        internal void SetGate(HungerBehaviorGate gate, bool? allowed)
        {
            EnsureCollections();
            gateOverrides.Remove(gate);
            if (allowed.HasValue)
            {
                gateOverrides[gate] = allowed.Value;
            }
        }

        internal bool? GetGateOverride(HungerBehaviorGate gate)
        {
            if (gateOverrides != null && gateOverrides.TryGetValue(gate, out bool allowed))
            {
                return allowed;
            }

            return null;
        }

        internal void SetExtra(string key, string value)
        {
            EnsureCollections();
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (value == null)
            {
                extraData.Remove(key);
                return;
            }

            extraData[key] = value;
        }

        internal bool TryGetExtra(string key, out string value)
        {
            if (string.IsNullOrEmpty(key) || extraData == null)
            {
                value = null;
                return false;
            }

            if (extraData.TryGetValue(key, out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        internal void EnsureCollections()
        {
            if (childPawnLoadIds == null)
            {
                childPawnLoadIds = new List<int>();
            }

            if (gateOverrides == null)
            {
                gateOverrides = new Dictionary<HungerBehaviorGate, bool>();
            }

            if (extraData == null)
            {
                extraData = new Dictionary<string, string>();
            }
        }

        internal HungerPawnSnapshot ToSnapshot()
        {
            return new HungerPawnSnapshot(
                sourceIncidentDisplayId,
                spawnBatchId,
                relationshipGroupId,
                role,
                lifecycle,
                hasBeenFed,
                leaveAfterGameTick,
                carriesPlague,
                attitudeAtArrival,
                parentPawnLoadId,
                childPawnLoadIds);
        }

        internal static bool IsLegalTransition(HungerLifecycle from, HungerLifecycle to)
        {
            switch (from)
            {
                case HungerLifecycle.Arriving:
                    return to == HungerLifecycle.SeekingFood ||
                           to == HungerLifecycle.Leaving ||
                           to == HungerLifecycle.Released ||
                           to == HungerLifecycle.Dead;
                case HungerLifecycle.SeekingFood:
                    return to == HungerLifecycle.Fed ||
                           to == HungerLifecycle.Leaving ||
                           to == HungerLifecycle.Released ||
                           to == HungerLifecycle.Dead;
                case HungerLifecycle.Fed:
                    return to == HungerLifecycle.Leaving ||
                           to == HungerLifecycle.Released ||
                           to == HungerLifecycle.Dead;
                case HungerLifecycle.Leaving:
                    return to == HungerLifecycle.Released ||
                           to == HungerLifecycle.Dead;
                case HungerLifecycle.Released:
                    return to == HungerLifecycle.Dead;
                default:
                    return false;
            }
        }
    }
}
