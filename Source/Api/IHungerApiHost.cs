using System;
using Verse;

namespace HungerAndHavoc.Api
{
    // 实现程序集启动时绑定 其它模组不得实现此接口
    internal interface IHungerApiHost
    {
        IHungerPawn Get(Pawn pawn);

        bool IsRatkin(Pawn pawn);

        IHungerPawn TryMarkOrigin(Pawn pawn, HungerPawnSeed seed);

        bool ReleaseToColony(Pawn pawn, HungerReleaseReason reason);

        void SetLifecycle(Pawn pawn, HungerLifecycle lifecycle);

        bool Allows(Pawn pawn, HungerBehaviorGate gate);

        void SetGate(Pawn pawn, HungerBehaviorGate gate, bool? allowed);

        void SetExtra(Pawn pawn, string key, string value);

        bool TryGetExtra(Pawn pawn, string key, out string value);

        void RegisterRatkinMatcher(Func<ThingDef, bool> matcher);
    }
}
