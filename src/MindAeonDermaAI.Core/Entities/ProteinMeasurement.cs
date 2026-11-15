namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A single protein measurement within an assessment, plus normalization results (z-score against the
/// young reference and, when a baseline exists, fold-change / percent-change).
/// </summary>
public class ProteinMeasurement
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public int BiomarkerId { get; set; }
    public ProteinBiomarker? Biomarker { get; set; }

    /// <summary>Measured value; null when the marker was not measured (missing data).</summary>
    public double? Value { get; set; }

    public int? UnitId { get; set; }
    public MeasurementUnit? Unit { get; set; }

    // Normalization outputs (computed by the analysis engine).
    public double? ZScore { get; set; }
    public double? FoldChange { get; set; }
    public double? PercentChange { get; set; }

    public bool IsMissing { get; set; }
    public bool IsOutlier { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
