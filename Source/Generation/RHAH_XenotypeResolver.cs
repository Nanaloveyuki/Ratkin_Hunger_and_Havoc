using System.Collections.Generic;
using RimWorld;
using Verse;
using VersePawn = Verse.Pawn;

namespace HungerAndHavoc.Generation
{
    internal static class RHAH_XenotypeResolver
    {
        internal static XenotypeDef Resolve(RHAH_PawnProfile profile)
        {
            RHAH_PawnProfile resolved = profile ?? new RHAH_PawnProfile();
            if (resolved.UseExplicitXenotype)
            {
                if (resolved.Xenotype != null)
                {
                    return resolved.Xenotype;
                }

                return Load(resolved.XenotypeDefName);
            }

            if (!RHAH_GenerationOptimizer.Enabled || !ModsConfig.BiotechActive)
            {
                return null;
            }

            string chosen = RHAH_XenotypeWeightTable.Choose(EnabledNames(), EnabledWeights(), Rand.Value * EnabledTotal());
            XenotypeDef weighted = Load(chosen);
            if (weighted != null)
            {
                return weighted;
            }

            XenotypeDef primary = Load(RHAH_GeneCatalog.DefaultXenotypeDefName);
            if (primary != null)
            {
                return primary;
            }

            return Load(RHAH_GeneCatalog.FallbackXenotypeDefName);
        }

        internal static void ApplyEnabledGenes(VersePawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn?.genes == null)
            {
                return;
            }

            Core.RHAH_Settings settings = Core.RHAH_Mod.Settings;
            if (settings == null)
            {
                return;
            }

            List<string> names = settings.EnabledGeneNames();
            for (int i = 0; i < names.Count; i++)
            {
                GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(names[i]);
                if (!RHAH_GeneCatalog.IsOwnedGene(gene) || pawn.genes.HasActiveGene(gene) || Conflicts(pawn, gene))
                {
                    continue;
                }

                pawn.genes.AddGene(gene, false);
            }
        }

        internal static List<XenotypeDef> LoadedCandidates(Core.RHAH_Settings settings)
        {
            List<XenotypeDef> loaded = new List<XenotypeDef>();
            if (!ModsConfig.BiotechActive || settings == null)
            {
                return loaded;
            }

            AddKnown(loaded, settings);
            AddJoined(loaded, settings);
            return loaded;
        }

        internal static List<XenotypeDef> AvailableToJoin(Core.RHAH_Settings settings)
        {
            List<XenotypeDef> available = new List<XenotypeDef>();
            if (!ModsConfig.BiotechActive || settings == null)
            {
                return available;
            }

            List<XenotypeDef> all = DefDatabase<XenotypeDef>.AllDefsListForReading;
            List<XenotypeDef> loaded = LoadedCandidates(settings);
            for (int i = 0; i < all.Count; i++)
            {
                XenotypeDef xenotype = all[i];
                if (xenotype == null || IsFallback(xenotype.defName) || Contains(loaded, xenotype.defName))
                {
                    continue;
                }

                if (RHAH_GeneCatalog.IsRegistered(xenotype.defName) || LooksRatkin(xenotype))
                {
                    available.Add(xenotype);
                }
            }

            return available;
        }

        static bool LooksRatkin(XenotypeDef xenotype)
        {
            string name = xenotype.defName ?? string.Empty;
            string label = xenotype.label ?? string.Empty;
            return name.IndexOf("ratkin", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("ratkin", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("鼠族", System.StringComparison.Ordinal) >= 0;
        }
        internal static List<GeneDef> LoadedOwnedGenes()
        {
            List<GeneDef> genes = new List<GeneDef>();
            if (!ModsConfig.BiotechActive)
            {
                return genes;
            }

            List<GeneDef> all = DefDatabase<GeneDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                if (RHAH_GeneCatalog.IsOwnedGene(all[i]))
                {
                    genes.Add(all[i]);
                }
            }

            return genes;
        }

        static void AddKnown(List<XenotypeDef> loaded, Core.RHAH_Settings settings)
        {
            IReadOnlyList<RHAH_XenotypeRegistration> registrations = RHAH_GeneCatalog.Registrations;
            for (int i = 0; i < registrations.Count; i++)
            {
                XenotypeDef xenotype = Load(registrations[i].DefName);
                if (xenotype == null || IsFallback(xenotype.defName))
                {
                    continue;
                }

                if (registrations[i].Builtin || settings.IsXenotypeEnabled(xenotype.defName))
                {
                    loaded.Add(xenotype);
                }
            }
        }

        static void AddJoined(List<XenotypeDef> loaded, Core.RHAH_Settings settings)
        {
            List<string> joined = settings.EnabledXenotypeNames();
            for (int i = 0; i < joined.Count; i++)
            {
                if (Contains(loaded, joined[i]) || IsFallback(joined[i]))
                {
                    continue;
                }

                XenotypeDef xenotype = Load(joined[i]);
                if (xenotype != null)
                {
                    loaded.Add(xenotype);
                }
            }
        }

        static bool IsFallback(string defName)
        {
            return string.Equals(defName, RHAH_GeneCatalog.FallbackXenotypeDefName, System.StringComparison.OrdinalIgnoreCase);
        }
        static bool Contains(List<XenotypeDef> loaded, string defName)
        {
            for (int i = 0; i < loaded.Count; i++)
            {
                if (loaded[i].defName == defName)
                {
                    return true;
                }
            }

            return false;
        }

        static List<string> EnabledNames()
        {
            List<string> names = new List<string>();
            List<XenotypeDef> loaded = LoadedCandidates(Core.RHAH_Mod.Settings);
            for (int i = 0; i < loaded.Count; i++)
            {
                names.Add(loaded[i].defName);
            }

            return names;
        }

        static List<float> EnabledWeights()
        {
            Core.RHAH_Settings settings = Core.RHAH_Mod.Settings;
            List<XenotypeDef> loaded = LoadedCandidates(settings);
            List<float> weights = new List<float>(loaded.Count);
            for (int i = 0; i < loaded.Count; i++)
            {
                weights.Add(settings.XenotypeWeight(loaded[i].defName));
            }

            return weights;
        }

        static float EnabledTotal()
        {
            List<float> weights = EnabledWeights();
            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] > 0f)
                {
                    total += weights[i];
                }
            }

            return total;
        }

        static XenotypeDef Load(string defName)
        {
            if (string.IsNullOrEmpty(defName) || !ModsConfig.BiotechActive)
            {
                return null;
            }

            return DefDatabase<XenotypeDef>.GetNamedSilentFail(defName);
        }

        static bool Conflicts(VersePawn pawn, GeneDef gene)
        {
            if (gene.exclusionTags == null)
            {
                return false;
            }

            List<Gene> current = pawn.genes.GenesListForReading;
            for (int i = 0; i < current.Count; i++)
            {
                GeneDef other = current[i]?.def;
                if (other != null && gene.ConflictsWith(other))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
