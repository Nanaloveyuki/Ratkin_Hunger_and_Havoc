using HungerAndHavoc.Api;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HungerAndHavoc.Trade
{
    internal static class RHAH_CaravanStay
    {
        internal const int FoodPerChild = 10;
        internal const float TemperatureMargin = 10f;

        internal static bool IsTradeCaravan(string displayId, RHAH_PawnRole role)
        {
            return role == RHAH_PawnRole.Trader || displayId == "I-012" || displayId == "I-038";
        }

        internal static bool ShouldHold(Verse.Pawn pawn, bool ignoreHarshEnvironment, bool ignoreEnclosedSpace)
        {
            if (!IsTradeCaravan(pawn))
            {
                return false;
            }

            return (ignoreHarshEnvironment && CellIsHarsh(pawn)) || (ignoreEnclosedSpace && RoomIsEnclosed(pawn));
        }

        internal static bool IsTradeCaravan(Verse.Pawn pawn)
        {
            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            return snapshot != null && IsTradeCaravan(snapshot.SourceIncidentDisplayId, snapshot.Role);
        }

        static bool CellIsHarsh(Verse.Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return false;
            }

            FloatRange comfort = pawn.ComfortableTemperatureRange();
            return CellIsHarsh(pawn.AmbientTemperature, comfort.min, comfort.max);
        }

        static bool RoomIsEnclosed(Verse.Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return false;
            }

            Room room = pawn.GetRoom();
            return RoomIsEnclosed(room != null, room != null && room.PsychologicallyOutdoors, room != null && room.UsesOutdoorTemperature);
        }

        internal static bool BlocksEnvironmentLeave(bool ignoreHarshEnvironment, bool isTradeCaravan)
        {
            return ignoreHarshEnvironment && isTradeCaravan;
        }

        internal static bool BlocksEnclosedLeave(bool ignoreEnclosedSpace, bool isTradeCaravan)
        {
            return ignoreEnclosedSpace && isTradeCaravan;
        }

        internal static bool IsEnvironmentLeave(bool ignoreHarshEnvironment, bool isTradeCaravan, string messageKey)
        {
            return BlocksEnvironmentLeave(ignoreHarshEnvironment, isTradeCaravan) &&
                (messageKey == "MessageVisitorsDangerousTemperature" ||
                 messageKey == "MessageVisitorsAnomalousWeather" ||
                 messageKey == "MessageVisitorsDangerousConditions");
        }

        internal static bool IsEnclosedLeave(bool ignoreEnclosedSpace, bool isTradeCaravan, string messageKey)
        {
            return BlocksEnclosedLeave(ignoreEnclosedSpace, isTradeCaravan) && messageKey == "MessageVisitorsTrappedLeaving";
        }

        internal static bool CellIsHarsh(float temperature, float comfortMin, float comfortMax)
        {
            if (float.IsNaN(temperature) || float.IsInfinity(temperature) ||
                float.IsNaN(comfortMin) || float.IsInfinity(comfortMin) ||
                float.IsNaN(comfortMax) || float.IsInfinity(comfortMax))
            {
                return false;
            }

            return temperature < comfortMin - TemperatureMargin || temperature > comfortMax + TemperatureMargin;
        }

        internal static bool RoomIsEnclosed(bool hasRoom, bool psychologicallyOutdoors, bool usesOutdoorTemperature)
        {
            return hasRoom && !psychologicallyOutdoors && !usesOutdoorTemperature;
        }

        internal static bool AllowsFoodForChild(bool foodSubstitutionEnabled, RHAH_ChoiceKind choice, bool playerSide, bool foodIsNutrition, int foodCount, int childCount)
        {
            if (!foodSubstitutionEnabled || choice != RHAH_ChoiceKind.ChildExchange || !playerSide || !foodIsNutrition || childCount <= 0)
            {
                return false;
            }

            return foodCount >= childCount * FoodPerChild;
        }

        internal static bool NpcSellsFood(bool isTradeCaravan, bool foodIsNutrition)
        {
            return !(isTradeCaravan && foodIsNutrition);
        }

        internal static string MessageKey(TransitionAction action)
        {
            TransitionAction_Message message = action as TransitionAction_Message;
            return message?.message;
        }
    }
}
