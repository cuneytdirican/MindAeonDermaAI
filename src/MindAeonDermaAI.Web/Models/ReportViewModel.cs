using MindAeonDermaAI.Core.Entities;

namespace MindAeonDermaAI.Web.Models;

/// <summary>Everything the report view (and PDF) needs, assembled from persisted entities.</summary>
public class ReportViewModel
{
    public Assessment Assessment { get; set; } = default!;
    public Subject Subject { get; set; } = default!;
    public ScoreResult Score { get; set; } = default!;
    public ReportDocument? Document { get; set; }

    public Panel? SelectedPanel { get; set; }
    public FacialGeometry? FacialGeometry { get; set; }
    public string PrimaryPhenotypeName { get; set; } = "";
    public List<string> PhenotypeNames { get; set; } = new();

    public List<BiomarkerRow> Biomarkers { get; set; } = new();
    public List<ImageScoreRow> ImageScores { get; set; } = new();
    public List<PanelNeedRow> PanelNeeds { get; set; } = new();

    // Advanced multimodal surfaces.
    public List<ZoneRow> Zones { get; set; } = new();
    public List<ResponseRow> ResponseLikelihood { get; set; } = new();
    public List<InstrumentRow> Instruments { get; set; } = new();
    /// <summary>Digital-twin 12-week what-if projections for the top intervention categories.</summary>
    public List<ProjectionSeries> Projections { get; set; } = new();

    public bool HasImages { get; set; }
    public bool UsedAi { get; set; }
    public string BrandName { get; set; } = "MindAeon Dermalytics™";
    public string BrandColor { get; set; } = "#0B2340";

    public record ZoneRow(string Zone, double? Overall, double? Wrinkle, double? RednessHealth, double? Hydration, double? Pigmentation);
    public record ResponseRow(string Category, double Likelihood);
    public record InstrumentRow(string Name, double Value);
    public record ProjectionSeries(string Category, double Likelihood, double[] Weekly);

    public const string Disclaimer =
        "This report is intended for research-use and cosmetic skin biology interpretation only. It is not a " +
        "medical diagnosis, does not replace dermatological examination, and should be interpreted together with " +
        "standardized imaging, validated assays, clinical evaluation, and longitudinal follow-up.";

    public record BiomarkerRow(string Name, string Domain, double? Value, string Unit, double? ZScore,
        double? PercentChange, string Direction, string Interpretation);

    public record ImageScoreRow(string Metric, double? Score);

    public record PanelNeedRow(string Panel, double NeedScore, bool Selected);
}

/// <summary>Narrative section keys stored in ReportDocument.SectionsJson.</summary>
public class ReportSections
{
    public string? ExecutiveSummary { get; set; }
    public string? SubjectProfileNote { get; set; }
    public string? ImageAssessmentNote { get; set; }
    public string? ProteinAnalysisNote { get; set; }
    public string? CohortNote { get; set; }
    public string? ModelExplanation { get; set; }
    public string? PanelSelectionNote { get; set; }
    public string? PathwayInterpretation { get; set; }
    public string? PersonalizedInterpretation { get; set; }
    public string? ReasoningSummary { get; set; }
    public string? Limitations { get; set; }
}
