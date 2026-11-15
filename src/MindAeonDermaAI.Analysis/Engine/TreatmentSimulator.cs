using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Simulates and compares candidate treatments. Each treatment's effective response scales with how well
/// its biomarker targets overlap the subject's actual biological need; the integrated score and the
/// targeted biomarkers are then projected over the given month horizons. Illustrative, non-diagnostic.
/// </summary>
public static class TreatmentSimulator
{
    public record MarkerState(string Code, string Name, double Z, NeedDirection Need, double Value);

    public record TreatmentOutcome(
        string Key, string Name, string Category, double EffectiveResponse,
        double[] Integrated, double Projected6mo,
        List<(string Name, double Pct6)> Biomarkers);

    public static List<TreatmentOutcome> Simulate(double baseline, IReadOnlyList<MarkerState> markers,
        int[] months, IEnumerable<string>? onlyKeys = null)
    {
        var byCode = markers.ToDictionary(m => m.Code, m => m, StringComparer.OrdinalIgnoreCase);
        var keys = onlyKeys?.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var outcomes = new List<TreatmentOutcome>();

        foreach (var t in TreatmentCatalog.All)
        {
            if (keys is not null && !keys.Contains(t.Key)) continue;

            // Overlap = average biological need on the markers this treatment targets (that were measured).
            var targeted = t.Effects.Keys.Where(byCode.ContainsKey).ToList();
            double overlap = targeted.Count > 0
                ? targeted.Average(c => AnalysisEngine.MarkerNeed(byCode[c].Z, byCode[c].Need))
                : 50;
            double eff = Math.Clamp(0.45 * t.BaseResponsiveness + 0.55 * overlap, 0, 100);

            var integrated = Projection.TrajectoryMonths(baseline, eff, months);
            double maxFrac = 0.05 + eff / 100.0 * 0.25;
            double sat6 = 1 - Math.Exp(-0.5 * 6);

            var bio = new List<(string, double)>();
            foreach (var (code, mag) in t.Effects)
            {
                if (!byCode.TryGetValue(code, out var m)) continue;
                double sign = m.Need switch { NeedDirection.LowIsNeed => +1, NeedDirection.HighIsNeed => -1, _ => -0.5 };
                double pct6 = Math.Round(sign * mag * maxFrac * sat6 * 100, 1);
                bio.Add((m.Name, pct6));
            }
            bio = bio.OrderByDescending(x => Math.Abs(x.Item2)).Take(4).ToList();

            int idx6 = Array.IndexOf(months, 6);
            double proj6 = idx6 >= 0 && idx6 < integrated.Length ? integrated[idx6] : integrated[^1];
            outcomes.Add(new TreatmentOutcome(t.Key, t.Name, t.Category, Math.Round(eff, 0), integrated, proj6, bio));
        }

        return outcomes.OrderByDescending(o => o.Projected6mo).ToList();
    }
}
