using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_Feeding
    {
        internal static bool TryComplete(Verse.Pawn pawn)
        {
            if (!HungerAndHavocApi.IsVisitor(pawn) || pawn.needs == null || pawn.needs.food == null)
            {
                return false;
            }

            float level = pawn.needs.food.CurLevelPercentage;
            if (float.IsNaN(level) || float.IsInfinity(level) || level < RHAH_ReliefFood.SatisfiedLevel)
            {
                return false;
            }

            IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
            if (snapshot == null)
            {
                return false;
            }

            bool already = snapshot.HasBeenFed;
            if (!already && snapshot.Lifecycle == HungerLifecycle.SeekingFood)
            {
                HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Fed);
            }

            if (!already)
            {
                TryAddRefeeding(pawn);
            }

            return HungerAndHavocApi.Get(pawn) != null && HungerAndHavocApi.Get(pawn).HasBeenFed;
        }

        static void TryAddRefeeding(Verse.Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet == null)
            {
                return;
            }

            HediffDef syndrome = HungerAndHavocDefOf.RHAH_RefeedingSyndrome;
            if (syndrome == null || pawn.health.hediffSet.HasHediff(syndrome))
            {
                return;
            }

            Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
            if (malnutrition == null || malnutrition.Severity < RHAH_ReliefFood.RefeedMalnutrition)
            {
                return;
            }

            pawn.health.AddHediff(syndrome);
        }
    }
}
