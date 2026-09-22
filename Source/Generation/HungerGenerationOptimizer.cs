using RimWorld;
using Verse;

namespace HungerAndHavoc.Generation
{
    internal static class HungerGenerationOptimizer
    {
        internal const string DefaultXenotypeDefName = HungerGeneCatalog.DefaultXenotypeDefName;

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
            return HungerXenotypeResolver.Resolve(profile);
        }
    }
}
