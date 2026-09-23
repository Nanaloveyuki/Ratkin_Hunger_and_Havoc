using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Data
{
    internal enum HungerHistorySlot
    {
        Childhood = 0,
        Adulthood = 1
    }

    internal enum HungerContentGender
    {
        Any = 0,
        Female = 1
    }

    internal enum HungerContentCategory
    {
        None = 0,
        Plague = 1,
        Theft = 2,
        Conflict = 3,
        Siege = 4
    }

    internal enum HungerTraitSlot
    {
        Any = 0,
        Childhood = 1,
        Adulthood = 2
    }

    internal sealed class HungerSkillRange
    {
        internal HungerSkillRange(string skillDefName, int minimum, int maximum)
        {
            SkillDefName = skillDefName;
            Minimum = minimum;
            Maximum = maximum;
        }

        internal string SkillDefName { get; }
        internal int Minimum { get; }
        internal int Maximum { get; }
    }

    internal sealed class HungerPassionRule
    {
        internal HungerPassionRule(string skillDefName, float activationChance, float majorChance)
        {
            SkillDefName = skillDefName;
            ActivationChance = activationChance;
            MajorChance = majorChance;
        }

        internal string SkillDefName { get; }
        internal float ActivationChance { get; }
        internal float MajorChance { get; }
    }

    internal sealed class HungerHistoryRecord
    {
        internal HungerHistoryRecord(
            string displayId,
            string backstoryDefName,
            HungerHistorySlot slot,
            float weight,
            float? minimumAge,
            bool minimumInclusive,
            float? maximumAge,
            bool maximumInclusive,
            float minimumTemperature,
            float maximumTemperature,
            HungerContentGender gender,
            string[] incidentDisplayIds,
            HungerContentCategory category,
            string[] linkedAdultDisplayIds,
            HungerSkillRange[] skills,
            HungerPassionRule[] passions,
            string[][] passionGroups,
            bool stripWeapons)
        {
            DisplayId = displayId;
            BackstoryDefName = backstoryDefName;
            Slot = slot;
            Weight = weight < 0.001f ? 0.001f : weight;
            MinimumAge = minimumAge;
            MinimumInclusive = minimumInclusive;
            MaximumAge = maximumAge;
            MaximumInclusive = maximumInclusive;
            MinimumTemperature = minimumTemperature;
            MaximumTemperature = maximumTemperature;
            Gender = gender;
            IncidentDisplayIds = incidentDisplayIds ?? Array.Empty<string>();
            Category = category;
            LinkedAdultDisplayIds = linkedAdultDisplayIds ?? Array.Empty<string>();
            Skills = skills ?? Array.Empty<HungerSkillRange>();
            Passions = passions ?? Array.Empty<HungerPassionRule>();
            PassionGroups = passionGroups ?? Array.Empty<string[]>();
            StripWeapons = stripWeapons;
        }

        internal string DisplayId { get; }
        internal string BackstoryDefName { get; }
        internal HungerHistorySlot Slot { get; }
        internal float Weight { get; }
        internal float? MinimumAge { get; }
        internal bool MinimumInclusive { get; }
        internal float? MaximumAge { get; }
        internal bool MaximumInclusive { get; }
        internal float MinimumTemperature { get; }
        internal float MaximumTemperature { get; }
        internal HungerContentGender Gender { get; }
        internal IReadOnlyList<string> IncidentDisplayIds { get; }
        internal HungerContentCategory Category { get; }
        internal IReadOnlyList<string> LinkedAdultDisplayIds { get; }
        internal IReadOnlyList<HungerSkillRange> Skills { get; }
        internal IReadOnlyList<HungerPassionRule> Passions { get; }
        internal IReadOnlyList<string[]> PassionGroups { get; }
        internal bool StripWeapons { get; }
    }

    internal sealed class HungerTraitRecord
    {
        internal HungerTraitRecord(
            string displayId,
            string traitDefName,
            HungerTraitSlot slot,
            float chance,
            string[] historyDisplayIds,
            string[] incidentDisplayIds,
            HungerContentCategory category,
            float red,
            float green,
            float blue)
        {
            DisplayId = displayId;
            TraitDefName = traitDefName;
            Slot = slot;
            Chance = chance < 0f ? 0f : (chance > 1f ? 1f : chance);
            HistoryDisplayIds = historyDisplayIds ?? Array.Empty<string>();
            IncidentDisplayIds = incidentDisplayIds ?? Array.Empty<string>();
            Category = category;
            Red = red;
            Green = green;
            Blue = blue;
        }

        internal string DisplayId { get; }
        internal string TraitDefName { get; }
        internal HungerTraitSlot Slot { get; }
        internal float Chance { get; }
        internal IReadOnlyList<string> HistoryDisplayIds { get; }
        internal IReadOnlyList<string> IncidentDisplayIds { get; }
        internal HungerContentCategory Category { get; }
        internal float Red { get; }
        internal float Green { get; }
        internal float Blue { get; }
    }
}
