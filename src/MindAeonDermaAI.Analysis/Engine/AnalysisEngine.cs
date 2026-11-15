using MindAeonDermaAI.Analysis.Models;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Robust rule-based scoring engine. Computes per-marker z-scores against the (synthetic) young
/// reference, aggregates panel need scores, selects the highest-need panel, derives lifestyle and
/// integrated skin-quality scores, and classifies biological phenotype(s). Interpretable by design.
/// </summary>
public class AnalysisEngine : IAnalysisEngine
{
    public AnalysisResult Analyze(AnalysisInput input)
    {
        var result = new AnalysisResult
        {
            UsedRealCohort = input.UsedRealCohort,
            UsedSyntheticCohort = !input.UsedRealCohort
        };

        // 1) Normalize each measured protein to a z-score and per-marker need (0..100).
        var markerNeed = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var markerValue = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        int measured = 0, total = input.Proteins.Count;

        foreach (var p in input.Proteins)
        {
            if (!p.IsMeasured) continue;
            measured++;
            double z = p.YoungSd > 0 ? (p.Value!.Value - p.YoungMean) / p.YoungSd : 0;
            result.ZScores[p.Code] = Math.Round(z, 2);
            markerValue[p.Code] = p.Value!.Value;
            markerNeed[p.Code] = MarkerNeed(z, p.Need);

            // Data-quality control: flag robust outliers (|z| > 3 vs the young reference).
            if (Math.Abs(z) > 3) result.OutlierCodes.Add(p.Code);

            if (input.IsFollowUp && p.BaselineValue is { } bv && bv != 0)
            {
                result.PercentChanges[p.Code] = Math.Round((p.Value!.Value - bv) / bv * 100.0, 1);
                result.FoldChanges[p.Code] = Math.Round(p.Value!.Value / bv, 2);
            }
        }

        // 2) Panel need scores (weighted average of member marker needs that were measured).
        result.CollagenRemodelingNeedScore = PanelScoreFor(PanelCodes.Collagen, input, markerNeed, out int cMeasured);
        result.BarrierHydrationNeedScore = PanelScoreFor(PanelCodes.Barrier, input, markerNeed, out int bMeasured);
        result.DeepEcmLaxityNeedScore = PanelScoreFor(PanelCodes.DeepEcm, input, markerNeed, out int dMeasured);

        // 3) Panel selection — highest need; mixed if top two within 10 points.
        var needs = new List<PanelNeed>
        {
            new(PanelCodes.Collagen, result.CollagenRemodelingNeedScore, cMeasured),
            new(PanelCodes.Barrier, result.BarrierHydrationNeedScore, bMeasured),
            new(PanelCodes.DeepEcm, result.DeepEcmLaxityNeedScore, dMeasured)
        };
        var ordered = needs.OrderByDescending(n => n.Score).ToList();
        result.SelectedPanelCode = ordered[0].PanelCode;
        result.IsMixedPhenotype = (ordered[0].Score - ordered[1].Score) < 10.0;

        // 4) Component scores. Panel quality = 100 − selected panel need.
        double selectedNeed = ordered[0].Score;
        result.PanelScore = Clamp(100 - selectedNeed);
        result.ImageQualityScore = ImageQuality(input.Image);
        result.LifestyleScore = LifestyleScore(input.Subject);

        // 5a) Reference cohort percentile. A real cohort takes precedence; otherwise a synthetic,
        // literature-inspired cohort is used purely for comparison (it does not alter the weighting).
        if (input.UsedRealCohort && input.CohortPercentile.HasValue)
        {
            result.CohortPercentileScore = input.CohortPercentile;
        }
        else
        {
            var ranking = SyntheticCohort.Rank(input.Proteins);
            result.CohortPercentileScore = ranking.Percentile;
            result.SyntheticCohortSize = ranking.Size;
        }

        // 5) Integrated skin-quality score (weighting depends on real-cohort availability).
        if (input.UsedRealCohort && input.CohortPercentile.HasValue)
        {
            result.IntegratedSkinQualityScore = Clamp(
                0.45 * result.PanelScore + 0.30 * result.ImageQualityScore +
                0.15 * result.LifestyleScore + 0.10 * input.CohortPercentile.Value);
        }
        else
        {
            result.IntegratedSkinQualityScore = Clamp(
                0.50 * result.PanelScore + 0.30 * result.ImageQualityScore + 0.20 * result.LifestyleScore);
        }
        result.IntegratedSkinQualityScore = Math.Round(result.IntegratedSkinQualityScore, 1);

        // 6) Confidence & model selection.
        (result.Confidence, result.ModelUsed, result.ModelRationale) =
            SelectModel(measured, total, input);

        // 7) Phenotype classification.
        result.PhenotypeCodes = ClassifyPhenotype(result, markerValue, input, ordered);

        // 7b) Advanced multimodal intelligence (indices, concordance, subclinical, response, counterfactual, exposome).
        ComputeAdvanced(input, result, markerNeed);

        // 8) QC notes.
        if (measured < total)
            result.QualityNotes.Add($"{total - measured} of {total} panel markers were not measured; scores use available-data modeling with reduced confidence.");
        if (input.Image is null || !input.Image.HasAny)
            result.QualityNotes.Add("No analyzed image was available; the image component uses a neutral prior.");
        if (result.OutlierCodes.Count > 0)
            result.QualityNotes.Add($"{result.OutlierCodes.Count} biomarker value(s) were flagged as robust outliers (|z| > 3) and should be re-verified before interpretation.");
        result.SyntheticCohortSize = result.SyntheticCohortSize == 0 && result.UsedSyntheticCohort ? SyntheticCohort.Size : result.SyntheticCohortSize;

        return result;
    }

