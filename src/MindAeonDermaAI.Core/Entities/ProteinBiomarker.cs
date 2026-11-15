using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A measurable protein biomarker (e.g. COL1A1, MMP-1, filaggrin) belonging to a biological domain.
/// Carries literature-inspired young-reference statistics used for z-score normalization. Reference
/// values are flagged synthetic and must not be presented as validated clinical ranges.
/// </summary>
public class ProteinBiomarker : LookupBase
{
    public int DomainId { get; set; }
    public BiologicalDomain? Domain { get; set; }

    /// <summary>Which direction of deviation signals increased biological need.</summary>
    public NeedDirection NeedDirection { get; set; } = NeedDirection.LowIsNeed;

    /// <summary>Literature-inspired mean for young healthy reference (synthetic).</summary>
    public double YoungRefMean { get; set; }

    /// <summary>Literature-inspired standard deviation for young healthy reference (synthetic).</summary>
    public double YoungRefSd { get; set; }

    public int? UnitId { get; set; }
    public MeasurementUnit? Unit { get; set; }

    /// <summary>True when the reference statistics are simulated / literature-inspired (not validated).</summary>
    public bool IsSyntheticReference { get; set; } = true;

    /// <summary>Shift in the reference mean per decade beyond age 30 (stratified reference). Signed in the
    /// biomarker's natural direction (e.g. COL1A1 declines with age → negative).</summary>
    public double AgeAdjustPerDecade { get; set; }

    // Self-calibrating reference: recomputed from the platform's own cohort once enough data exists.
    public double? CohortRefMean { get; set; }
    public double? CohortRefSd { get; set; }
    public int CohortRefN { get; set; }

    public ICollection<PanelBiomarker> PanelBiomarkers { get; set; } = new List<PanelBiomarker>();
}
