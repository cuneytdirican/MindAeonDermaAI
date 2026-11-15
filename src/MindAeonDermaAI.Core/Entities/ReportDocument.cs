namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// The generated report narrative for an assessment. Structured scores live in <see cref="ScoreResult"/>;
/// this holds the LLM-authored prose sections (stored as JSON) plus the transparent reasoning summary.
/// </summary>
public class ReportDocument
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    /// <summary>JSON document holding all narrative sections keyed by name.</summary>
    public string SectionsJson { get; set; } = "{}";

    /// <summary>Concise, non-sensitive reasoning summary (hidden chain-of-thought is never stored).</summary>
    public string? ReasoningSummary { get; set; }

    public string? ExecutiveSummary { get; set; }
    public string? PersonalizedInterpretation { get; set; }
    public string? PhenotypeClassificationText { get; set; }
    public string? Limitations { get; set; }

    /// <summary>True when literature-inspired synthetic data was used and must be labelled as such.</summary>
    public bool UsedSyntheticData { get; set; }

    public string? GeneratedByProvider { get; set; }
    public string? GeneratedByModel { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
