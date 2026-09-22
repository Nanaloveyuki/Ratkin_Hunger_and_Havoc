using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Generation
{
    internal sealed class HungerXenotypeRegistration
    {
        internal HungerXenotypeRegistration(string defName, float suggestedWeight, bool builtin)
        {
            DefName = defName;
            SuggestedWeight = HungerXenotypeWeightTable.Clamp(suggestedWeight);
            Builtin = builtin;
        }

        internal string DefName { get; }
        internal float SuggestedWeight { get; }
        internal bool Builtin { get; }
    }

    internal static class HungerGeneCatalog
    {
        internal const string DefaultXenotypeDefName = "RK_XenoType_Ratkin";
        internal const string FallbackXenotypeDefName = "RHAH_Xenotype_Ratkin";
        internal const string GenePrefix = "RHAH_";

        static readonly List<HungerXenotypeRegistration> registrations = new List<HungerXenotypeRegistration>();

        static HungerGeneCatalog()
        {
            RegisterXenotype(DefaultXenotypeDefName, HungerXenotypeWeightTable.DefaultBuiltinWeight, true);
            RegisterXenotype(FallbackXenotypeDefName, HungerXenotypeWeightTable.DefaultExternalWeight, false);
        }

        internal static void RegisterXenotype(string defName, float suggestedWeight, bool builtin)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            for (int i = 0; i < registrations.Count; i++)
            {
                if (string.Equals(registrations[i].DefName, defName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!builtin && string.Equals(registrations[i].DefName, DefaultXenotypeDefName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    registrations[i] = new HungerXenotypeRegistration(registrations[i].DefName, suggestedWeight, builtin);
                    return;
                }
            }

            registrations.Add(new HungerXenotypeRegistration(defName, suggestedWeight, builtin));
        }

        internal static void ResetForTests()
        {
            registrations.Clear();
            RegisterXenotype(DefaultXenotypeDefName, HungerXenotypeWeightTable.DefaultBuiltinWeight, true);
            RegisterXenotype(FallbackXenotypeDefName, HungerXenotypeWeightTable.DefaultExternalWeight, false);
        }

        internal static IReadOnlyList<HungerXenotypeRegistration> Registrations => registrations;

        internal static bool IsOwnedGene(GeneDef gene)
        {
            return gene != null &&
                !string.IsNullOrEmpty(gene.defName) &&
                gene.defName.StartsWith(GenePrefix, StringComparison.Ordinal);
        }

        internal static bool IsRegistered(string defName)
        {
            return Find(defName) != null;
        }

        internal static float SuggestedWeight(string defName)
        {
            HungerXenotypeRegistration registration = Find(defName);
            return registration == null
                ? HungerXenotypeWeightTable.DefaultExternalWeight
                : registration.SuggestedWeight;
        }

        internal static bool IsBuiltin(string defName)
        {
            HungerXenotypeRegistration registration = Find(defName);
            return registration != null && registration.Builtin;
        }

        static HungerXenotypeRegistration Find(string defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return null;
            }

            for (int i = 0; i < registrations.Count; i++)
            {
                if (string.Equals(registrations[i].DefName, defName, StringComparison.OrdinalIgnoreCase))
                {
                    return registrations[i];
                }
            }

            return null;
        }
    }
}