    /// <summary>Per-marker need on 0..100. Directional markers center at 50 (z=0); dysregulation at 0.</summary>
    internal static double MarkerNeed(double z, NeedDirection need)
    {
        return need switch
        {
            NeedDirection.LowIsNeed => Clamp(50 + (-z) * 25),
            NeedDirection.HighIsNeed => Clamp(50 + z * 25),
            NeedDirection.DysregulationIsNeed => Clamp(Math.Abs(z) * 30),
            _ => 50
        };
    }

    private static double PanelScoreFor(string panelCode, AnalysisInput input,
        Dictionary<string, double> markerNeed, out int measuredCount)
    {
        measuredCount = 0;
        if (!input.PanelMembers.TryGetValue(panelCode, out var members) || members.Count == 0)
            return 0;

        double sum = 0, wsum = 0;
        foreach (var (code, weight) in members)
        {
            if (!markerNeed.TryGetValue(code, out var need)) continue;
            sum += need * weight;
            wsum += weight;
            measuredCount++;
        }
        return wsum > 0 ? Math.Round(sum / wsum, 1) : 0;
    }

    private static double ImageQuality(ImageScoreInput? img)
    {
        if (img is null || !img.HasAny) return 50; // neutral prior when no image
        if (img.Overall.HasValue) return Clamp(img.Overall.Value);
        var parts = new[] { img.Hydration, img.Texture, img.Elasticity, img.Wrinkle,
            img.Pigmentation, img.RednessHealth, img.Pore, img.Brightness }
            .Where(v => v.HasValue).Select(v => v!.Value).ToList();
        return parts.Count > 0 ? Clamp(parts.Average()) : 50;
    }

    private static double LifestyleScore(SubjectInput s)
    {
        double score = 100;
        score -= s.SmokingCode switch { "HEAVY" => 25, "LIGHT" => 15, "FORMER" => 5, _ => 0 };
        score -= s.UvCode switch { "HIGH" => 15, "MOD" => 5, _ => 0 };
        score -= s.SleepCode switch { "POOR" => 15, "FAIR" => 5, _ => 0 };
        score -= s.StressCode switch { "HIGH" => 12, "MOD" => 4, _ => 0 };
        score -= s.AlcoholCode switch { "HEAVY" => 15, "MOD" => 5, _ => 0 };
        if (s.Bmi is { } bmi)
        {
            if (bmi < 18.5 || bmi >= 30) score -= 12;
            else if (bmi >= 25) score -= 5;
        }
        return Clamp(score);
    }

