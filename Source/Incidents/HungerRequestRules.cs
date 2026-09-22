using Verse;
using System.Collections.Generic;

namespace HungerAndHavoc.Incidents
{
    public enum HungerRequestKind
    {
        None = 0,
        SimpleMeal = 1,
        FineMeal = 2,
        Medicine = 3,
        Silver = 4,
        Baby = 5,
        HerbalMedicine = 6
    }

    public enum HungerIntelSiteKind
    {
        None = 0,
        Treasure = 1,
        Structure = 2,
        Settlement = 3
    }

    public enum HungerChoiceKind
    {
        None = 0,
        Aid = 1,
        Intel = 2,
        Refugees = 3,
        Abandoned = 4,
        ChildExchange = 5,
        Kinship = 6,
        Airdrop = 7,
        Visitors = 8
    }

    public enum HungerChoiceAction
    {
        None = 0,
        Deliver = 1,
        Reject = 2,
        Ignore = 3,
        Join = 4,
        Hire = 5,
        Feed = 6,
        Timeout = 7
    }

    internal readonly struct HungerRequestSpec
    {
        public HungerRequestKind Kind { get; }
        public HungerIntelSiteKind Site { get; }
        public HungerChoiceKind Choice { get; }

        public HungerRequestSpec(HungerRequestKind kind, HungerIntelSiteKind site, HungerChoiceKind choice)
        {
            Kind = kind;
            Site = site;
            Choice = choice;
        }
    }

    internal static class HungerRequestRules
    {
        internal const int MinSimple = 6;
        internal const int MaxSimple = 28;
        internal const int MinFine = 4;
        internal const int MaxFine = 18;
        internal const int MinMedicine = 2;
        internal const int MaxMedicine = 10;
        internal const int MinSilver = 80;
        internal const int MaxSilver = 1200;
        internal const int MinHerbal = 3;
        internal const int MaxHerbal = 15;
        internal const int RequestDays = 1;
        internal const int TicksPerDay = 60000;

        internal static HungerRequestSpec SpecFor(string displayId)
        {
            switch (displayId)
            {
                case "I-015": return new HungerRequestSpec(HungerRequestKind.SimpleMeal, HungerIntelSiteKind.None, HungerChoiceKind.Aid);
                case "I-016": return new HungerRequestSpec(HungerRequestKind.FineMeal, HungerIntelSiteKind.None, HungerChoiceKind.Aid);
                case "I-017": return new HungerRequestSpec(HungerRequestKind.Medicine, HungerIntelSiteKind.None, HungerChoiceKind.Aid);
                case "I-018": return new HungerRequestSpec(HungerRequestKind.Silver, HungerIntelSiteKind.None, HungerChoiceKind.Aid);
                case "I-019": return new HungerRequestSpec(HungerRequestKind.Baby, HungerIntelSiteKind.None, HungerChoiceKind.Aid);
                case "I-020": return new HungerRequestSpec(HungerRequestKind.SimpleMeal, HungerIntelSiteKind.Treasure, HungerChoiceKind.Intel);
                case "I-021": return new HungerRequestSpec(HungerRequestKind.HerbalMedicine, HungerIntelSiteKind.Treasure, HungerChoiceKind.Intel);
                case "I-022": return new HungerRequestSpec(HungerRequestKind.Silver, HungerIntelSiteKind.Treasure, HungerChoiceKind.Intel);
                case "I-023": return new HungerRequestSpec(HungerRequestKind.SimpleMeal, HungerIntelSiteKind.Structure, HungerChoiceKind.Intel);
                case "I-024": return new HungerRequestSpec(HungerRequestKind.HerbalMedicine, HungerIntelSiteKind.Structure, HungerChoiceKind.Intel);
                case "I-025": return new HungerRequestSpec(HungerRequestKind.Silver, HungerIntelSiteKind.Structure, HungerChoiceKind.Intel);
                case "I-026": return new HungerRequestSpec(HungerRequestKind.SimpleMeal, HungerIntelSiteKind.Settlement, HungerChoiceKind.Intel);
                case "I-027": return new HungerRequestSpec(HungerRequestKind.HerbalMedicine, HungerIntelSiteKind.Settlement, HungerChoiceKind.Intel);
                case "I-028": return new HungerRequestSpec(HungerRequestKind.Silver, HungerIntelSiteKind.Settlement, HungerChoiceKind.Intel);
                case "I-002":
                case "I-003":
                case "I-037": return new HungerRequestSpec(HungerRequestKind.None, HungerIntelSiteKind.None, HungerChoiceKind.Abandoned);
                case "I-011": return new HungerRequestSpec(HungerRequestKind.None, HungerIntelSiteKind.None, HungerChoiceKind.Refugees);
                case "I-013": return new HungerRequestSpec(HungerRequestKind.Baby, HungerIntelSiteKind.None, HungerChoiceKind.ChildExchange);
                case "I-032":
                case "I-046": return new HungerRequestSpec(HungerRequestKind.None, HungerIntelSiteKind.None, HungerChoiceKind.Airdrop);
                case "I-033":
                case "I-047": return new HungerRequestSpec(HungerRequestKind.None, HungerIntelSiteKind.None, HungerChoiceKind.Kinship);
                default: return new HungerRequestSpec(HungerRequestKind.None, HungerIntelSiteKind.None, HungerChoiceKind.None);
            }
        }

        internal static bool UsesRequest(string displayId)
        {
            HungerRequestSpec spec = SpecFor(displayId);
            return spec.Kind != HungerRequestKind.None;
        }

        internal static bool UsesChoice(string displayId)
        {
            return SpecFor(displayId).Choice != HungerChoiceKind.None || OffersVisitorControl(displayId);
        }

