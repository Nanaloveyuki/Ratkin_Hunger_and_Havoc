using System.Collections.Generic;
using HungerAndHavoc.Core;
using HungerAndHavoc.Data;
using RimWorld;
using Verse;
using VersePawn = Verse.Pawn;

namespace HungerAndHavoc.Generation
{
    internal static class HungerContentApplier
    {
        internal static void Apply(VersePawn pawn, HungerPawnRequest request, bool replaceBackstory)
        {
            if (pawn?.story == null || request == null)
            {
                return;
            }

            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            bool histories = settings == null || settings.pawnHistoriesEnabled;
            bool traits = settings == null || settings.pawnTraitsEnabled;
            if (!histories && !traits)
            {
                return;
            }

            float age = pawn.ageTracker != null ? pawn.ageTracker.AgeBiologicalYearsFloat : 0f;
            bool adult = pawn.DevelopmentalStage.Adult();
            float? temperature = request.Map?.mapTemperature?.OutdoorTemp;
            if (temperature.HasValue && (float.IsNaN(temperature.Value) || float.IsInfinity(temperature.Value)))
            {
                temperature = null;
            }

            HungerContentQuery query = new HungerContentQuery(
                age,
                adult,
                pawn.gender == Gender.Female,
                temperature,
                request.SourceIncidentDisplayId,
                histories && replaceBackstory,
                traits,
                id => settings == null || settings.IsHistoryEnabled(id),
                id => settings == null || settings.IsTraitEnabled(id),
                id => settings == null ? HungerContentCatalog.DefaultTraitWeight(id) : settings.TraitWeight(id));

            HungerHistoryRecord history = HungerContentSelector.SelectHistory(query, () => Rand.Value);
            if (history != null)
            {
                ApplyHistory(pawn, history, adult);
            }

            if (HasOwnedTrait(pawn))
            {
                return;
            }

            string historyId = history != null ? history.DisplayId : null;
            HungerContentSelector.SelectTrait(query, historyId, () => Rand.Value, record => TryGain(pawn, record));
        }

        static void ApplyHistory(VersePawn pawn, HungerHistoryRecord history, bool adult)
        {
            BackstoryDef backstory = DefDatabase<BackstoryDef>.GetNamedSilentFail(history.BackstoryDefName);
            if (backstory == null || pawn.story == null)
            {
                return;
            }

            if (history.Slot == HungerHistorySlot.Adulthood)
            {
                if (!adult)
                {
                    return;
                }

                pawn.story.Childhood = DefDatabase<BackstoryDef>.GetNamedSilentFail(HungerContentCatalog.FallbackChildhood);
                pawn.story.Adulthood = backstory;
            }
            else
            {
                pawn.story.Childhood = backstory;
                pawn.story.Adulthood = null;
            }

            ApplySkills(pawn, history);
            if (history.StripWeapons)
            {
                pawn.equipment?.DestroyAllEquipment(DestroyMode.Vanish);
            }
        }

        static void ApplySkills(VersePawn pawn, HungerHistoryRecord history)
        {
            if (pawn.skills == null)
            {
                return;
            }

            for (int i = 0; i < history.Skills.Count; i++)
            {
                HungerSkillRange range = history.Skills[i];
                SkillRecord record = Skill(pawn, range.SkillDefName);
                if (record == null)
                {
                    continue;
                }

                record.levelInt = Rand.RangeInclusive(range.Minimum, range.Maximum);
                record.xpSinceLastLevel = 0f;
                record.xpSinceMidnight = 0f;
            }

            for (int i = 0; i < history.Passions.Count; i++)
            {
                HungerPassionRule rule = history.Passions[i];
                SkillRecord record = Skill(pawn, rule.SkillDefName);
                if (record == null || record.TotallyDisabled)
                {
                    continue;
                }

                Passion passion = Passion.None;
                if (Rand.Chance(rule.ActivationChance))
                {
                    passion = Rand.Chance(rule.MajorChance) ? Passion.Major : Passion.Minor;
                }

                record.passion = passion;
            }

            for (int i = 0; i < history.PassionGroups.Count; i++)
            {
                string[] group = history.PassionGroups[i];
                List<SkillRecord> records = new List<SkillRecord>();
                bool already = false;
                for (int j = 0; j < group.Length; j++)
                {
                    SkillRecord record = Skill(pawn, group[j]);
                    if (record == null || record.TotallyDisabled)
                    {
                        continue;
                    }

                    if (record.passion > Passion.None)
                    {
                        already = true;
                    }

                    records.Add(record);
                }

                if (already || records.Count == 0)
                {
                    continue;
                }

                records[Rand.Range(0, records.Count)].passion = Passion.Minor;
            }
        }

        static bool TryGain(VersePawn pawn, HungerTraitRecord record)
        {
            if (record == null || pawn.story?.traits == null)
            {
                return false;
            }

            TraitDef def = DefDatabase<TraitDef>.GetNamedSilentFail(record.TraitDefName);
            if (def == null || Conflicts(pawn, def))
            {
                return false;
            }

            int degree = 0;
            if (def.degreeDatas != null && def.degreeDatas.Count > 0 && def.degreeDatas[0] != null)
            {
                degree = def.degreeDatas[0].degree;
            }

            pawn.story.traits.GainTrait(new Trait(def, degree, false), false);
            return true;
        }

        static bool HasOwnedTrait(VersePawn pawn)
        {
            List<Trait> traits = pawn.story?.traits?.allTraits;
            if (traits == null)
            {
                return false;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                Trait trait = traits[i];
                if (trait?.def != null && HungerContentCatalog.IsOwnedTrait(trait.def.defName))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Conflicts(VersePawn pawn, TraitDef def)
        {
            List<Trait> traits = pawn.story.traits.allTraits;
            if (traits == null || def.conflictingTraits == null)
            {
                return false;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                Trait trait = traits[i];
                if (trait?.def == null)
                {
                    continue;
                }

                if (trait.def == def)
                {
                    return true;
                }

                for (int j = 0; j < def.conflictingTraits.Count; j++)
                {
                    if (def.conflictingTraits[j] == trait.def)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static SkillRecord Skill(VersePawn pawn, string defName)
        {
            SkillDef def = DefDatabase<SkillDef>.GetNamedSilentFail(defName);
            return def == null ? null : pawn.skills.GetSkill(def);
        }
    }
}
