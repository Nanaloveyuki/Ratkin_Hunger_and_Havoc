using RimWorld;
using Verse;

namespace HungerAndHavoc.Generation
{
    internal static class RHAH_GenerationOptimizer
    {
        internal const string DefaultXenotypeDefName = RHAH_GeneCatalog.DefaultXenotypeDefName;

        internal static bool Enabled =>
            Core.RHAH_Mod.Settings != null &&
            Core.RHAH_Mod.Settings.optimizeGeneration;

        internal static PawnGenerationRequest BuildRequest(RHAH_PawnRequest request, RHAH_PawnProfile profile)
        {
            RHAH_PawnProfile resolved = profile ?? new RHAH_PawnProfile();
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

        internal static XenotypeDef ResolveXenotype(RHAH_PawnProfile profile)
        {
            return RHAH_XenotypeResolver.Resolve(profile);
        }
    }
}
