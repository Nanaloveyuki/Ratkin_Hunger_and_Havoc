using System.Collections.Generic;
using HarmonyLib;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;

using VersePawn = Verse.Pawn;
namespace HungerAndHavoc.Generation
{
    internal static class RHAH_Fertility
    {
        const float VanillaLovinPregnancyChance = 0.05f;

        internal static bool Has(VersePawn pawn, string defName)
        {
            if (!ModsConfig.BiotechActive || pawn?.genes == null || string.IsNullOrEmpty(defName))
            {
                return false;
            }

            GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(defName);
            return gene != null && pawn.genes.HasActiveGene(gene);
        }

        internal static bool Either(VersePawn first, VersePawn second, string defName)
        {
            return Has(first, defName) || Has(second, defName);
        }

        internal static VersePawn Female(VersePawn first, VersePawn second)
        {
            if (first != null && first.gender == Gender.Female)
            {
                return first;
            }

            if (second != null && second.gender == Gender.Female)
            {
                return second;
            }

            return null;
        }

        internal static VersePawn Male(VersePawn first, VersePawn second)
        {
            if (first != null && first.gender == Gender.Male)
            {
                return first;
            }

            if (second != null && second.gender == Gender.Male)
            {
                return second;
            }

            return null;
        }

        internal static bool AgesAllow(VersePawn first, VersePawn second, float minimumYears)
        {
            return first?.ageTracker != null &&
                second?.ageTracker != null &&
                RHAH_FertilityRules.AgeAllows(first.ageTracker.AgeBiologicalYearsFloat, minimumYears) &&
                RHAH_FertilityRules.AgeAllows(second.ageTracker.AgeBiologicalYearsFloat, minimumYears);
        }

        internal static bool CanProduce(VersePawn first, VersePawn second)
        {
            if (first == null || second == null || first.Dead || second.Dead || first.gender == second.gender)
            {
                return false;
            }

            if (!Find.Storyteller.difficulty.ChildrenAllowed)
            {
                return false;
            }

            VersePawn female = Female(first, second);
            VersePawn male = Male(first, second);
            if (female == null || male == null || female.Sterile() || male.Sterile())
            {
                return false;
            }

            if (PregnancyUtility.GetPregnancyHediff(female) != null)
            {
                return false;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            float minimum = settings == null ? RHAH_FertilityRules.DefaultFertileAge : settings.fertileMinAge;
            bool early = Either(first, second, RHAH_FertilityRules.EarlyFertility);
            bool room = Has(female, RHAH_FertilityRules.RoomFertility);
            if (!early && !room)
            {
                return PregnancyUtility.CanEverProduceChild(first, second).Accepted;
            }

            return AgesAllow(first, second, early ? minimum : RHAH_FertilityRules.MinFertileAge);
        }

        internal static float PregnancyFactor(VersePawn woman, VersePawn man)
        {
            if (!Either(woman, man, RHAH_FertilityRules.HighFertility))
            {
                return 1f;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            float percent = settings == null ? RHAH_FertilityRules.DefaultFertilityPercent : settings.fertilityPercent;
            return RHAH_FertilityRules.FertilityFactor(percent);
        }

        internal static float GestationDays(VersePawn mother)
        {
            if (!Has(mother, RHAH_FertilityRules.FastBirth) || mother?.RaceProps == null)
            {
                return mother?.RaceProps == null ? 0f : mother.RaceProps.gestationPeriodDays;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            float requested = settings == null ? RHAH_FertilityRules.DefaultGestationDays : settings.gestationDays;
            return RHAH_FertilityRules.GestationDays(requested, mother.RaceProps.gestationPeriodDays);
        }

        internal static int LitterCount(VersePawn mother)
        {
            if (!Has(mother, RHAH_FertilityRules.LargeLitter))
            {
                return 1;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            int minimum = settings == null ? RHAH_FertilityRules.DefaultLitterMin : settings.litterMin;
            int peak = settings == null ? RHAH_FertilityRules.DefaultLitterPeak : settings.litterPeak;
            int maximum = settings == null ? RHAH_FertilityRules.DefaultLitterMax : settings.litterMax;
            return RHAH_FertilityRules.RollLitter(minimum, peak, maximum, () => Rand.Value);
        }

        internal static float PregnancyChance(VersePawn woman, VersePawn man)
        {
            float chance = PregnancyUtility.PregnancyChanceForPartners(woman, man);
            if (!Has(woman, RHAH_FertilityRules.RoomFertility))
            {
                return chance;
            }

            float lactation = LactationFactor(woman);
            if (lactation <= 0f || lactation >= 1f)
            {
                return chance;
            }

            return chance / lactation;
        }

        static float LactationFactor(VersePawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return 1f;
            }

            float factor = 1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff = hediffs[i];
                if (hediff?.def != HediffDefOf.Lactating || hediff.CurStage == null)
                {
                    continue;
                }

                factor *= hediff.CurStage.fertilityFactor;
            }

            return factor;
        }

        internal static void TryRoomConception(VersePawn initiator)
        {
            if (!ModsConfig.BiotechActive || initiator?.CurJob == null || initiator.CurJob.def != JobDefOf.Lovin)
            {
                return;
            }

            VersePawn partner = initiator.CurJob.targetA.Thing as VersePawn;
            VersePawn mother = Female(initiator, partner);
            VersePawn father = Male(initiator, partner);
            if (mother == null || father == null || !Has(mother, RHAH_FertilityRules.RoomFertility))
            {
                return;
            }

            if (!CanProduce(mother, father))
            {
                return;
            }

            float chance = VanillaLovinPregnancyChance * PregnancyChance(mother, father);
            if (!Rand.Chance(chance))
            {
                return;
            }

            Hediff_Pregnant pregnancy = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
            pregnancy.SetParents(null, father, null);
            mother.health.AddHediff(pregnancy);
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.CanEverProduceChild))]
    internal static class RHAH_EarlyFertilityPatch
    {
        static void Postfix(VersePawn first, VersePawn second, ref AcceptanceReport __result)
        {
            if (__result.Accepted || !RHAH_Fertility.Either(first, second, RHAH_FertilityRules.EarlyFertility))
            {
                return;
            }

            if (RHAH_Fertility.CanProduce(first, second))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.PregnancyChanceForPartners))]
    internal static class RHAH_HighFertilityPatch
    {
        static void Postfix(VersePawn woman, VersePawn man, ref float __result)
        {
            __result *= RHAH_Fertility.PregnancyFactor(woman, man);
        }
    }

