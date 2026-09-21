using System;
using System.IO;
using System.Runtime.CompilerServices;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Pawn.Compat;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_PawnCompatTests
    {
        static RHAH_PawnCompatTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void EmptyHooksAbstain()
        {
            HungerPawnSnapshot snapshot = DummySnapshot(HungerPawnRole.BeggarChild);
            RHAH_LeashCompat leash = new RHAH_LeashCompat();
            RHAH_ToddlerCompat toddler = new RHAH_ToddlerCompat();
            RHAH_PrisonerCompat prisoner = new RHAH_PrisonerCompat();

            Assert.Null(leash.Allows(null, snapshot, HungerBehaviorGate.Leash));
            Assert.Null(leash.ShouldReleaseToColony(null, snapshot, HungerReleaseReason.Recruited));
            Assert.Null(toddler.Allows(null, snapshot, HungerBehaviorGate.Carry));
            Assert.Null(toddler.ShouldReleaseToColony(null, snapshot, HungerReleaseReason.ModRequest));
            Assert.Null(prisoner.Allows(null, snapshot, HungerBehaviorGate.Imprison));
            Assert.Null(prisoner.ShouldReleaseToColony(null, snapshot, HungerReleaseReason.Imprisoned));
        }

        [Fact]
        public void TryQueryLastRegisteredWinsAndSkipsNull()
        {
            HungerPawnBehaviors.ResetForTests();
            RHAH_PawnCompat.ResetForTests();
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                Assert.Null(RHAH_PawnCompat.TryQuery(null, HungerBehaviorGate.Leash));

                RHAH_PawnCompat.Register(null, new StubHook { AllowsResult = false });
                RHAH_PawnCompat.Register("test.first", new StubHook { AllowsResult = true });
                RHAH_PawnCompat.Register("test.later", new StubHook { AllowsResult = false });
                RHAH_PawnCompat.Register("test.skip", new StubHook { AllowsResult = null });

                Assert.False(RHAH_PawnCompat.TryQuery(null, HungerBehaviorGate.Leash));
            }
            finally
            {
                RHAH_PawnCompat.ResetForTests();
                HungerPawnBehaviors.ResetForTests();
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void DenyLeashDoesNotChangeDefaultsUnlessAdapterWired()
        {
            HungerPawnBehaviors.ResetForTests();
            RHAH_PawnCompat.ResetForTests();
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                HungerPawnState child = ChildVisitor();
                Assert.True(HungerPawnDefaults.Allows(child.ToSnapshot(), HungerBehaviorGate.Leash));
                Assert.True(child.Allows(HungerBehaviorGate.Leash));
                Assert.False(HungerAndHavocApi.Allows(null, HungerBehaviorGate.Leash));

                RHAH_PawnCompat.Register("test.deny.leash", new DenyLeashHook());
                Assert.False(RHAH_PawnCompat.TryQuery(null, HungerBehaviorGate.Leash));
                Assert.True(child.Allows(HungerBehaviorGate.Leash));
                Assert.Null(HungerPawnBehaviors.Query(handler =>
                    handler.Allows(null, child.ToSnapshot(), HungerBehaviorGate.Leash)));
                Assert.False(HungerAndHavocApi.Allows(null, HungerBehaviorGate.Leash));
            }
            finally
            {
                RHAH_PawnCompat.ResetForTests();
                HungerPawnBehaviors.ResetForTests();
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void WiredAdapterDenyLeashWinsAfterRegister()
        {
            HungerPawnBehaviors.ResetForTests();
            RHAH_PawnCompat.ResetForTests();
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                HungerPawnState child = ChildVisitor();
                RHAH_PawnCompat.EnsureAdapterRegistered();
                Assert.True(child.Allows(HungerBehaviorGate.Leash));
                Assert.Null(HungerPawnBehaviors.Query(handler =>
                    handler.Allows(null, child.ToSnapshot(), HungerBehaviorGate.Leash)));

                RHAH_PawnCompat.Register("test.deny.leash", new DenyLeashHook());
                Assert.False(child.Allows(HungerBehaviorGate.Leash));
                Assert.False(HungerPawnBehaviors.Query(handler =>
                    handler.Allows(null, child.ToSnapshot(), HungerBehaviorGate.Leash)));
                Assert.False(HungerAndHavocApi.Allows(null, HungerBehaviorGate.Leash));
            }
            finally
            {
                RHAH_PawnCompat.ResetForTests();
                HungerPawnBehaviors.ResetForTests();
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void StartupRegistersAdapterAndStaysPublic()
        {
            HungerPawnBehaviors.ResetForTests();
            RHAH_PawnCompat.ResetForTests();
            try
            {
                string source = File.ReadAllText(CompatPath());
                Assert.Contains("[StaticConstructorOnStartup]", source);
                Assert.Contains("public static class RHAH_PawnCompatStartup", source);
                Assert.Contains("EnsureAdapterRegistered", source);
                Assert.DoesNotContain("Harmony", source);

                Type startup = typeof(RHAH_PawnCompatStartup);
                Assert.True(startup.IsPublic);
                Assert.Equal("HungerAndHavoc.Pawn.Compat", startup.Namespace);
                Assert.True(Attribute.IsDefined(startup, typeof(StaticConstructorOnStartup)));
            }
            finally
            {
                RHAH_PawnCompat.ResetForTests();
                HungerPawnBehaviors.ResetForTests();
            }
        }

        [Fact]
        public void ReservedHookTypesStayInternalAndAbstainWhenRegistered()
        {
            HungerPawnBehaviors.ResetForTests();
            RHAH_PawnCompat.ResetForTests();
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                RHAH_PawnCompat.EnsureAdapterRegistered();
                RHAH_PawnCompat.Register("unlimited.lead.your.pet", new RHAH_LeashCompat());
                RHAH_PawnCompat.Register("toddler.mod", new RHAH_ToddlerCompat());
                RHAH_PawnCompat.Register("prisoner.work.expansion", new RHAH_PrisonerCompat());

                HungerPawnState child = ChildVisitor();
                Assert.True(child.Allows(HungerBehaviorGate.Leash));
                Assert.True(child.Allows(HungerBehaviorGate.Carry));
                Assert.True(child.Allows(HungerBehaviorGate.Imprison));
                Assert.Null(RHAH_PawnCompat.TryQuery(null, HungerBehaviorGate.Leash));
                Assert.False(typeof(RHAH_LeashCompat).IsPublic);
                Assert.False(typeof(RHAH_ToddlerCompat).IsPublic);
                Assert.False(typeof(RHAH_PrisonerCompat).IsPublic);
            }
            finally
            {
                RHAH_PawnCompat.ResetForTests();
                HungerPawnBehaviors.ResetForTests();
                HungerAndHavocApi.Bind(null);
            }
        }

        static HungerPawnState ChildVisitor()
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                sourceIncidentDisplayId: "I-001",
                role: HungerPawnRole.BeggarChild,
                lifecycle: HungerLifecycle.SeekingFood,
                attitudeAtArrival: HungerAttitude.Neutral));
            return state;
        }

        static HungerPawnSnapshot DummySnapshot(HungerPawnRole role)
        {
            return new HungerPawnSnapshot(
                "I-001",
                0,
                0,
                role,
                HungerLifecycle.SeekingFood,
                false,
                -1,
                false,
                HungerAttitude.Neutral,
                0,
                null);
        }

        static string CompatPath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Pawn", "Compat", "RHAH_PawnCompat.cs"));
        }

        sealed class DenyLeashHook : RHAH_PawnCompatHook
        {
            public bool? Allows(Verse.Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate)
            {
                if (gate == HungerBehaviorGate.Leash)
                {
                    return false;
                }

                return null;
            }

            public bool? ShouldReleaseToColony(Verse.Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
            {
                return null;
            }
        }

        sealed class StubHook : RHAH_PawnCompatHook
        {
            public bool? AllowsResult;

            public bool? Allows(Verse.Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate)
            {
                return AllowsResult;
            }

            public bool? ShouldReleaseToColony(Verse.Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
            {
                return null;
            }
        }
    }
}
