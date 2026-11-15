namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// An optional reference cohort. When a real cohort is provided it is the primary reference; otherwise
/// the engine generates a literature-inspired synthetic cohort in memory, clearly labelled simulated.
/// </summary>
public class Cohort : LookupBase
{
    /// <summary>False for real uploaded cohorts, true for simulated literature-inspired references.</summary>
    public bool IsSynthetic { get; set; }

    public ICollection<CohortSubject> Subjects { get; set; } = new List<CohortSubject>();
}

public class CohortSubject
{
    public int Id { get; set; }

    public int CohortId { get; set; }
    public Cohort? Cohort { get; set; }

    public int? Age { get; set; }
    public int? SexId { get; set; }
    public Sex? Sex { get; set; }

    /// <summary>Optional profile label (e.g. "young-healthy", "photoaged", "smoker").</summary>
    public string? ProfileLabel { get; set; }

    public ICollection<CohortMeasurement> Measurements { get; set; } = new List<CohortMeasurement>();
}

public class CohortMeasurement
{
    public int Id { get; set; }

    public int CohortSubjectId { get; set; }
    public CohortSubject? CohortSubject { get; set; }

    public int BiomarkerId { get; set; }
    public ProteinBiomarker? Biomarker { get; set; }

    public double Value { get; set; }
}