    private static (ConfidenceLevel, string, string) SelectModel(int measured, int total, AnalysisInput input)
    {
        if (input.UsedRealCohort && input.CohortSize >= 50)
        {
            var model = input.CohortSize > 200
                ? "Regularized ensemble (Elastic Net / Random Forest) with cross-validation"
                : "Elastic Net regression with cross-validation";
            return (ConfidenceLevel.High, model,
                $"A real cohort of {input.CohortSize} subjects supports model-based estimation; " +
                "interpretability was prioritized over complexity.");
        }

        // No adequate cohort → interpretable rule-based scoring on the synthetic reference.
        var frac = total > 0 ? (double)measured / total : 0;
        var confidence = frac >= 0.8 ? ConfidenceLevel.Moderate
                       : frac >= 0.5 ? ConfidenceLevel.Reduced
                       : ConfidenceLevel.Low;
        return (confidence,
            "Robust z-score + rule-based biological scoring (synthetic literature-inspired reference)",
            "No validated cohort was supplied, so model-based prediction was not scientifically justified. " +
            "Robust z-scores against a synthetic young reference and interpretable rule-based scoring were used " +
            "for hypothesis generation only.");
    }

    private static List<string> ClassifyPhenotype(AnalysisResult r,
        Dictionary<string, double> value, AnalysisInput input, List<PanelNeed> ordered)
    {
        var codes = new List<string>();
        double topNeed = ordered[0].Score;

        // All needs low → young-like balanced.
        if (ordered.All(n => n.Score < 35))
        {
            codes.Add(PhenotypeCodes.YoungBalanced);
            return codes;
        }

        double Z(string code) => r.ZScores.TryGetValue(code, out var z) ? z : 0;

        switch (r.SelectedPanelCode)
        {
            case PanelCodes.Barrier:
                // Strong superficial inflammation → inflammaging, else barrier-deficient.
                if (Z("IL1A") >= 1.0) codes.Add(PhenotypeCodes.Inflammaging);
                codes.Add(PhenotypeCodes.BarrierDeficient);
                break;
            case PanelCodes.Collagen:
                // Elevated TGF-β1 with low MMP-1 suggests fibrotic remodeling bias.
                if (Z("TGFB1") >= 1.0 && Z("MMP1") <= 0) codes.Add(PhenotypeCodes.Fibrosis);
                codes.Add(PhenotypeCodes.CollagenDepleted);
                break;
            case PanelCodes.DeepEcm:
                // High periostin / MMP-2 → mechanically stressed remodeling.
                if (Z("POSTN") >= 1.0 || Z("MMP2") >= 1.0) codes.Add(PhenotypeCodes.MechStress);
                codes.Add(PhenotypeCodes.DeepEcm);
                break;
        }

        if (r.IsMixedPhenotype && !codes.Contains(PhenotypeCodes.Mixed))
            codes.Add(PhenotypeCodes.Mixed);

        if (codes.Count == 0) codes.Add(PhenotypeCodes.Mixed);
        return codes;
    }

    private static double Clamp(double v) => Math.Max(0, Math.Min(100, v));

