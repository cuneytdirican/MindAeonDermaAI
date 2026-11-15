using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Analysis.Models;

/// <summary>A single protein input with its synthetic young-reference statistics.</summary>
public class ProteinInput
{
    public int BiomarkerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public NeedDirection Need { get; set; }
    public double? Value { get; set; }
    public double YoungMean { get; set; }
    public double YoungSd { get; set; }
    /// <summary>Optional baseline value for fold-change / percent-change on follow-up.</summary>
    public double? BaselineValue { get; set; }

    public bool IsMeasured => Value.HasValue;
}

/// <summary>Demographic + lifestyle inputs (codes come from the seeded lookups).</summary>
public class SubjectInput
{
    public int? Age { get; set; }
    public double? Bmi { get; set; }
    public string? SexCode { get; set; }
    public string? SmokingCode { get; set; }
    public string? AlcoholCode { get; set; }
    public string? SleepCode { get; set; }
    public string? StressCode { get; set; }
    public string? UvCode { get; set; }
}

/// <summary>Standardized 0–100 image scores (redness already reversed to a health score).</summary>
public class ImageScoreInput
{
    public double? Hydration { get; set; }
    public double? Texture { get; set; }
    public double? Elasticity { get; set; }
    public double? Wrinkle { get; set; }
    public double? Pigmentation { get; set; }
    public double? RednessHealth { get; set; }
    public double? Pore { get; set; }
    public double? Brightness { get; set; }
    public double? Overall { get; set; }
    /// <summary>Count of raised QC flags (makeup/filter/lighting/angle) across analyzed images.</summary>
    public int QcFlagCount { get; set; }

    public bool HasAny => Overall.HasValue || Hydration.HasValue;
}

/// <summary>Everything the scoring engine needs for one assessment.</summary>
public class AnalysisInput
{
    public SubjectInput Subject { get; set; } = new();
    public List<ProteinInput> Proteins { get; set; } = new();
    public ImageScoreInput? Image { get; set; }

    /// <summary>Panel code → list of (biomarker code, weight) memberships.</summary>
    public Dictionary<string, List<(string Code, double Weight)>> PanelMembers { get; set; } = new();

    public bool UsedRealCohort { get; set; }
    public int CohortSize { get; set; }
    public double? CohortPercentile { get; set; }
    public bool IsFollowUp { get; set; }

    // Environmental exposome context.
    public double? ExposomeUvIndex { get; set; }
    public double? ExposomePm25 { get; set; }
    public double? ExposomeHumidity { get; set; }
}

/// <summary>Per-panel need score.</summary>
public record PanelNeed(string PanelCode, double Score, int MeasuredCount);

/// <summary>Full deterministic scoring output.</summary>
public class AnalysisResult
{
    public double CollagenRemodelingNeedScore { get; set; }
    public double BarrierHydrationNeedScore { get; set; }
    public double DeepEcmLaxityNeedScore { get; set; }

    public string SelectedPanelCode { get; set; } = string.Empty;
    public bool IsMixedPhenotype { get; set; }

    public double PanelScore { get; set; }
    public double ImageQualityScore { get; set; }
    public double LifestyleScore { get; set; }
    public double? CohortPercentileScore { get; set; }
    public double IntegratedSkinQualityScore { get; set; }

    public ConfidenceLevel Confidence { get; set; }
    public bool UsedRealCohort { get; set; }
    public bool UsedSyntheticCohort { get; set; }
    public int SyntheticCohortSize { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public string ModelRationale { get; set; } = string.Empty;

    public List<string> PhenotypeCodes { get; set; } = new();

    // ---- Advanced multimodal intelligence ----
    public double CrossModalConcordance { get; set; } = 50;
    public bool IsSubclinical { get; set; }
    public string? SubclinicalNote { get; set; }
    public double? RemodelingBalanceIndex { get; set; }
    public double? HyaluronanAxisIndex { get; set; }
    public double? BarrierResilienceIndex { get; set; }
    /// <summary>Intervention category code → biological fit likelihood (0–100).</summary>
    public Dictionary<string, double> ResponseLikelihood { get; set; } = new();
    public double? CounterfactualScore { get; set; }
    public string? CounterfactualNote { get; set; }
    public double? ExposomeBurden { get; set; }

    // Derived biological ages (years).
    public double? FaceAge { get; set; }
    public double? ProteinAge { get; set; }
    public double? BarrierAge { get; set; }
    public double? CollagenAge { get; set; }
    public double? BiologicalSkinAge { get; set; }

    // Per-modality confidence (0–100 %).
    public double? ProteinConfidence { get; set; }
    public double? ImageConfidence { get; set; }
    public double? OverallConfidence { get; set; }

    // Stratified benchmark.
    public double? BenchmarkPercentile { get; set; }
    public string? BenchmarkGroupLabel { get; set; }

    /// <summary>Per-biomarker normalization (code → z-score) for the report tables.</summary>
    public Dictionary<string, double> ZScores { get; set; } = new();
    public Dictionary<string, double> PercentChanges { get; set; } = new();
    public Dictionary<string, double> FoldChanges { get; set; } = new();
    /// <summary>Biomarker codes flagged as robust outliers (|z| &gt; 3) during data-quality control.</summary>
    public HashSet<string> OutlierCodes { get; set; } = new();
    public List<string> QualityNotes { get; set; } = new();
}
