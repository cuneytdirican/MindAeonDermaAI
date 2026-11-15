using System.ComponentModel.DataAnnotations;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A single assessment session for a subject: images + protein measurements + computed analysis results.
/// </summary>
public class Assessment
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    /// <summary>Current intervention / product category under evaluation (Smart-Select).</summary>
    public int? InterventionCategoryId { get; set; }
    public InterventionCategory? InterventionCategory { get; set; }

    public TimePointType TimePoint { get; set; } = TimePointType.Baseline;
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;

    public AssessmentStatus Status { get; set; } = AssessmentStatus.Draft;

    /// <summary>Panel selected by the engine as having the highest biological need.</summary>
    public int? SelectedPanelId { get; set; }
    public Panel? SelectedPanel { get; set; }

    public int? PrimaryPhenotypeId { get; set; }
    public PhenotypeType? PrimaryPhenotype { get; set; }

    /// <summary>Assay run / plate identifier — enables batch-drift monitoring across assessments.</summary>
    [MaxLength(60)]
    public string? AssayBatch { get; set; }

    // Environmental exposome context (entered or geo/season-derived).
    public double? ExposomeUvIndex { get; set; }
    public double? ExposomePm25 { get; set; }
    public double? ExposomeHumidity { get; set; }

    public ICollection<SkinImage> Images { get; set; } = new List<SkinImage>();
    public ICollection<ProteinMeasurement> Measurements { get; set; } = new List<ProteinMeasurement>();
    public ICollection<ZoneScore> ZoneScores { get; set; } = new List<ZoneScore>();
    public ICollection<InstrumentalReading> InstrumentalReadings { get; set; } = new List<InstrumentalReading>();

    public ScoreResult? ScoreResult { get; set; }
    public ReportDocument? Report { get; set; }
    public FacialGeometry? FacialGeometry { get; set; }

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