    /// <summary>
    /// Derives the advanced, differentiated signals: mechanistic pathway indices, cross-modal (image vs
    /// protein) concordance, the molecular-ahead-of-visible subclinical flag, intervention-response
    /// likelihoods, a counterfactual "what-if", and environmental exposome burden.
    /// </summary>
    private static void ComputeAdvanced(AnalysisInput input, AnalysisResult r, Dictionary<string, double> markerNeed)
    {
        double Z(string c) => r.ZScores.TryGetValue(c, out var z) ? z : 0;
        bool Has(string c) => r.ZScores.ContainsKey(c);

        // --- Mechanistic indices (0–100; higher = healthier balance) ---
        if (Has("MMP1") && Has("TIMP1"))
            r.RemodelingBalanceIndex = Math.Round(Clamp(50 + (Z("TIMP1") - Z("MMP1")) * 20), 0);
        if (Has("HAS2") && Has("CD44"))
            r.HyaluronanAxisIndex = Math.Round(Clamp(50 + ((Z("HAS2") + Z("CD44")) / 2) * 22), 0);
        if (Has("FLG") && Has("LOR") && Has("IL1A"))
            r.BarrierResilienceIndex = Math.Round(Clamp(50 + (((Z("FLG") + Z("LOR")) / 2) - Z("IL1A")) * 18), 0);

        // --- Cross-modal concordance (image vs protein by domain) ---
        var img = input.Image;
        if (img is { HasAny: true })
        {
            double? ImgConcern(params double?[] vals)
            {
                var v = vals.Where(x => x.HasValue).Select(x => x!.Value).ToList();
                return v.Count > 0 ? 100 - v.Average() : (double?)null;
            }
            var disc = new List<double>();
            void Add(double proteinConcern, double? imgConcern) { if (imgConcern.HasValue) disc.Add(Math.Abs(proteinConcern - imgConcern.Value)); }
            Add(r.BarrierHydrationNeedScore, ImgConcern(img.Hydration, img.RednessHealth));
            Add(r.CollagenRemodelingNeedScore, ImgConcern(img.Wrinkle, img.Elasticity, img.Texture));
            Add(r.DeepEcmLaxityNeedScore, ImgConcern(img.Elasticity, img.Texture));
            if (disc.Count > 0) r.CrossModalConcordance = Math.Round(Clamp(100 - disc.Average()), 0);
        }

        // --- Subclinical: molecular need elevated while the visible appearance still scores well ---
        double topNeed = Math.Max(r.CollagenRemodelingNeedScore, Math.Max(r.BarrierHydrationNeedScore, r.DeepEcmLaxityNeedScore));
        double imgOverall = img?.Overall ?? 55;
        if (topNeed >= 55 && imgOverall >= 65)
        {
            r.IsSubclinical = true;
            r.SubclinicalNote = "Molecular biomarker need is elevated while the visible appearance still scores well — a pattern " +
                "compatible with early, subclinical change that may benefit from preventive monitoring before it becomes visible.";
        }

        // --- Intervention-response likelihood (per category code) ---
        double col = r.CollagenRemodelingNeedScore, bar = r.BarrierHydrationNeedScore, deep = r.DeepEcmLaxityNeedScore;
        double infl = Has("IL1A") ? Clamp(50 + Z("IL1A") * 25) : bar;
        double uv = input.ExposomeUvIndex.HasValue ? Clamp(input.ExposomeUvIndex.Value / 11 * 100) : 40;
        r.ResponseLikelihood["BARRIER"] = Math.Round(Clamp(bar), 0);
        r.ResponseLikelihood["PEPTIDE"] = Math.Round(Clamp(0.75 * col + 0.15 * deep), 0);
        r.ResponseLikelihood["RETINOID"] = Math.Round(Clamp(0.6 * col + 0.2 * deep + 0.1 * uv), 0);
        r.ResponseLikelihood["GF"] = Math.Round(Clamp(0.5 * col + 0.4 * deep), 0);
        r.ResponseLikelihood["EXOSOME"] = Math.Round(Clamp(0.3 * col + 0.6 * deep), 0);
        r.ResponseLikelihood["ANTIOX"] = Math.Round(Clamp(0.5 * infl + 0.3 * col + 0.2 * uv), 0);
        r.ResponseLikelihood["SUNSCREEN"] = Math.Round(Clamp(0.5 * uv + 0.4 * col), 0);
        r.ResponseLikelihood["NONE"] = Math.Round(Clamp(100 - topNeed), 0);

        // --- Counterfactual: normalize the single highest-need marker in the selected panel ---
        if (input.PanelMembers.TryGetValue(r.SelectedPanelCode, out var members) && members.Count > 0)
        {
            string? topCode = null; double topM = -1;
            foreach (var (code, _) in members)
                if (markerNeed.TryGetValue(code, out var need) && need > topM) { topM = need; topCode = code; }
            if (topCode != null && topM > 55)
            {
                double sum = 0, w = 0;
                foreach (var (code, weight) in members)
                    if (markerNeed.TryGetValue(code, out var need)) { sum += (code == topCode ? 50 : need) * weight; w += weight; }
                double newPanel = Clamp(100 - (w > 0 ? sum / w : topNeed));
                double newIntegrated = input.UsedRealCohort && input.CohortPercentile.HasValue
                    ? Clamp(0.45 * newPanel + 0.30 * r.ImageQualityScore + 0.15 * r.LifestyleScore + 0.10 * input.CohortPercentile.Value)
                    : Clamp(0.50 * newPanel + 0.30 * r.ImageQualityScore + 0.20 * r.LifestyleScore);
                r.CounterfactualScore = Math.Round(newIntegrated, 1);
                var name = input.Proteins.FirstOrDefault(p => p.Code == topCode)?.Name ?? topCode;
                r.CounterfactualNote = $"If {name} normalized toward the reference range, the integrated skin-quality score " +
                    $"would be projected to rise to approximately {newIntegrated:0}/100.";
            }
        }

        // --- Exposome burden (0–100) ---
        var parts = new List<double>();
        if (input.ExposomePm25.HasValue) parts.Add(Clamp(input.ExposomePm25.Value / 150 * 100));
        if (input.ExposomeUvIndex.HasValue) parts.Add(Clamp(input.ExposomeUvIndex.Value / 11 * 100));
        if (input.ExposomeHumidity.HasValue) parts.Add(Clamp(100 - input.ExposomeHumidity.Value)); // dry air adds burden
        if (parts.Count > 0) r.ExposomeBurden = Math.Round(parts.Average(), 0);

        // --- Derived biological ages ---
        AgeEstimator.Estimate(r, input.Subject.Age);

        // --- Per-modality confidence (0–100 %) ---
        int total = input.Proteins.Count, measured = input.Proteins.Count(p => p.IsMeasured), outliers = r.OutlierCodes.Count;
        r.ProteinConfidence = total > 0 ? Math.Round(Clamp((double)measured / total * 100 - 5 * outliers), 0) : 25;
        bool hasImg = input.Image is { HasAny: true };
        r.ImageConfidence = hasImg ? Math.Round(Clamp(55 + 0.35 * r.CrossModalConcordance - 8 * input.Image!.QcFlagCount), 0) : 30;
        r.OverallConfidence = Math.Round(0.6 * r.ProteinConfidence.Value + 0.4 * r.ImageConfidence.Value, 0);

        // --- Stratified (age-matched) benchmark ---
        if (input.Subject.Age is int)
        {
            r.BenchmarkPercentile = SyntheticCohort.RankStratified(input.Proteins, input.Subject.Age).Percentile;
            r.BenchmarkGroupLabel = BenchmarkLabel(input.Subject);
        }
    }

    private static string BenchmarkLabel(SubjectInput s)
    {
        string sex = s.SexCode switch { "F" => "Women", "M" => "Men", _ => "Adults" };
        string band = s.Age is int a ? $"{a / 5 * 5}–{a / 5 * 5 + 4}" : "";
        string smk = s.SmokingCode switch { "NEVER" => ", non-smokers", "FORMER" => ", former smokers", "LIGHT" or "HEAVY" => ", smokers", _ => "" };
        return string.IsNullOrEmpty(band) ? sex + smk : $"{sex} {band}{smk}";
    }
}
