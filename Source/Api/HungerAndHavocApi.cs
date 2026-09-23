using System.Collections.Generic;
using System;
using Verse;

namespace HungerAndHavoc.Api
{
    public static class HungerAndHavocApi
    {
        public static event Action<Pawn, IHungerPawn> OriginMarked;
        public static event Action<Pawn, IHungerPawn, HungerReleaseReason> ReleasedToColony;
        public static event Action<Pawn, IHungerPawn, HungerLifecycle> LifecycleChanged;
        public static event Action<Pawn, IHungerPawn, HungerBehaviorGate, bool> GateQueried;

        static IHungerApiHost host;

        internal static void Bind(IHungerApiHost next)
        {
            host = next;
        }

        public static IHungerPawn Get(Pawn pawn)
        {
            if (host == null)
            {
                return null;
            }

            return host.Get(pawn);
        }

        public static bool IsOrigin(Pawn pawn)
        {
            return Get(pawn) != null;
        }

        public static bool IsVisitor(Pawn pawn)
        {
            IHungerPawn snapshot = Get(pawn);
            return snapshot != null && snapshot.IsActiveVisitor;
        }

        public static bool IsRatkin(Pawn pawn)
        {
            if (host == null)
            {
                return false;
            }

            return host.IsRatkin(pawn);
        }

        public static bool IsRatkinYoung(Pawn pawn)
        {
            IHungerPawn snapshot = Get(pawn);
            return IsRatkin(pawn) && snapshot != null && snapshot.Role == HungerPawnRole.RatkinYoung;
        }

        public static IHungerPawn TryMarkOrigin(Pawn pawn, HungerPawnSeed seed)
        {
            if (host == null)
            {
                return null;
            }

            return host.TryMarkOrigin(pawn, seed);
        }

        public static bool ReleaseToColony(Pawn pawn, HungerReleaseReason reason)
        {
            if (host == null)
            {
                return false;
            }

            return host.ReleaseToColony(pawn, reason);
        }

        public static void SetLifecycle(Pawn pawn, HungerLifecycle lifecycle)
        {
            if (host == null)
            {
                return;
            }

            host.SetLifecycle(pawn, lifecycle);
        }

        public static bool Allows(Pawn pawn, HungerBehaviorGate gate)
        {
            if (host == null)
            {
                return false;
            }

            return host.Allows(pawn, gate);
        }

        public static void SetGate(Pawn pawn, HungerBehaviorGate gate, bool? allowed)
        {
            if (host == null)
            {
                return;
            }

            host.SetGate(pawn, gate, allowed);
        }

        public static void SetExtra(Pawn pawn, string key, string value)
        {
            if (host == null)
            {
                return;
            }

            host.SetExtra(pawn, key, value);
        }

        public static bool TryGetExtra(Pawn pawn, string key, out string value)
        {
            if (host == null)
            {
                value = null;
                return false;
            }

            return host.TryGetExtra(pawn, key, out value);
        }

        public static void RegisterRatkinMatcher(Func<ThingDef, bool> matcher)
        {
            if (host == null || matcher == null)
            {
                return;
            }

            host.RegisterRatkinMatcher(matcher);
        }
        public static bool IsOwnedHistory(string backstoryDefName)
        {
            return host != null && host.IsOwnedHistory(backstoryDefName);
        }

        public static bool IsOwnedTrait(string traitDefName)
        {
            return host != null && host.IsOwnedTrait(traitDefName);
        }

        public static bool TryGetHistory(string displayId, out string backstoryDefName)
        {
            backstoryDefName = null;
            return host != null && host.TryGetHistory(displayId, out backstoryDefName);
        }

        public static bool TryGetTrait(string displayId, out string traitDefName)
        {
            traitDefName = null;
            return host != null && host.TryGetTrait(displayId, out traitDefName);
        }

        public static void CopyHistoryIds(List<string> destination)
        {
            if (destination == null)
            {
                return;
            }

            destination.Clear();
            host?.CopyHistoryIds(destination);
        }

        public static void CopyTraitIds(List<string> destination)
        {
            if (destination == null)
            {
                return;
            }

            destination.Clear();
            host?.CopyTraitIds(destination);
        }

        internal static void RaiseOriginMarked(Pawn pawn, IHungerPawn snapshot)
        {
            OriginMarked?.Invoke(pawn, snapshot);
        }

        internal static void RaiseReleasedToColony(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
        {
            ReleasedToColony?.Invoke(pawn, snapshot, reason);
        }

        internal static void RaiseLifecycleChanged(Pawn pawn, IHungerPawn snapshot, HungerLifecycle lifecycle)
        {
            LifecycleChanged?.Invoke(pawn, snapshot, lifecycle);
        }

        internal static void RaiseGateQueried(Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate, bool allowed)
        {
            GateQueried?.Invoke(pawn, snapshot, gate, allowed);
        }
    }
}
