namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// Per-facial-zone appearance scores (0–100) enabling a regional skin-quality heatmap. Aging is
/// spatially heterogeneous, so zone-level scoring is far more informative than a single global value.
/// </summary>
public class ZoneScore
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public int FacialZoneId { get; set; }
    public FacialZone? FacialZone { get; set; }

    public double? Overall { get; set; }
    public double? Hydration { get; set; }
    public double? Wrinkle { get; set; }
    /// <summary>Redness reversed to a health score (100 = no redness).</summary>
    public double? RednessHealth { get; set; }
    public double? Pigmentation { get; set; }
}

/// <summary>
/// An instrumental / multi-omics reading (e.g. TEWL, corneometry, cutometer elasticity), demonstrating
/// the platform's readiness to ingest device and omics data beyond images and proteins.
/// </summary>
public class InstrumentalReading
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

    public int InstrumentTypeId { get; set; }
    public InstrumentType? InstrumentType { get; set; }

    public double Value { get; set; }
    public int? FacialZoneId { get; set; }
    public FacialZone? FacialZone { get; set; }
}
