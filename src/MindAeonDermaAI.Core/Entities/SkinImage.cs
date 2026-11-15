using System.ComponentModel.DataAnnotations;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// An uploaded skin/face photograph plus the standardized 0–100 appearance scores produced by the
/// multimodal vision model. Redness is stored already reversed to a health-oriented score.
/// </summary>
public class SkinImage
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    [Required, MaxLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? ContentType { get; set; }

    // Image quality-control flags raised during analysis.
    public bool HasLightingArtifact { get; set; }
    public bool HasMakeup { get; set; }
    public bool HasFilter { get; set; }
    public bool NonStandardAngle { get; set; }

    // Standardized appearance scores (0–100). Null until analyzed.
    public double? HydrationScore { get; set; }
    public double? TextureSmoothnessScore { get; set; }
    public double? ElasticityScore { get; set; }
    public double? WrinkleBurdenScore { get; set; }
    public double? PigmentationUniformityScore { get; set; }
    /// <summary>Redness reversed to a health score (100 = no redness).</summary>
    public double? RednessHealthScore { get; set; }
    public double? PoreVisibilityScore { get; set; }
    public double? BrightnessScore { get; set; }
    public double? OverallSkinQualityScore { get; set; }

    /// <summary>Short model rationale returned alongside the scores.</summary>
    [MaxLength(2000)]
    public string? AnalysisNotes { get; set; }

    public bool IsAnalyzed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
