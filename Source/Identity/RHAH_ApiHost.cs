using System.Collections.Generic;
using System;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using Verse;

namespace HungerAndHavoc.Identity
{
    internal sealed class RHAH_ApiHost : IRHAH_ApiHost
    {
        public IRHAH_Pawn Get(Verse.Pawn pawn)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return null;
            }

            return comp.ToSnapshot();
        }

        public bool IsRatkin(Verse.Pawn pawn)
        {
            return RHAH_Race.IsRatkin(pawn?.def);
        }

        public IRHAH_Pawn TryMarkOrigin(Verse.Pawn pawn, RHAH_PawnSeed seed)
        {
            if (pawn?.health == null || seed == null)
            {
                return null;
            }

            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                HediffDef def = DefDatabase<HediffDef>.GetNamed("RHAH_HungerMark", false);
                if (def == null)
                {
                    return null;
                }

                pawn.health.AddHediff(HediffMaker.MakeHediff(def, pawn));
                comp = CompRHAH_Pawn.TryGet(pawn);
                if (comp == null)
                {
                    return null;
                }
            }

            comp.ApplySeed(seed);
            IRHAH_Pawn snapshot = comp.ToSnapshot();
            RHAH_Api.RaiseOriginMarked(pawn, snapshot);
            return snapshot;
        }

        public bool ReleaseToColony(Verse.Pawn pawn, RHAH_ReleaseReason reason)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return false;
            }

            if (comp.State.IsReleased)
            {
                return true;
            }

            IRHAH_Pawn current = comp.ToSnapshot();
            bool? decision = RHAH_PawnBehaviors.Query(
                handler => handler.ShouldReleaseToColony(pawn, current, reason));
            if (decision == false)
            {
                return false;
            }

            SetLifecycle(pawn, RHAH_Lifecycle.Released);
            if (!comp.State.IsReleased)
            {
                return false;
            }

            global::HungerAndHavoc.Pawn.RHAH_VisitorGroup.NotifyReleased(pawn);
            global::HungerAndHavoc.Pawn.Compat.RHAH_LeashBridge.ClearDeparture(pawn);
            RHAH_Api.RaiseReleasedToColony(pawn, comp.ToSnapshot(), reason);
            return true;
        }

        public void SetLifecycle(Verse.Pawn pawn, RHAH_Lifecycle lifecycle)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            if (!comp.State.TrySetLifecycle(lifecycle))
            {
                return;
            }

            RHAH_Api.RaiseLifecycleChanged(pawn, comp.ToSnapshot(), lifecycle);
        }

        public bool Allows(Verse.Pawn pawn, RHAH_BehaviorGate gate)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return false;
            }

            IRHAH_Pawn snapshot = comp.ToSnapshot();
            bool? gateOverride = comp.State.GetGateOverride(gate);
            bool allowed;
            if (gateOverride.HasValue)
            {
                allowed = gateOverride.Value;
            }
            else
            {
                bool? behavior = RHAH_PawnBehaviors.Query(
                    handler => handler.Allows(pawn, snapshot, gate));
                allowed = behavior ?? RHAH_PawnDefaults.Allows(snapshot, gate);
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            bool blockJoin = settings == null || settings.plagueQuarantineBlocksJoin;
            if (RHAH_Plague.BlocksGate(gate, RHAH_PlagueRuntime.IsQuarantined(pawn), blockJoin))
            {
                allowed = false;
            }

            RHAH_Api.RaiseGateQueried(pawn, snapshot, gate, allowed);
            return allowed;
        }

        public void SetGate(Verse.Pawn pawn, RHAH_BehaviorGate gate, bool? allowed)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            comp.State.SetGate(gate, allowed);
        }

        public void SetExtra(Verse.Pawn pawn, string key, string value)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            comp.State.SetExtra(key, value);
        }

        public bool TryGetExtra(Verse.Pawn pawn, string key, out string value)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                value = null;
                return false;
            }

            return comp.State.TryGetExtra(key, out value);
        }

        public void RegisterRatkinMatcher(Func<ThingDef, bool> matcher)
        {
            RHAH_Race.Register(matcher);
        }
        public bool IsOwnedHistory(string backstoryDefName)
        {
            return Data.RHAH_ContentCatalog.IsOwnedHistory(backstoryDefName);
        }

        public bool IsOwnedTrait(string traitDefName)
        {
            return Data.RHAH_ContentCatalog.IsOwnedTrait(traitDefName);
        }

        public bool TryGetHistory(string displayId, out string backstoryDefName)
        {
            Data.RHAH_HistoryRecord record = Data.RHAH_ContentCatalog.FindHistory(displayId);
            backstoryDefName = record?.BackstoryDefName;
            return record != null;
        }

        public bool TryGetTrait(string displayId, out string traitDefName)
        {
            Data.RHAH_TraitRecord record = Data.RHAH_ContentCatalog.FindTrait(displayId);
            traitDefName = record?.TraitDefName;
            return record != null;
        }

        public void CopyHistoryIds(List<string> destination)
        {
            if (destination == null)
            {
                return;
            }

            IReadOnlyList<Data.RHAH_HistoryRecord> records = Data.RHAH_ContentCatalog.Histories;
            for (int i = 0; i < records.Count; i++)
            {
                destination.Add(records[i].DisplayId);
            }
        }

        public void CopyTraitIds(List<string> destination)
        {
            if (destination == null)
            {
                return;
            }

            IReadOnlyList<Data.RHAH_TraitRecord> records = Data.RHAH_ContentCatalog.Traits;
            for (int i = 0; i < records.Count; i++)
            {
                destination.Add(records[i].DisplayId);
            }
        }
    }
}
