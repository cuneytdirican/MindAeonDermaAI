using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Digital-twin projection: a deterministic, saturating "what-if" trajectory of the integrated
/// skin-quality score over N weeks under a given intervention. The achievable gain scales with the
/// intervention's biological response likelihood; the curve saturates toward a realistic ceiling.
/// Illustrative and non-diagnostic — not a promise of outcome.
/// </summary>
public static class Projection
{
    /// <summary>Maximum plausible point-gain for a given response likelihood (0–100).</summary>
    public static double MaxGain(double responseLikelihood)
        => 4 + Math.Clamp(responseLikelihood, 0, 100) / 100.0 * 16.0; // +4 (supportive) … +20 (best fit)

    /// <summary>Weekly projected scores from week 0 (=baseline) to <paramref name="weeks"/> inclusive.</summary>
    public static double[] Trajectory(double baseline, double responseLikelihood, int weeks = 12)
    {
        double ceiling = Math.Min(96, baseline + MaxGain(responseLikelihood));
        const double k = 0.28; // approach rate
        var series = new double[weeks + 1];
        for (int w = 0; w <= weeks; w++)
            series[w] = Math.Round(baseline + (ceiling - baseline) * (1 - Math.Exp(-k * w)), 1);
        return series;
    }

    /// <summary>Projected integrated score at the given month horizons (0 = baseline).</summary>
    public static double[] TrajectoryMonths(double baseline, double responseLikelihood, int[] months)
    {
        double ceiling = Math.Min(96, baseline + MaxGain(responseLikelihood));
        const double kWeek = 0.28;
        return months.Select(m =>
        {
            double weeks = m * 4.345;
            return Math.Round(baseline + (ceiling - baseline) * (1 - Math.Exp(-kWeek * weeks)), 1);
        }).ToArray();
    }

    /// <summary>
    /// Projected value + %-change of a single biomarker after N months under an intervention, moving toward
    /// the healthier direction (opposite of its need direction) by an amount scaled by response likelihood.
    /// </summary>
    public static (double Projected, double PercentChange) BiomarkerTrajectory(
        double baselineValue, NeedDirection need, double responseLikelihood, int months)
    {
        double maxFrac = 0.05 + Math.Clamp(responseLikelihood, 0, 100) / 100.0 * 0.25; // up to ~30%
        double sat = 1 - Math.Exp(-0.5 * months);                                       // ~0.95 by 6 months
        double sign = need switch
        {
            NeedDirection.LowIsNeed => +1,   // deficient markers rise toward reference
            NeedDirection.HighIsNeed => -1,  // elevated markers fall toward reference
            _ => -0.5                        // dysregulated markers partially normalize
        };
        double change = sign * maxFrac * sat;
        return (Math.Round(baselineValue * (1 + change), 1), Math.Round(change * 100, 1));
    }
}
