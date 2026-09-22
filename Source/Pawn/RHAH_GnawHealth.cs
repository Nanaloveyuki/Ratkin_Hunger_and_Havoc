using System.Collections.Generic;

namespace HungerAndHavoc.Pawn
{
    internal readonly struct GnawBite
    {
        internal GnawBite(bool wall, float nutrition, float damage, float severity, float toxic)
        {
            Wall = wall;
            Nutrition = nutrition;
            Damage = damage;
            Severity = severity;
            Toxic = toxic;
        }

        internal bool Wall { get; }
        internal float Nutrition { get; }
        internal float Damage { get; }
        internal float Severity { get; }
        internal float Toxic { get; }
    }

    internal static class RHAH_GnawHealth
    {
        internal const float BarkNutrition = 0.2f;
        internal const float BarkDamage = 2f;
        internal const float BarkSeverity = 0.2f;
        internal const float BarkToxic = 0.08f;
        internal const float WallNutrition = 0.5f;
        internal const float WallDamage = 5f;
        internal const float WallSeverity = 0.2f;
        internal const float OverSeverity = 0.3f;
        internal const int OverCount = 5;
        internal const int DurationTicks = 90000;

        internal static GnawBite ForTarget(bool wall)
        {
            if (wall)
            {
                return new GnawBite(true, WallNutrition, WallDamage, WallSeverity, 0f);
            }

            return new GnawBite(false, BarkNutrition, BarkDamage, BarkSeverity, BarkToxic);
        }

        internal static int NextWallCount(Dictionary<int, int> counts, int pawnLoadId)
        {
            if (counts == null || pawnLoadId <= 0)
            {
                return 0;
            }

            int current;
            counts.TryGetValue(pawnLoadId, out current);
            int next = current + 1;
            counts[pawnLoadId] = next;
            return next;
        }

        internal static void Forget(Dictionary<int, int> counts, int pawnLoadId)
        {
            if (counts == null || pawnLoadId <= 0)
            {
                return;
            }

            counts.Remove(pawnLoadId);
        }

        internal static bool IsOvergnaw(int count)
        {
            return count >= OverCount;
        }

        internal static float ClampFood(float current, float max)
        {
            if (float.IsNaN(current) || float.IsInfinity(current) || float.IsNaN(max) || float.IsInfinity(max))
            {
                return current;
            }

            return current > max ? max : current;
        }
    }
}
