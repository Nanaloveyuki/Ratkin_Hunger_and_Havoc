using System.Collections.Generic;
using Verse;
using VersePawn = Verse.Pawn;
namespace HungerAndHavoc.Generation
{
    internal enum HungerPawnCreationFailure
    {
        None = 0,
        InvalidRequest = 1,
        NoMap = 2,
        NoPawnKind = 3,
        PopulationLimit = 4,
        DuplicateBatch = 5,
        GenerationFailed = 6,
        MarkingFailed = 7,
        RelationshipFailed = 8
    }

    internal sealed class HungerPawnCreationResult
    {
        public List<VersePawn> Pawns { get; } = new List<VersePawn>();
        public HungerPawnCreationFailure Failure { get; private set; }
        public bool Succeeded => Failure == HungerPawnCreationFailure.None && Pawns.Count > 0;

        public static HungerPawnCreationResult Success(List<VersePawn> pawns)
        {
            HungerPawnCreationResult result = new HungerPawnCreationResult();
            result.Pawns.AddRange(pawns);
            return result;
        }

        public static HungerPawnCreationResult Failed(HungerPawnCreationFailure failure)
        {
            return new HungerPawnCreationResult { Failure = failure };
        }
    }
}
