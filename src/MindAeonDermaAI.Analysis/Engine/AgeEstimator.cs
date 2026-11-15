using MindAeonDermaAI.Analysis.Models;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Derives illustrative biological "ages" (years) from the already-computed multimodal scores, anchored to
/// chronological age. Each age reflects one lens (image appearance, protein status, barrier, collagen);
/// the biological skin age is a weighted blend. Non-diagnostic and illustrative — a communication device,
/// not a validated clinical age.
/// </summary>
public static class AgeEstimator
{
    public static void Estimate(AnalysisResult r, int? chronologicalAge)
    {
        double chrono = chronologicalAge is > 0 ? chronologicalAge.Value : 40;

        // Higher need / lower quality → older; +1 year per ~3 points of deviation from the 50 midpoint.
        double FromNeed(double need) => Clamp(chrono + (need - 50) / 3.0);
        double FromQuality(double quality) => Clamp(chrono + (50 - quality) / 2.5);

        r.FaceAge = Math.Round(FromQuality(r.ImageQualityScore), 0);
        r.CollagenAge = Math.Round(FromNeed(r.CollagenRemodelingNeedScore), 0);
        r.BarrierAge = Math.Round(FromNeed(r.BarrierHydrationNeedScore), 0);

        double avgNeed = (r.CollagenRemodelingNeedScore + r.BarrierHydrationNeedScore + r.DeepEcmLaxityNeedScore) / 3.0;
        r.ProteinAge = Math.Round(FromNeed(avgNeed), 0);

        r.BiologicalSkinAge = Math.Round(Clamp(
            0.35 * r.ProteinAge!.Value + 0.25 * r.CollagenAge!.Value +
            0.20 * r.BarrierAge!.Value + 0.20 * r.FaceAge!.Value), 0);
    }

    private static double Clamp(double v) => Math.Max(15, Math.Min(90, v));
}
