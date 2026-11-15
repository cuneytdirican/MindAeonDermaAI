using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Web.Models;

public class AssessmentListRow
{
    public int Id { get; set; }
    public string SubjectCode { get; set; } = "";
    public string? SubjectName { get; set; }
    public string Status { get; set; } = "";
    public string TimePoint { get; set; } = "";
    public string? Panel { get; set; }
    public double? Score { get; set; }
    public DateTime Created { get; set; }
}

public class AssessmentCreateViewModel
{
    public int SubjectId { get; set; }
    public int? InterventionCategoryId { get; set; }
    public string? InterventionCategoryName { get; set; }
    public TimePointType TimePoint { get; set; } = TimePointType.Baseline;

    // Assay batch + environmental exposome context.
    public string? AssayBatch { get; set; }
    public double? ExposomeUvIndex { get; set; }
    public double? ExposomePm25 { get; set; }
    public double? ExposomeHumidity { get; set; }

    public List<SubjectOption> Subjects { get; set; } = new();
    public List<DomainGroup> Domains { get; set; } = new();

    public record SubjectOption(int Id, string Label);
    public record DomainGroup(string Domain, List<BiomarkerField> Biomarkers);
    public record BiomarkerField(int Id, string Name, string Unit, double YoungMean);
}

public class AssessmentDetailsViewModel
{
    public int Id { get; set; }
    public string SubjectCode { get; set; } = "";
    public string? SubjectName { get; set; }
    public string TimePoint { get; set; } = "";
    public string? InterventionCategory { get; set; }
    public AssessmentStatus Status { get; set; }
    public DateTime Created { get; set; }

    public List<ImageItem> Images { get; set; } = new();
    public List<MeasurementItem> Measurements { get; set; } = new();

    public bool HasScore { get; set; }
    public double? IntegratedScore { get; set; }
    public string? SelectedPanel { get; set; }
    public string? Phenotype { get; set; }
    public bool AiReady { get; set; }
    public int AnalyzedImageCount { get; set; }

    public string? AssayBatch { get; set; }
    public double? ExposomeUvIndex { get; set; }
    public double? ExposomePm25 { get; set; }
    public double? ExposomeHumidity { get; set; }
    public int ZoneCount { get; set; }
    public List<InstrumentItem> Instruments { get; set; } = new();

    public record ImageItem(int Id, bool Analyzed, double? Overall, string? Notes,
        bool Makeup, bool Filter, bool Lighting, bool Angle);
    public record MeasurementItem(string Name, string Domain, double? Value, string Unit, double? Z);
    public record InstrumentItem(string Name, double Value);
}

/// <summary>Treatment simulation comparison.</summary>
public class SimulateViewModel
{
    public int AssessmentId { get; set; }
    public string SubjectCode { get; set; } = "";
    public double Baseline { get; set; }
    public int[] Months { get; } = { 0, 3, 6, 12 };
    public List<MindAeonDermaAI.Analysis.Engine.TreatmentSimulator.TreatmentOutcome> Outcomes { get; set; } = new();
}

/// <summary>Digital-twin multi-horizon projection (integrated score + per-biomarker).</summary>
public class DigitalTwinViewModel
{
    public int AssessmentId { get; set; }
    public string SubjectCode { get; set; } = "";
    public double Baseline { get; set; }
    public string TopTreatment { get; set; } = "";
    public double TopLikelihood { get; set; }
    public int[] Months { get; } = { 0, 3, 6, 12 };
    public double[] Integrated { get; set; } = System.Array.Empty<double>();
    public List<BiomarkerProjection> Biomarkers { get; set; } = new();
    public List<(string Category, double Likelihood, double[] Curve)> Alternatives { get; set; } = new();

    public record BiomarkerProjection(string Name, double Baseline, double M3, double M6, double M12, double Pct6, string Direction);
}

/// <summary>Two-assessment before/after comparison.</summary>
public class CompareViewModel
{
    public List<Option> Assessments { get; set; } = new();
    public int? AId { get; set; }
    public int? BId { get; set; }
    public Side? A { get; set; }
    public Side? B { get; set; }
    public double? IntegratedDelta { get; set; }
    public List<ZoneDelta> ZoneDeltas { get; set; } = new();
    public List<BiomarkerDelta> BiomarkerDeltas { get; set; } = new();

    public record Option(int Id, string Label);

    public class Side
    {
        public int AssessmentId { get; set; }
        public string SubjectCode { get; set; } = "";
        public string? Name { get; set; }
        public string TimePoint { get; set; } = "";
        public DateTime Date { get; set; }
        public int? ImageId { get; set; }
        public double? ImageOverall { get; set; }
        public double? Integrated { get; set; }
        public string? Panel { get; set; }
        public string? Phenotype { get; set; }
        public double? Concordance { get; set; }
        public double? RemodelIdx { get; set; }
        public double? HyalIdx { get; set; }
        public double? BarrierIdx { get; set; }
        public Dictionary<string, double?> Zones { get; set; } = new();
    }

    public record ZoneDelta(string Zone, double? A, double? B, double? Delta);
    public record BiomarkerDelta(string Name, double? A, double? B, double? PctChange);
}