        internal static bool OffersVisitorControl(string displayId)
        {
            switch (displayId)
            {
                case "I-004":
                case "I-005":
                case "I-006":
                case "I-007":
                case "I-008":
                case "I-009":
                case "I-010":
                case "I-014":
                case "I-029":
                case "I-030":
                case "I-031":
                case "I-036":
                case "I-039":
                case "I-040":
                case "I-041":
                case "I-042":
                case "I-043":
                case "I-044":
                case "I-045":
                    return true;
                default:
                    return false;
            }
        }

        internal static int Amount(HungerRequestKind kind, float wealth, int roll)
        {
            switch (kind)
            {
                case HungerRequestKind.SimpleMeal:
                    return Clamp(6 + Round(wealth / 12000f) + ClampRoll(roll, 0, 4), MinSimple, MaxSimple);
                case HungerRequestKind.FineMeal:
                    return Clamp(4 + Round(wealth / 18000f) + ClampRoll(roll, 0, 3), MinFine, MaxFine);
                case HungerRequestKind.Medicine:
                    return Clamp(2 + Round(wealth / 25000f) + ClampRoll(roll, 0, 2), MinMedicine, MaxMedicine);
                case HungerRequestKind.Silver:
                    return Clamp(80 + Round(wealth / 40f) + ClampRoll(roll, 0, 120), MinSilver, MaxSilver);
                case HungerRequestKind.HerbalMedicine:
                    return Clamp(3 + Round(wealth / 22000f) + ClampRoll(roll, 0, 3), MinHerbal, MaxHerbal);
                case HungerRequestKind.Baby:
                    return 1;
                default:
                    return 0;
            }
        }

        internal static string ThingDefName(HungerRequestKind kind)
        {
            switch (kind)
            {
                case HungerRequestKind.SimpleMeal: return "MealSimple";
                case HungerRequestKind.FineMeal: return "MealFine";
                case HungerRequestKind.Medicine: return "MedicineIndustrial";
                case HungerRequestKind.HerbalMedicine: return "MedicineHerbal";
                case HungerRequestKind.Silver: return "Silver";
                default: return null;
            }
        }

        internal static string SitePartDefName(HungerIntelSiteKind kind)
        {
            switch (kind)
            {
                case HungerIntelSiteKind.Treasure: return "ItemStash";
                case HungerIntelSiteKind.Structure: return "Outpost";
                case HungerIntelSiteKind.Settlement: return "BanditCamp";
                default: return null;
            }
        }

        internal static bool CanDeliver(HungerRequestKind kind, int stock, int amount, bool hasCaptiveBaby)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (kind == HungerRequestKind.Baby)
            {
                return hasCaptiveBaby;
            }

            return stock >= amount;
        }

        internal static HungerChoiceAction Settle(
            HungerChoiceAction requested,
            bool enabled,
            bool alreadySettled,
            bool canDeliver)
        {
            if (!enabled || alreadySettled || requested == HungerChoiceAction.None)
            {
                return HungerChoiceAction.None;
            }

            if (requested == HungerChoiceAction.Deliver && !canDeliver)
            {
                return HungerChoiceAction.None;
            }

            return requested;
        }

        internal static bool RemovesStock(HungerChoiceAction action, HungerRequestKind kind)
        {
            return action == HungerChoiceAction.Deliver && kind != HungerRequestKind.None && kind != HungerRequestKind.Baby;
        }

        internal static bool CreatesSite(HungerChoiceAction action, HungerIntelSiteKind site)
        {
            return action == HungerChoiceAction.Deliver && site != HungerIntelSiteKind.None;
        }

        internal static bool Joins(HungerChoiceAction action)
        {
            return action == HungerChoiceAction.Join;
        }

        internal static bool Hires(HungerChoiceAction action)
        {
            return action == HungerChoiceAction.Hire;
        }

        internal static bool Leaves(HungerChoiceAction action)
        {
            return action == HungerChoiceAction.Reject || action == HungerChoiceAction.Timeout;
        }

        static int Round(float value)
        {
            if (value <= 0f)
            {
                return 0;
            }

            return (int)(value + 0.5f);
        }

        static int ClampRoll(int roll, int min, int max)
        {
            if (roll < min)
            {
                return min;
            }

            return roll > max ? max : roll;
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }

    public sealed class HungerChoiceRecord : IExposable
    {
        internal int Id;
        internal string DisplayId = string.Empty;
        internal int MapId;
        internal int BatchId;
        internal HungerRequestKind Kind;
        internal HungerIntelSiteKind Site;
        internal HungerChoiceKind Choice;
        internal int Amount;
        internal int ExpireTick = -1;
        internal HungerChoiceAction Settled;
        internal List<int> PawnLoadIds = new List<int>();

        internal bool Open => Settled == HungerChoiceAction.None;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Id, "id", 0);
            Scribe_Values.Look(ref DisplayId, "displayId");
            Scribe_Values.Look(ref MapId, "mapId", 0);
            Scribe_Values.Look(ref BatchId, "batchId", 0);
            Scribe_Values.Look(ref Kind, "kind", HungerRequestKind.None);
            Scribe_Values.Look(ref Site, "site", HungerIntelSiteKind.None);
            Scribe_Values.Look(ref Choice, "choice", HungerChoiceKind.None);
            Scribe_Values.Look(ref Amount, "amount", 0);
            Scribe_Values.Look(ref ExpireTick, "expireTick", -1);
            Scribe_Values.Look(ref Settled, "settled", HungerChoiceAction.None);
            Scribe_Collections.Look(ref PawnLoadIds, "pawnLoadIds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                DisplayId = DisplayId ?? string.Empty;
                PawnLoadIds = PawnLoadIds ?? new List<int>();
            }
        }
    }
}
