using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Incidents
{
    internal enum HungerIncidentFamily
    {
        Beggar = 0,
        Thief = 1,
        Wild = 2,
        Aid = 3,
        Intel = 4,
        Siege = 5,
        Trade = 6,
        Special = 7
    }

    internal enum HungerIncidentOrigin
    {
        Original = 0,
        Sequel = 1
    }

    internal enum HungerIncidentCategory
    {
        Hunger = 0,
        Plague = 1
    }

    internal enum HungerIncidentTarget
    {
        Map = 0,
        Caravan = 1
    }

    internal enum HungerAttitudePool
    {
        Positive = 0,
        Negative = 1
    }

    internal sealed class HungerIncidentEntry
    {
        public string DisplayId { get; }
        public string DefName { get; }
        public string LabelKey { get; }
        public HungerIncidentFamily Family { get; }
        public HungerIncidentOrigin Origin { get; }
        public HungerIncidentCategory Category { get; }
        public HungerIncidentTarget Target { get; }
        public bool BroadcastEligible { get; }
        public HungerAttitudePool DefaultAttitudePool { get; }
        public float DebugPoints { get; }

        public HungerIncidentEntry(
            string displayId,
            string defName,
            HungerIncidentFamily family,
            HungerIncidentOrigin origin,
            HungerIncidentCategory category,
            HungerIncidentTarget target,
            bool broadcastEligible,
            HungerAttitudePool defaultAttitudePool,
            float debugPoints)
        {
            DisplayId = displayId;
            DefName = defName;
            LabelKey = "RHAH_Incident_" + defName.Substring("RHAH_".Length) + "_Label";
            Family = family;
            Origin = origin;
            Category = category;
            Target = target;
            BroadcastEligible = broadcastEligible;
            DefaultAttitudePool = defaultAttitudePool;
            DebugPoints = debugPoints;
        }
    }

    internal static class HungerIncidentCatalog
    {
        static readonly List<HungerIncidentEntry> entries = new List<HungerIncidentEntry>
        {
            Original("I-001", "RHAH_LargeRefugeeWave", HungerIncidentFamily.Beggar, true, HungerAttitudePool.Negative, 700f),
            Original("I-002", "RHAH_AbandonedRatkinChildren", HungerIncidentFamily.Special, true, HungerAttitudePool.Negative, 400f),
            Original("I-003", "RHAH_ShatteredMother", HungerIncidentFamily.Special, true, HungerAttitudePool.Negative, 400f),
            Original("I-004", "RHAH_BeggarFamily", HungerIncidentFamily.Beggar, true, HungerAttitudePool.Negative, 200f),
            Original("I-005", "RHAH_BeggarGroup", HungerIncidentFamily.Beggar, true, HungerAttitudePool.Negative, 300f),
            Original("I-006", "RHAH_ThiefRatkinGroup", HungerIncidentFamily.Thief, true, HungerAttitudePool.Negative, 300f),
            Original("I-007", "RHAH_ThiefRatkinChildGroup", HungerIncidentFamily.Thief, true, HungerAttitudePool.Negative, 300f),
            Original("I-008", "RHAH_WildRatkinWandersIn", HungerIncidentFamily.Wild, true, HungerAttitudePool.Negative, 150f),
            Original("I-009", "RHAH_WildRatkinChildWandersIn", HungerIncidentFamily.Wild, true, HungerAttitudePool.Negative, 150f),
            Original("I-010", "RHAH_WildRatkinGroupWandersIn", HungerIncidentFamily.Wild, true, HungerAttitudePool.Negative, 400f),
            Original("I-011", "RHAH_FamineRefugees", HungerIncidentFamily.Beggar, true, HungerAttitudePool.Positive, 350f),
            Original("I-012", "RHAH_RatkinTraderCaravan", HungerIncidentFamily.Trade, true, HungerAttitudePool.Positive, 350f),
            Original("I-013", "RHAH_ChildExchange", HungerIncidentFamily.Special, true, HungerAttitudePool.Negative, 200f),
            Original("I-014", "RHAH_BeggarSiege", HungerIncidentFamily.Siege, false, HungerAttitudePool.Negative, 300f),
            Sequel("I-015", "RHAH_AidSimpleMeal", HungerIncidentFamily.Aid, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-016", "RHAH_AidFineMeal", HungerIncidentFamily.Aid, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-017", "RHAH_AidMedicine", HungerIncidentFamily.Aid, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-018", "RHAH_AidSilver", HungerIncidentFamily.Aid, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-019", "RHAH_AidBaby", HungerIncidentFamily.Aid, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-020", "RHAH_IntelTreasureSimpleMeal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-021", "RHAH_IntelTreasureHerbal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-022", "RHAH_IntelTreasureSilver", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-023", "RHAH_IntelStructureSimpleMeal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-024", "RHAH_IntelStructureHerbal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-025", "RHAH_IntelStructureSilver", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-026", "RHAH_IntelSettlementSimpleMeal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-027", "RHAH_IntelSettlementHerbal", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-028", "RHAH_IntelSettlementSilver", HungerIncidentFamily.Intel, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Positive, 300f),
            Sequel("I-029", "RHAH_LaboringRefugees", HungerIncidentFamily.Beggar, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 260f),
            Sequel("I-030", "RHAH_StrongSiege", HungerIncidentFamily.Siege, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 320f),
            Sequel("I-031", "RHAH_Passersby", HungerIncidentFamily.Beggar, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-032", "RHAH_AirdropMistake", HungerIncidentFamily.Special, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-033", "RHAH_MisguidedKinship", HungerIncidentFamily.Special, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 200f),
            Sequel("I-034", "RHAH_GreatFamine", HungerIncidentFamily.Thief, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 600f),
            Sequel("I-035", "RHAH_CaravanMuggers", HungerIncidentFamily.Thief, HungerIncidentCategory.Hunger, HungerIncidentTarget.Caravan, HungerAttitudePool.Negative, 320f),
            Sequel("I-036", "RHAH_PlagueWanderers", HungerIncidentFamily.Wild, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-037", "RHAH_PlagueAbandonedBabies", HungerIncidentFamily.Special, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-038", "RHAH_PlagueTraderCaravan", HungerIncidentFamily.Trade, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 350f),
            Sequel("I-039", "RHAH_PlaguePassersby", HungerIncidentFamily.Beggar, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 350f),
            Sequel("I-040", "RHAH_PlagueRefugees", HungerIncidentFamily.Beggar, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 350f),
            Sequel("I-041", "RHAH_PlagueOrphan", HungerIncidentFamily.Wild, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 200f),
            Sequel("I-042", "RHAH_PlagueBeggarGroup", HungerIncidentFamily.Beggar, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-043", "RHAH_PlagueThiefGroup", HungerIncidentFamily.Thief, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-044", "RHAH_PlagueLaboringRefugees", HungerIncidentFamily.Beggar, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 260f),
            Sequel("I-045", "RHAH_PlagueStrongSiege", HungerIncidentFamily.Siege, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 320f),
            Sequel("I-046", "RHAH_PlagueAirdropMistake", HungerIncidentFamily.Special, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f),
            Sequel("I-047", "RHAH_PlagueMisguidedKinship", HungerIncidentFamily.Special, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 200f),
            Sequel("I-048", "RHAH_PlagueGreatFamine", HungerIncidentFamily.Thief, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 600f),
            Sequel("I-049", "RHAH_PlagueRevenge", HungerIncidentFamily.Special, HungerIncidentCategory.Plague, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 100f),
            Sequel("I-050", "RHAH_PlagueCaravanMuggers", HungerIncidentFamily.Thief, HungerIncidentCategory.Plague, HungerIncidentTarget.Caravan, HungerAttitudePool.Negative, 320f),
            Sequel("I-051", "RHAH_RefugeeMassacre", HungerIncidentFamily.Special, HungerIncidentCategory.Hunger, HungerIncidentTarget.Map, HungerAttitudePool.Negative, 300f)
        };

        public static IReadOnlyList<HungerIncidentEntry> All => entries;

        public static HungerIncidentEntry GetByDisplayId(string displayId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].DisplayId, displayId, StringComparison.Ordinal))
                {
                    return entries[i];
                }
            }

            return null;
        }

        public static HungerIncidentEntry GetByDefName(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].DefName, defName, StringComparison.OrdinalIgnoreCase))
                {
                    return entries[i];
                }
            }

            return null;
        }

        static HungerIncidentEntry Original(
            string displayId,
            string defName,
            HungerIncidentFamily family,
            bool broadcastEligible,
            HungerAttitudePool pool,
            float debugPoints)
        {
            return new HungerIncidentEntry(
                displayId,
                defName,
                family,
                HungerIncidentOrigin.Original,
                HungerIncidentCategory.Hunger,
                HungerIncidentTarget.Map,
                broadcastEligible,
                pool,
                debugPoints);
        }

        static HungerIncidentEntry Sequel(
            string displayId,
            string defName,
            HungerIncidentFamily family,
            HungerIncidentCategory category,
            HungerIncidentTarget target,
            HungerAttitudePool pool,
            float debugPoints)
        {
            return new HungerIncidentEntry(
                displayId,
                defName,
                family,
                HungerIncidentOrigin.Sequel,
                category,
                target,
                broadcastEligible: false,
                pool,
                debugPoints);
        }
    }
}
