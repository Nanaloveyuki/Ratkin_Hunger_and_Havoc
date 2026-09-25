using RimWorld;

using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Generation
{
    public class RHAH_GenerationExtension : DefModExtension
    {
        public bool allowRefugeeApparel;
    }

    internal static class RHAH_ApparelPolicy
    {
        internal const float AwfulShare = 0.35f;
        internal const float PoorShare = 0.50f;
        internal const float MinDurability = 0.10f;
        internal const float MaxDurability = 0.60f;
        internal const float CorpseChance = 0.25f;
        internal const float ClothWeight = 0.65f;
        internal const float LeatherWeight = 0.35f;
        internal const int MaxPieces = 4;

        const string RatkinPackageId = "solaris.ratkinracemod";
        const string CorePackageId = "ludeon.rimworld";
        const string ClothDefName = "Cloth";
        const string LeatherDefName = "Humanleather";

        static readonly string[] YoungNames =
        {
            "Apparel_BabyOnesie",
            "Apparel_WarmerHat",
            "Apparel_SunHat",
            "Apparel_KidTribal"
        };

        static readonly string[] BabyNames =
        {
            "Apparel_BabyOnesie",
            "Apparel_WarmerHat",
            "Apparel_SunHat"
        };

        static readonly string[] ChildNames =
        {
            "Apparel_WarmerHat",
            "Apparel_SunHat",
            "Apparel_KidTribal"
        };

        internal static bool AllowsSource(string packageId, bool official)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                return false;
            }

            string id = packageId.ToLowerInvariant();
            if (id == RatkinPackageId)
            {
                return true;
            }

            return official && (id == CorePackageId || id.StartsWith(CorePackageId + "."));
        }

        internal static bool AllowsApparel(string defName, bool isApparel, bool madeFromStuff, bool countsAsClothing, string packageId, bool official, bool allowRefugeeApparel, int techLevel, bool enabled)
        {
            if (!enabled)
            {
                return false;
            }

            if (string.IsNullOrEmpty(defName) || !isApparel || !madeFromStuff || !countsAsClothing)
            {
                return false;
            }

            if (allowRefugeeApparel)
            {
                return true;
            }

            if (IsYoung(defName))
            {
                return true;
            }

            return AllowsSource(packageId, official) && techLevel <= (int)TechLevel.Medieval;
        }
        internal static bool IsYoung(string defName)
        {
            return IndexOf(YoungNames, defName) >= 0;
        }

        internal static bool FitsStage(string defName, int stage)
        {
            if (stage == 0)
            {
                return IndexOf(BabyNames, defName) >= 0;
            }

            if (stage == 1)
            {
                return IndexOf(ChildNames, defName) >= 0;
            }

            return !IsYoung(defName);
        }

        internal static int Quality(float roll)
        {
            float safe = roll < 0f || float.IsNaN(roll) ? 0f : roll > 1f ? 1f : roll;
            if (safe < AwfulShare)
            {
                return 0;
            }

            return safe < AwfulShare + PoorShare ? 1 : 2;
        }

        internal static int HitPoints(int maxHitPoints, float fraction)
        {
            if (maxHitPoints <= 1)
            {
                return maxHitPoints;
            }

            float safe = fraction < MinDurability || float.IsNaN(fraction) ? MinDurability : fraction > MaxDurability ? MaxDurability : fraction;
            int points = (int)System.Math.Round(maxHitPoints * safe);
            if (points < 1)
            {
                return 1;
            }

            return points > maxHitPoints ? maxHitPoints : points;
        }

        internal static int PieceCount(int stage, float roll)
        {
            if (stage == 0)
            {
                return Weighted(roll, 0.30f, 0.50f, 0.18f, 0.02f);
            }

            if (stage == 1)
            {
                return Weighted(roll, 0.16f, 0.34f, 0.30f, 0.16f, 0.04f);
            }

            int adult = Weighted(roll, 0.02f, 0.16f, 0.34f, 0.30f, 0.18f);
            return adult < 1 ? 1 : adult > MaxPieces ? MaxPieces : adult;
        }

        internal static string PreferredStuff(IList<string> allowed, float roll)
        {
            bool cloth = false;
            bool leather = false;
            if (allowed != null)
            {
                for (int i = 0; i < allowed.Count; i++)
                {
                    cloth |= allowed[i] == ClothDefName;
                    leather |= allowed[i] == LeatherDefName;
                }
            }

            if (cloth && leather)
            {
                float safe = roll < 0f || float.IsNaN(roll) ? 0f : roll > 1f ? 1f : roll;
                return safe < ClothWeight ? ClothDefName : LeatherDefName;
            }

            if (cloth)
            {
                return ClothDefName;
            }

            return leather ? LeatherDefName : null;
        }

        static int Weighted(float roll, params float[] weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                total += weights[i] < 0f ? 0f : weights[i];
            }

            if (total <= 0f)
            {
                return 0;
            }

            float safe = roll < 0f || float.IsNaN(roll) ? 0f : roll > 1f ? 1f : roll;
            float cursor = safe * total;
            float sum = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] < 0f ? 0f : weights[i];
                if (cursor <= sum)
                {
                    return i;
                }
            }

            return weights.Length - 1;
        }

        static int IndexOf(string[] names, string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return -1;
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == defName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
