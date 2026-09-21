using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class ApiSurfaceTests
    {
        static readonly string[] Whitelist =
        {
            "HungerAndHavocApi",
            "IHungerPawn",
            "HungerPawnSnapshot",
            "IHungerPawnBehavior",
            "HungerPawnBehaviors",
            "HungerPawnSeed",
            "HungerBehaviorGate",
            "HungerPawnRole",
            "HungerLifecycle",
            "HungerReleaseReason",
            "HungerAttitude"
        };

        static readonly string[] ForbiddenNames =
        {
            "CompHungerPawn",
            "CompProperties_HungerPawn",
            "Hediff_HungerMark",
            "HungerRace",
            "HungerRaceExtension"
        };

        static ApiSurfaceTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void ExportedTypesMatchWhitelist()
        {
            Assembly api = typeof(HungerAndHavocApi).Assembly;
            Assert.Equal("HungerAndHavoc.Api", api.GetName().Name);

            HashSet<string> actual = new HashSet<string>(
                api.GetExportedTypes().Select(type => type.Name),
                StringComparer.Ordinal);
            HashSet<string> expected = new HashSet<string>(Whitelist, StringComparer.Ordinal);

            Assert.True(
                expected.SetEquals(actual),
                "missing=[" + string.Join(", ", expected.Except(actual).OrderBy(name => name)) +
                "] extra=[" + string.Join(", ", actual.Except(expected).OrderBy(name => name)) + "]");
            Assert.Contains("HungerPawnSnapshot", actual);
            Assert.True(typeof(HungerPawnSnapshot).IsSealed);
            Assert.False(typeof(IHungerApiHost).IsPublic);
        }

        [Fact]
        public void PublicSignaturesDoNotExposeCompHediffOrHungerRace()
        {
            Assembly api = typeof(HungerAndHavocApi).Assembly;
            List<string> leaks = new List<string>();
            foreach (Type type in api.GetExportedTypes())
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                foreach (MethodInfo method in type.GetMethods(flags))
                {
                    if (method.DeclaringType == typeof(object))
                    {
                        continue;
                    }

                    CollectForbidden(leaks, type, method.Name, method.ReturnType);
                    foreach (ParameterInfo parameter in method.GetParameters())
                    {
                        CollectForbidden(leaks, type, method.Name, parameter.ParameterType);
                    }
                }

                foreach (ConstructorInfo ctor in type.GetConstructors(flags))
                {
                    foreach (ParameterInfo parameter in ctor.GetParameters())
                    {
                        CollectForbidden(leaks, type, ctor.Name, parameter.ParameterType);
                    }
                }

                foreach (EventInfo eventInfo in type.GetEvents(flags))
                {
                    CollectForbidden(leaks, type, eventInfo.Name, eventInfo.EventHandlerType);
                }

                foreach (FieldInfo field in type.GetFields(flags))
                {
                    CollectForbidden(leaks, type, field.Name, field.FieldType);
                }
            }

            Assert.True(leaks.Count == 0, string.Join("; ", leaks));
        }

        [Fact]
        public void AssemblyReferencesFlowApiToImplementationOnly()
        {
            Assembly api = typeof(HungerAndHavocApi).Assembly;
            HashSet<string> apiRefs = Names(api);
            Assert.DoesNotContain("HungerAndHavoc", apiRefs);
            Assert.DoesNotContain("HungerAndHavoc.Tests", apiRefs);
            Assert.DoesNotContain("0Harmony", apiRefs);
            Assert.DoesNotContain("HarmonyLib", apiRefs);
            Assert.DoesNotContain("HungerAndHavocGuard", apiRefs);

            Assembly implementation = Assembly.Load("HungerAndHavoc");
            HashSet<string> implRefs = Names(implementation);
            Assert.Contains("HungerAndHavoc.Api", implRefs);
            Assert.DoesNotContain("HungerAndHavoc.Tests", implRefs);
            Assert.False(typeof(IHungerPawn).IsAssignableFrom(typeof(CompHungerPawn)));
        }

        static HashSet<string> Names(Assembly assembly)
        {
            return new HashSet<string>(
                assembly.GetReferencedAssemblies().Select(name => name.Name),
                StringComparer.Ordinal);
        }

        static void CollectForbidden(List<string> leaks, Type owner, string member, Type signatureType)
        {
            foreach (Type type in Enumerate(signatureType))
            {
                if (!IsForbidden(type))
                {
                    continue;
                }

                leaks.Add(owner.Name + "." + member + " -> " + type.FullName);
            }
        }

        static bool IsForbidden(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (ForbiddenNames.Contains(type.Name))
            {
                return true;
            }

            if (type.Name.IndexOf("Hediff", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (type.Name.IndexOf("CompHunger", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (type.Namespace != null &&
                type.Namespace.StartsWith("HungerAndHavoc.", StringComparison.Ordinal) &&
                type.Namespace != "HungerAndHavoc.Api")
            {
                return true;
            }

            return false;
        }

        static IEnumerable<Type> Enumerate(Type type)
        {
            if (type == null)
            {
                yield break;
            }

            yield return type;
            if (type.HasElementType)
            {
                foreach (Type inner in Enumerate(type.GetElementType()))
                {
                    yield return inner;
                }
            }

            if (!type.IsGenericType)
            {
                yield break;
            }

            foreach (Type argument in type.GetGenericArguments())
            {
                foreach (Type inner in Enumerate(argument))
                {
                    yield return inner;
                }
            }
        }
    }
}
