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
            float? age = WalkingAge(request);
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
                developmentalStages: StageFor(age),
                dontGiveWeapon: true,
                onlyUseForcedBackstories: explicitBackstory,
                forceNoGear: explicitApparel)
            {
                FixedGender = request.Gender,
                FixedBiologicalAge = age,
                ForceNoBackstory = explicitBackstory && resolved.Childhood == null && resolved.Adulthood == null
            };
            return generation;
        }

        // 与原版人类阶段一致 婴儿阶段会永久倒地 不能按成人生成
        internal static DevelopmentalStage StageFor(float? age)
        {
            if (!age.HasValue || float.IsNaN(age.Value) || float.IsInfinity(age.Value))
            {
                return DevelopmentalStage.Adult;
            }

            if (age.Value < 3f)
            {
                return DevelopmentalStage.Baby;
            }

            return age.Value < 13f ? DevelopmentalStage.Child : DevelopmentalStage.Adult;
        }

        static float? WalkingAge(RHAH_PawnRequest request)
        {
            if (!Identity.RHAH_PawnDefaults.IsYoungRole(request.Role))
            {
                return request.BiologicalAge;
            }

            return HungerAndHavoc.Pawn.RHAH_VisitorRules.WalkingAgeFloor(
                request.BiologicalAge,
                ModsConfig.IsActive("cyanobot.toddlers"));
        }

        internal static XenotypeDef ResolveXenotype(RHAH_PawnProfile profile)
        {
            return RHAH_XenotypeResolver.Resolve(profile);
        }
    }
}
