using System;
using HungerAndHavoc.Api;
using Verse;

namespace HungerAndHavoc.Identity
{
    internal sealed class HungerApiHost : IHungerApiHost
    {
        public IHungerPawn Get(Verse.Pawn pawn)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return null;
            }

            return comp.ToSnapshot();
        }

        public bool IsRatkin(Verse.Pawn pawn)
        {
            return HungerRace.IsRatkin(pawn?.def);
        }

        public IHungerPawn TryMarkOrigin(Verse.Pawn pawn, HungerPawnSeed seed)
        {
            if (pawn?.health == null || seed == null)
            {
                return null;
            }

            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                HediffDef def = DefDatabase<HediffDef>.GetNamed("RHAH_HungerMark", false);
                if (def == null)
                {
                    return null;
                }

                pawn.health.AddHediff(HediffMaker.MakeHediff(def, pawn));
                comp = CompHungerPawn.TryGet(pawn);
                if (comp == null)
                {
                    return null;
                }
            }

            comp.ApplySeed(seed);
            IHungerPawn snapshot = comp.ToSnapshot();
            HungerAndHavocApi.RaiseOriginMarked(pawn, snapshot);
            return snapshot;
        }

        public bool ReleaseToColony(Verse.Pawn pawn, HungerReleaseReason reason)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return false;
            }

            if (comp.State.IsReleased)
            {
                return true;
            }

            IHungerPawn current = comp.ToSnapshot();
            bool? decision = HungerPawnBehaviors.Query(
                handler => handler.ShouldReleaseToColony(pawn, current, reason));
            if (decision == false)
            {
                return false;
            }

            SetLifecycle(pawn, HungerLifecycle.Released);
            if (!comp.State.IsReleased)
            {
                return false;
            }

            global::HungerAndHavoc.Pawn.RHAH_VisitorGroup.NotifyReleased(pawn);
            HungerAndHavocApi.RaiseReleasedToColony(pawn, comp.ToSnapshot(), reason);
            return true;
        }

        public void SetLifecycle(Verse.Pawn pawn, HungerLifecycle lifecycle)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            if (!comp.State.TrySetLifecycle(lifecycle))
            {
                return;
            }

            HungerAndHavocApi.RaiseLifecycleChanged(pawn, comp.ToSnapshot(), lifecycle);
        }

        public bool Allows(Verse.Pawn pawn, HungerBehaviorGate gate)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return false;
            }

            IHungerPawn snapshot = comp.ToSnapshot();
            bool? gateOverride = comp.State.GetGateOverride(gate);
            bool allowed;
            if (gateOverride.HasValue)
            {
                allowed = gateOverride.Value;
            }
            else
            {
                bool? behavior = HungerPawnBehaviors.Query(
                    handler => handler.Allows(pawn, snapshot, gate));
                allowed = behavior ?? HungerPawnDefaults.Allows(snapshot, gate);
            }

            HungerAndHavocApi.RaiseGateQueried(pawn, snapshot, gate, allowed);
            return allowed;
        }

        public void SetGate(Verse.Pawn pawn, HungerBehaviorGate gate, bool? allowed)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            comp.State.SetGate(gate, allowed);
        }

        public void SetExtra(Verse.Pawn pawn, string key, string value)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            comp.State.SetExtra(key, value);
        }

        public bool TryGetExtra(Verse.Pawn pawn, string key, out string value)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            if (comp == null)
            {
                value = null;
                return false;
            }

            return comp.State.TryGetExtra(key, out value);
        }

        public void RegisterRatkinMatcher(Func<ThingDef, bool> matcher)
        {
            HungerRace.Register(matcher);
        }
    }
}
