using System.ComponentModel.DataAnnotations;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A study subject / user whose skin biology is assessed. Categorical attributes are foreign keys to
/// lookups (Smart-Select only); numeric measures are entered directly. No identifying free-text beyond
/// a display code is stored.
/// </summary>
public class Subject
{
    public int Id { get; set; }

    /// <summary>Non-identifying reference code (e.g. "SUBJ-0007"). Auto-generated.</summary>
    [MaxLength(40)]
    public string Code { get; set; } = string.Empty;

    /// <summary>Optional display alias used in the UI; never used for clinical inference.</summary>
    [MaxLength(120)]
    public string? DisplayName { get; set; }

    public int? Age { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }

    /// <summary>Computed body mass index (kg/m²); null when height/weight missing.</summary>
    public double? Bmi { get; set; }

    // Categorical demographics — Smart-Select foreign keys.
    public int? SexId { get; set; }
    public Sex? Sex { get; set; }

    public int? SkinTypeId { get; set; }
    public SkinType? SkinType { get; set; }

    public int? FitzpatrickTypeId { get; set; }
    public FitzpatrickType? FitzpatrickType { get; set; }

    public int? MenopauseStatusId { get; set; }
    public MenopauseStatus? MenopauseStatus { get; set; }

    // Lifestyle — Smart-Select foreign keys.
    public int? SmokingStatusId { get; set; }
    public SmokingStatus? SmokingStatus { get; set; }

    public int? AlcoholUseId { get; set; }
    public AlcoholUse? AlcoholUse { get; set; }

    public int? SleepQualityId { get; set; }
    public SleepQuality? SleepQuality { get; set; }

    public int? StressLevelId { get; set; }
    public StressLevel? StressLevel { get; set; }

    public int? UVExposureLevelId { get; set; }
    public UVExposureLevel? UVExposureLevel { get; set; }

    // Consent & privacy (compliance).
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentAtUtc { get; set; }
    [MaxLength(200)]
    public string? ConsentPurpose { get; set; }
    /// <summary>When true, the display name is masked in the UI/exports (pseudonymized) — the code still identifies the record.</summary>
    public bool Anonymized { get; set; }

    public ICollection<SubjectAestheticProcedure> AestheticProcedures { get; set; } = new List<SubjectAestheticProcedure>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection<WearableReading> WearableReadings { get; set; } = new List<WearableReading>();
    public ICollection<OmicsMarker> OmicsMarkers { get; set; } = new List<OmicsMarker>();

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Prior aesthetic procedures the subject has undergone (many-to-many via lookup).</summary>
public class SubjectAestheticProcedure
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int AestheticProcedureId { get; set; }
    public AestheticProcedure? AestheticProcedure { get; set; }
}
