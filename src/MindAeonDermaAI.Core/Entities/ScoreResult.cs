using System.ComponentModel.DataAnnotations;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// Deterministic scoring output for an assessment: the three panel need-scores, component scores, and
/// the final integrated skin-quality score, together with the model rationale.
/// </summary>
public class ScoreResult
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    // Panel need scores (0–100). Higher = greater biological need.
    public double CollagenRemodelingNeedScore { get; set; }
    public double BarrierHydrationNeedScore { get; set; }
    public double DeepEcmLaxityNeedScore { get; set; }

    /// <summary>True when the top two panels differ by less than 10 points (mixed phenotype).</summary>
    public bool IsMixedPhenotype { get; set; }

    // Component scores feeding the integrated score.
    public double PanelScore { get; set; }
    public double ImageQualityScore { get; set; }
    public double LifestyleScore { get; set; }
    public double? CohortPercentileScore { get; set; }
    public double IntegratedSkinQualityScore { get; set; }

    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Moderate;

    /// <summary>Whether a real cohort was used (affects the integrated-score weighting formula).</summary>
    public bool UsedRealCohort { get; set; }
    public bool UsedSyntheticCohort { get; set; }

    /// <summary>Number of subjects in the (synthetic or real) reference cohort used for percentile ranking.</summary>
    public int CohortSize { get; set; }

    /// <summary>Comma-separated phenotype codes classified for this assessment (one or more).</summary>
    [MaxLength(300)]
    public string? PhenotypeCodesCsv { get; set; }

    // ---- Advanced multimodal intelligence ----

    /// <summary>Agreement between image-derived and protein-derived signals (0–100; 100 = fully concordant).</summary>
    public double CrossModalConcordance { get; set; }

    /// <summary>True when molecular (protein) need is high while the image still looks well — i.e. subclinical,
    /// molecular-ahead-of-visible change worth preventive monitoring.</summary>
    public bool IsSubclinical { get; set; }
    [MaxLength(400)]
    public string? SubclinicalNote { get; set; }

    /// <summary>Rate of change of biological need vs the prior baseline, per 30 days ("biological momentum").
    /// Positive = worsening faster. Null when no prior baseline exists.</summary>
    public double? AgingVelocity { get; set; }

    // Derived mechanistic indices (0–100; higher = healthier balance).
    public double? RemodelingBalanceIndex { get; set; }  // MMP-1 / TIMP-1 degradation balance
    public double? HyaluronanAxisIndex { get; set; }     // HAS2 × CD44 hydration signaling
    public double? BarrierResilienceIndex { get; set; }  // FLG/LOR vs IL-1α

    /// <summary>JSON list of {category, likelihood} — biological fit of each intervention category.</summary>
    public string? ResponseLikelihoodJson { get; set; }

    /// <summary>Counterfactual: projected integrated score if the single highest-need marker normalized.</summary>
    public double? CounterfactualScore { get; set; }
    [MaxLength(400)]
    public string? CounterfactualNote { get; set; }

    /// <summary>Environmental exposome burden (0–100; higher = greater environmental load).</summary>
    public double? ExposomeBurden { get; set; }

    // ---- Derived biological "ages" (years) — illustrative, non-diagnostic ----
    public double? FaceAge { get; set; }
    public double? ProteinAge { get; set; }
    public double? BarrierAge { get; set; }
    public double? CollagenAge { get; set; }
    public double? BiologicalSkinAge { get; set; }

    // ---- Per-modality confidence (0–100 %) ----
    public double? ProteinConfidence { get; set; }
    public double? ImageConfidence { get; set; }
    public double? OverallConfidence { get; set; }

    // ---- Stratified benchmark (matched demographic group) ----
    public double? BenchmarkPercentile { get; set; }
    [MaxLength(120)]
    public string? BenchmarkGroupLabel { get; set; }

    /// <summary>Name of the statistical/ML model actually applied (e.g. "Robust z-score + rule-based").</summary>
    [MaxLength(200)]
    public string ModelUsed { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? ModelRationale { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
