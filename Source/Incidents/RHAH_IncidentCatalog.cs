using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Incidents
{
    internal enum RHAH_IncidentFamily
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

    internal enum RHAH_IncidentOrigin
    {
        Original = 0,
        Sequel = 1
    }

    internal enum RHAH_IncidentCategory
    {
        Hunger = 0,
        Plague = 1
    }

    internal enum RHAH_IncidentTarget
    {
        Map = 0,
        Caravan = 1
    }

    internal enum RHAH_AttitudePool
    {
        Positive = 0,
        Negative = 1
    }

    internal sealed class RHAH_IncidentEntry
    {
        public string DisplayId { get; }
        public string DefName { get; }
        public string LabelKey { get; }
        public RHAH_IncidentFamily Family { get; }
        public RHAH_IncidentOrigin Origin { get; }
        public RHAH_IncidentCategory Category { get; }
        public RHAH_IncidentTarget Target { get; }
        public bool BroadcastEligible { get; }
        public RHAH_AttitudePool DefaultAttitudePool { get; }
        public float DebugPoints { get; }

        public RHAH_IncidentEntry(
            string displayId,
            string defName,
            RHAH_IncidentFamily family,
            RHAH_IncidentOrigin origin,
            RHAH_IncidentCategory category,
            RHAH_IncidentTarget target,
            bool broadcastEligible,
            RHAH_AttitudePool defaultAttitudePool,
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

    internal static class RHAH_IncidentCatalog
    {
        static readonly List<RHAH_IncidentEntry> entries = new List<RHAH_IncidentEntry>
        {
            Original("I-001", "RHAH_LargeRefugeeWave", RHAH_IncidentFamily.Beggar, true, RHAH_AttitudePool.Negative, 700f),
            Original("I-002", "RHAH_AbandonedRatkinChildren", RHAH_IncidentFamily.Special, true, RHAH_AttitudePool.Negative, 400f),
            Original("I-003", "RHAH_ShatteredMother", RHAH_IncidentFamily.Special, true, RHAH_AttitudePool.Negative, 400f),
            Original("I-004", "RHAH_BeggarFamily", RHAH_IncidentFamily.Beggar, true, RHAH_AttitudePool.Negative, 200f),
            Original("I-005", "RHAH_BeggarGroup", RHAH_IncidentFamily.Beggar, true, RHAH_AttitudePool.Negative, 300f),
            Original("I-006", "RHAH_ThiefRatkinGroup", RHAH_IncidentFamily.Thief, true, RHAH_AttitudePool.Negative, 300f),
            Original("I-007", "RHAH_ThiefRatkinChildGroup", RHAH_IncidentFamily.Thief, true, RHAH_AttitudePool.Negative, 300f),
            Original("I-008", "RHAH_WildRatkinWandersIn", RHAH_IncidentFamily.Wild, true, RHAH_AttitudePool.Negative, 150f),
            Original("I-009", "RHAH_WildRatkinChildWandersIn", RHAH_IncidentFamily.Wild, true, RHAH_AttitudePool.Negative, 150f),
            Original("I-010", "RHAH_WildRatkinGroupWandersIn", RHAH_IncidentFamily.Wild, true, RHAH_AttitudePool.Negative, 400f),
            Original("I-011", "RHAH_FamineRefugees", RHAH_IncidentFamily.Beggar, true, RHAH_AttitudePool.Positive, 350f),
            Original("I-012", "RHAH_RatkinTraderCaravan", RHAH_IncidentFamily.Trade, true, RHAH_AttitudePool.Positive, 350f),
            Original("I-013", "RHAH_ChildExchange", RHAH_IncidentFamily.Special, true, RHAH_AttitudePool.Negative, 200f),
            Original("I-014", "RHAH_BeggarSiege", RHAH_IncidentFamily.Siege, false, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-015", "RHAH_AidSimpleMeal", RHAH_IncidentFamily.Aid, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-016", "RHAH_AidFineMeal", RHAH_IncidentFamily.Aid, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-017", "RHAH_AidMedicine", RHAH_IncidentFamily.Aid, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-018", "RHAH_AidSilver", RHAH_IncidentFamily.Aid, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-019", "RHAH_AidBaby", RHAH_IncidentFamily.Aid, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-020", "RHAH_IntelTreasureSimpleMeal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-021", "RHAH_IntelTreasureHerbal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-022", "RHAH_IntelTreasureSilver", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-023", "RHAH_IntelStructureSimpleMeal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-024", "RHAH_IntelStructureHerbal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-025", "RHAH_IntelStructureSilver", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-026", "RHAH_IntelSettlementSimpleMeal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-027", "RHAH_IntelSettlementHerbal", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-028", "RHAH_IntelSettlementSilver", RHAH_IncidentFamily.Intel, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Positive, 300f),
            Sequel("I-029", "RHAH_LaboringRefugees", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 260f),
            Sequel("I-030", "RHAH_StrongSiege", RHAH_IncidentFamily.Siege, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 320f),
            Sequel("I-031", "RHAH_Passersby", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-032", "RHAH_AirdropMistake", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-033", "RHAH_MisguidedKinship", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 200f),
            Sequel("I-034", "RHAH_GreatFamine", RHAH_IncidentFamily.Thief, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 600f),
            Sequel("I-035", "RHAH_CaravanMuggers", RHAH_IncidentFamily.Thief, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Caravan, RHAH_AttitudePool.Negative, 320f),
            Sequel("I-036", "RHAH_PlagueWanderers", RHAH_IncidentFamily.Wild, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-037", "RHAH_PlagueAbandonedBabies", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-038", "RHAH_PlagueTraderCaravan", RHAH_IncidentFamily.Trade, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 350f),
            Sequel("I-039", "RHAH_PlaguePassersby", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 350f),
            Sequel("I-040", "RHAH_PlagueRefugees", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 350f),
            Sequel("I-041", "RHAH_PlagueOrphan", RHAH_IncidentFamily.Wild, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 200f),
            Sequel("I-042", "RHAH_PlagueBeggarGroup", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-043", "RHAH_PlagueThiefGroup", RHAH_IncidentFamily.Thief, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-044", "RHAH_PlagueLaboringRefugees", RHAH_IncidentFamily.Beggar, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 260f),
            Sequel("I-045", "RHAH_PlagueStrongSiege", RHAH_IncidentFamily.Siege, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 320f),
            Sequel("I-046", "RHAH_PlagueAirdropMistake", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f),
            Sequel("I-047", "RHAH_PlagueMisguidedKinship", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 200f),
            Sequel("I-048", "RHAH_PlagueGreatFamine", RHAH_IncidentFamily.Thief, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 600f),
            Sequel("I-049", "RHAH_PlagueRevenge", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 100f),
            Sequel("I-050", "RHAH_PlagueCaravanMuggers", RHAH_IncidentFamily.Thief, RHAH_IncidentCategory.Plague, RHAH_IncidentTarget.Caravan, RHAH_AttitudePool.Negative, 320f),
            Sequel("I-051", "RHAH_RefugeeMassacre", RHAH_IncidentFamily.Special, RHAH_IncidentCategory.Hunger, RHAH_IncidentTarget.Map, RHAH_AttitudePool.Negative, 300f)
        };

        public static IReadOnlyList<RHAH_IncidentEntry> All => entries;

        public static RHAH_IncidentEntry GetByDisplayId(string displayId)
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

        public static RHAH_IncidentEntry GetByDefName(string defName)
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

        static RHAH_IncidentEntry Original(
            string displayId,
            string defName,
            RHAH_IncidentFamily family,
            bool broadcastEligible,
            RHAH_AttitudePool pool,
            float debugPoints)
        {
            return new RHAH_IncidentEntry(
                displayId,
                defName,
                family,
                RHAH_IncidentOrigin.Original,
                RHAH_IncidentCategory.Hunger,
                RHAH_IncidentTarget.Map,
                broadcastEligible,
                pool,
                debugPoints);
        }

        static RHAH_IncidentEntry Sequel(
            string displayId,
            string defName,
            RHAH_IncidentFamily family,
            RHAH_IncidentCategory category,
            RHAH_IncidentTarget target,
            RHAH_AttitudePool pool,
            float debugPoints)
        {
            return new RHAH_IncidentEntry(
                displayId,
                defName,
                family,
                RHAH_IncidentOrigin.Sequel,
                category,
                target,
                broadcastEligible: false,
                pool,
                debugPoints);
        }
    }
}
