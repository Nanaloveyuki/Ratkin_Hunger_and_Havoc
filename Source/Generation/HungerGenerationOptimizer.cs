using RimWorld;
using Verse;

namespace HungerAndHavoc.Generation
{
    internal static class HungerGenerationOptimizer
    {
        internal const string DefaultXenotypeDefName = "RK_XenoType_Ratkin";

        internal static bool Enabled =>
            Core.HungerAndHavocMod.Settings != null &&
            Core.HungerAndHavocMod.Settings.optimizeGeneration;

        internal static PawnGenerationRequest BuildRequest(HungerPawnRequest request, HungerPawnProfile profile)
        {
            HungerPawnProfile resolved = profile ?? new HungerPawnProfile();
            XenotypeDef xenotype = ResolveXenotype(resolved);
            bool explicitBackstory = resolved.UseExplicitBackstory;
            bool explicitApparel = resolved.UseExplicitApparel;
            PawnGenerationRequest generation = new PawnGenerationRequest(
                request.PawnKind,
                request.Faction,
                PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                allowPregnant: false,
                allowFood: false,
                allowAddictions: false,
                forbidAnyTitle: true,
                forcedXenotype: xenotype,
                developmentalStages: DevelopmentalStage.Adult,
                dontGiveWeapon: true,
                onlyUseForcedBackstories: explicitBackstory,
                forceNoGear: explicitApparel)
            {
                FixedGender = request.Gender,
                FixedBiologicalAge = request.BiologicalAge,
                ForceNoBackstory = explicitBackstory && resolved.Childhood == null && resolved.Adulthood == null
            };
            return generation;
        }

        internal static XenotypeDef ResolveXenotype(HungerPawnProfile profile)
        {
            if (profile != null && profile.UseExplicitXenotype)
            {
                if (profile.Xenotype != null)
                {
                    return profile.Xenotype;
                }

                if (string.IsNullOrEmpty(profile.XenotypeDefName))
                {
                    return null;
                }

                return DefDatabase<XenotypeDef>.GetNamedSilentFail(profile.XenotypeDefName);
            }

            if (!Enabled || !ModsConfig.BiotechActive)
            {
                return null;
            }

            return DefDatabase<XenotypeDef>.GetNamedSilentFail(DefaultXenotypeDefName);
        }
    }
}
