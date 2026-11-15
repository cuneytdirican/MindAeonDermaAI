using MindAeonDermaAI.Analysis.Models;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Generates a deterministic, literature-inspired synthetic reference cohort and ranks the subject's
/// biological quality against it as a percentile. The cohort spans a young→aged continuum across the
/// canonical skin-aging profiles (young-healthy, age-matched, photoaged, smoker-associated,
/// hydration-deficient, laxity-dominant). It is simulated for demonstration and hypothesis generation
/// only and must never be presented as a validated clinical reference range.
/// </summary>
public static class SyntheticCohort
{
    public const int Size = 150;
    private const int Seed = 20260701; // fixed seed → reproducible cohort

    /// <summary>Named archetype profiles represented in the synthetic distribution (for the report).</summary>
    public static readonly string[] Profiles =
    {
        "young-healthy", "age-matched", "photoaged", "smoker-associated",
        "hydration-deficient", "laxity-dominant"
    };

    public record CohortRanking(int Size, double Percentile);

    /// <summary>
    /// Returns the subject's percentile (0–100) of biological quality against the synthetic cohort:
    /// higher = better-than-more-of-the-cohort. Uses the same per-marker need function as the engine.
    /// </summary>
    public static CohortRanking Rank(IReadOnlyList<ProteinInput> proteins)
    {
        var measured = proteins.Where(p => p.IsMeasured).ToList();
        if (measured.Count == 0) return new CohortRanking(Size, 50);

        double subjectQuality = 100 - measured.Average(p =>
            AnalysisEngine.MarkerNeed(Z(p.Value!.Value, p.YoungMean, p.YoungSd), p.Need));

        var rng = new Random(Seed);
        var qualities = new double[Size];
        for (int i = 0; i < Size; i++)
        {
            // Aging factor spans young (~0) to aged (~2) so the cohort covers the full continuum.
            double aging = rng.NextDouble() * 2.0;
            double needSum = 0;
            foreach (var p in measured)
            {
                double sign = p.Need switch
                {
                    NeedDirection.LowIsNeed => -1,
                    NeedDirection.HighIsNeed => +1,
                    _ => 0
                };
                double jitter = (rng.NextDouble() - 0.5) * p.YoungSd;      // biological variability
                double shift = p.Need == NeedDirection.DysregulationIsNeed
                    ? (rng.NextDouble() * 2 - 1) * aging * p.YoungSd        // deviation either way
                    : sign * aging * p.YoungSd;                            // directional aging drift
                double value = p.YoungMean + shift + jitter;
                needSum += AnalysisEngine.MarkerNeed(Z(value, p.YoungMean, p.YoungSd), p.Need);
            }
            qualities[i] = 100 - needSum / measured.Count;
        }

        int below = qualities.Count(q => q <= subjectQuality);
        double percentile = Math.Round(100.0 * below / Size, 0);
        return new CohortRanking(Size, percentile);
    }

    /// <summary>
    /// Percentile of the subject's biological quality against an <em>age-matched</em> synthetic peer group
    /// (aging factor centered on the subject's age rather than spanning the full continuum).
    /// </summary>
    public static CohortRanking RankStratified(IReadOnlyList<ProteinInput> proteins, int? ageYears)
    {
        var measured = proteins.Where(p => p.IsMeasured).ToList();
        if (measured.Count == 0) return new CohortRanking(Size, 50);

        double subjectQuality = 100 - measured.Average(p =>
            AnalysisEngine.MarkerNeed(Z(p.Value!.Value, p.YoungMean, p.YoungSd), p.Need));

        double aExpected = Math.Clamp(((ageYears ?? 40) - 25) / 17.5 * 2.0, 0, 2); // map age → central aging factor
        var rng = new Random(Seed + 7);
        var qualities = new double[Size];
        for (int i = 0; i < Size; i++)
        {
            double aging = Math.Clamp(aExpected + NextNormal(rng) * 0.5, 0, 2.5); // peers of similar age
            double needSum = 0;
            foreach (var p in measured)
            {
                double sign = p.Need switch { NeedDirection.LowIsNeed => -1, NeedDirection.HighIsNeed => +1, _ => 0 };
                double jitter = (rng.NextDouble() - 0.5) * p.YoungSd;
                double shift = p.Need == NeedDirection.DysregulationIsNeed
                    ? (rng.NextDouble() * 2 - 1) * aging * p.YoungSd
                    : sign * aging * p.YoungSd;
                double value = p.YoungMean + shift + jitter;
                needSum += AnalysisEngine.MarkerNeed(Z(value, p.YoungMean, p.YoungSd), p.Need);
            }
            qualities[i] = 100 - needSum / measured.Count;
        }
        int below = qualities.Count(q => q <= subjectQuality);
        return new CohortRanking(Size, Math.Round(100.0 * below / Size, 0));
    }

    private static double NextNormal(Random rng)
    {
        double u1 = 1 - rng.NextDouble(), u2 = 1 - rng.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    }

    private static double Z(double value, double mean, double sd) => sd > 0 ? (value - mean) / sd : 0;
}