    [HarmonyPatch(typeof(Hediff_Pregnant), "TickInterval")]
    internal static class RHAH_FastBirthPatch
    {
        static void Postfix(Hediff_Pregnant __instance, int delta)
        {
            VersePawn mother = __instance?.pawn;
            if (mother == null || __instance.Severity >= 1f || !RHAH_Fertility.Has(mother, RHAH_FertilityRules.FastBirth))
            {
                return;
            }

            float days = RHAH_Fertility.GestationDays(mother);
            float raceDays = mother.RaceProps == null ? 0f : mother.RaceProps.gestationPeriodDays;
            if (days <= 0f || raceDays <= 0f || days >= raceDays)
            {
                return;
            }

            float vanilla = PawnUtility.BodyResourceGrowthSpeed(mother) / (raceDays * 60000f);
            __instance.Severity += vanilla * (raceDays / days - 1f) * delta;
            if (__instance.Severity < 1f)
            {
                return;
            }

            __instance.Severity = 1f;
            if (mother.RaceProps.Humanlike)
            {
                __instance.StartLabor();
            }
            else
            {
                Hediff_Pregnant.DoBirthSpawn(mother, __instance.Father);
            }

            mother.health.RemoveHediff(__instance);
        }
    }

    [HarmonyPatch(typeof(JobGiver_DoLovin), "TryGiveJob")]
    internal static class RHAH_RoomLovinPatch
    {
        static void Postfix(VersePawn pawn, ref Job __result)
        {
            if (__result != null || !RHAH_Fertility.Has(pawn, RHAH_FertilityRules.RoomFertility) || pawn?.Map == null)
            {
                return;
            }

            if (pawn.CurJobDef == JobDefOf.Lovin || pawn.Drafted || !pawn.Awake())
            {
                return;
            }

            VersePawn partner = Partner(pawn);
            Building_Bed bed = partner == null ? null : pawn.CurrentBed() ?? partner.CurrentBed();
            if (partner == null || bed == null || !RHAH_Fertility.CanProduce(pawn, partner))
            {
                return;
            }

            __result = JobMaker.MakeJob(JobDefOf.Lovin, partner, bed);
        }

        static VersePawn Partner(VersePawn pawn)
        {
            if (pawn.relations == null)
            {
                return null;
            }

            List<DirectPawnRelation> relations = pawn.relations.DirectRelations;
            for (int i = 0; i < relations.Count; i++)
            {
                VersePawn other = relations[i].otherPawn;
                if (other == null || other.Map != pawn.Map || other.Dead || other.gender == pawn.gender)
                {
                    continue;
                }

                if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Lover, other) ||
                    pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, other) ||
                    pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, other))
                {
                    return other;
                }
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Lovin), "MakeNewToils")]
    internal static class RHAH_RoomBirthPatch
    {
        static void Postfix(JobDriver_Lovin __instance, ref IEnumerable<Toil> __result)
        {
            if (__instance?.pawn == null || __result == null)
            {
                return;
            }

            VersePawn initiator = __instance.pawn;
            __result = Finish(__result, initiator);
        }

        static IEnumerable<Toil> Finish(IEnumerable<Toil> source, VersePawn initiator)
        {
            foreach (Toil toil in source)
            {
                if (toil != null)
                {
                    toil.AddFinishAction(() => RHAH_Fertility.TryRoomConception(initiator));
                }

                yield return toil;
            }
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
    internal static class RHAH_LargeLitterPatch
    {
        static void Postfix(VersePawn geneticMother, Thing birtherThing, VersePawn father)
        {
            VersePawn mother = geneticMother ?? birtherThing as VersePawn;
            if (mother == null || !RHAH_Fertility.Has(mother, RHAH_FertilityRules.LargeLitter))
            {
                return;
            }

            int extra = RHAH_Fertility.LitterCount(mother) - 1;
            for (int i = 0; i < extra; i++)
            {
                VersePawn baby = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    mother.kindDef,
                    mother.Faction,
                    PawnGenerationContext.NonPlayer,
                    developmentalStages: DevelopmentalStage.Newborn));
                if (baby == null)
                {
                    continue;
                }

                if (mother.MapHeld != null)
                {
                    GenSpawn.Spawn(baby, mother.PositionHeld, mother.MapHeld);
                }

                baby.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                if (father != null)
                {
                    baby.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                }
            }
        }
    }
}
