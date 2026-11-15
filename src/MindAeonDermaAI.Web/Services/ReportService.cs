using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Analysis.Engine;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Models;

namespace MindAeonDermaAI.Web.Services;

public interface IReportService
{
    Task<ReportViewModel> BuildViewModelAsync(int assessmentId, CancellationToken ct = default);
    Task<ReportDocument> GenerateAsync(int assessmentId, CancellationToken ct = default);
    Task<byte[]> GeneratePdfAsync(int assessmentId, CancellationToken ct = default);
}

/// <summary>
/// Assembles the report view model, generates the empathetic-but-objective narrative (via the configured
/// LLM, with a deterministic fallback so the demo always produces a report), and renders the PDF.
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IAiOrchestrator _ai;
    private readonly IBrandingService _branding;
    private readonly IWebhookDispatcher _webhooks;
    private readonly IAuditLogger _audit;

    public ReportService(AppDbContext db, IAiOrchestrator ai, IBrandingService branding, IWebhookDispatcher webhooks, IAuditLogger audit)
    {
        _db = db;
        _ai = ai;
        _branding = branding;
        _webhooks = webhooks;
        _audit = audit;
    }

    public async Task<ReportViewModel> BuildViewModelAsync(int assessmentId, CancellationToken ct = default)
    {
        var a = await LoadAsync(assessmentId, ct);
        var score = a.ScoreResult ?? throw new InvalidOperationException("Assessment has not been analyzed yet.");
        if (a.Subject!.Anonymized) a.Subject.DisplayName = null; // pseudonymized: mask the display name (code still identifies)

        var vm = new ReportViewModel
        {
            Assessment = a,
            Subject = a.Subject!,
            Score = score,
            Document = a.Report,
            SelectedPanel = a.SelectedPanel,
            FacialGeometry = a.FacialGeometry,
            HasImages = a.Images.Any(i => i.IsAnalyzed),
            UsedAi = a.Report is { GeneratedByModel: not null }
        };

        var brand = await _branding.GetAsync(ct);
        vm.BrandName = brand.ProductName;
        vm.BrandColor = brand.SecondaryColor;

        vm.PrimaryPhenotypeName = a.PrimaryPhenotype?.Name ?? "Not determined";

        // Biomarker rows.
        foreach (var m in a.Measurements.OrderBy(m => m.Biomarker!.Domain!.DisplayOrder).ThenBy(m => m.Biomarker!.Name))
        {
            var b = m.Biomarker!;
            vm.Biomarkers.Add(new ReportViewModel.BiomarkerRow(
                b.Name, b.Domain?.Name ?? "", m.Value, b.Unit?.Name ?? "AU", m.ZScore, m.PercentChange,
                DirectionLabel(b.NeedDirection), InterpretMarker(m.ZScore, b.NeedDirection)));
        }

        // Image score rows.
        if (vm.HasImages)
        {
            var imgs = a.Images.Where(i => i.IsAnalyzed).ToList();
            double? Avg(Func<SkinImage, double?> s)
            {
                var v = imgs.Select(s).Where(x => x.HasValue).Select(x => x!.Value).ToList();
                return v.Count > 0 ? Math.Round(v.Average(), 0) : null;
            }
            vm.ImageScores.Add(new("Hydration appearance", Avg(i => i.HydrationScore)));
            vm.ImageScores.Add(new("Texture smoothness", Avg(i => i.TextureSmoothnessScore)));
            vm.ImageScores.Add(new("Elasticity / firmness", Avg(i => i.ElasticityScore)));
            vm.ImageScores.Add(new("Wrinkle smoothness", Avg(i => i.WrinkleBurdenScore)));
            vm.ImageScores.Add(new("Pigmentation uniformity", Avg(i => i.PigmentationUniformityScore)));
            vm.ImageScores.Add(new("Redness (health)", Avg(i => i.RednessHealthScore)));
            vm.ImageScores.Add(new("Pore refinement", Avg(i => i.PoreVisibilityScore)));
            vm.ImageScores.Add(new("Brightness", Avg(i => i.BrightnessScore)));
            vm.ImageScores.Add(new("Overall skin quality", Avg(i => i.OverallSkinQualityScore)));
        }

        // Panel need rows.
        var panels = await _db.Panels.AsNoTracking().ToListAsync(ct);
        (string code, double s)[] needs =
        {
            (PanelCodes.Collagen, score.CollagenRemodelingNeedScore),
            (PanelCodes.Barrier, score.BarrierHydrationNeedScore),
            (PanelCodes.DeepEcm, score.DeepEcmLaxityNeedScore)
        };
        foreach (var (code, s) in needs.OrderByDescending(n => n.s))
        {
            var name = panels.FirstOrDefault(p => p.Code == code)?.Name ?? code;
            vm.PanelNeeds.Add(new ReportViewModel.PanelNeedRow(name, Math.Round(s, 1), code == a.SelectedPanel?.Code));
        }

        // Regional zone heatmap rows.
        foreach (var z in a.ZoneScores.OrderBy(z => z.FacialZone!.DisplayOrder))
            vm.Zones.Add(new ReportViewModel.ZoneRow(z.FacialZone?.Name ?? "", z.Overall, z.Wrinkle, z.RednessHealth, z.Hydration, z.Pigmentation));

        // Instrumental / multi-omics readings.
        foreach (var r in a.InstrumentalReadings.OrderBy(r => r.InstrumentType!.DisplayOrder))
            vm.Instruments.Add(new ReportViewModel.InstrumentRow(r.InstrumentType?.Name ?? "", r.Value));

        // Intervention-response likelihood (already ranked high→low in JSON).
        if (!string.IsNullOrWhiteSpace(score.ResponseLikelihoodJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(score.ResponseLikelihoodJson);
                foreach (var el in doc.RootElement.EnumerateArray())
                    vm.ResponseLikelihood.Add(new ReportViewModel.ResponseRow(
                        el.GetProperty("category").GetString() ?? "",
                        el.GetProperty("likelihood").GetDouble()));
            }
            catch { }
        }

        // Digital-twin 12-week what-if projection for the top intervention categories.
        foreach (var r in vm.ResponseLikelihood.Take(3))
            vm.Projections.Add(new ReportViewModel.ProjectionSeries(
                r.Category, r.Likelihood,
                Analysis.Engine.Projection.Trajectory(score.IntegratedSkinQualityScore, r.Likelihood)));

        // Resolve all classified phenotypes (one or more), not just the primary.
        if (!string.IsNullOrWhiteSpace(score.PhenotypeCodesCsv))
        {
            var codes = score.PhenotypeCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var names = await _db.PhenotypeTypes.AsNoTracking()
                .Where(p => codes.Contains(p.Code))
                .ToDictionaryAsync(p => p.Code!, p => p.Name, ct);
            vm.PhenotypeNames = codes.Select(c => names.TryGetValue(c, out var n) ? n : c).ToList();
        }
        else
        {
            vm.PhenotypeNames.Add(vm.PrimaryPhenotypeName);
        }
        return vm;
    }

    public async Task<ReportDocument> GenerateAsync(int assessmentId, CancellationToken ct = default)
    {
        var a = await LoadAsync(assessmentId, ct);
        var score = a.ScoreResult ?? throw new InvalidOperationException("Assessment must be analyzed before reporting.");

        var dataSummary = BuildDataSummary(a, score);
        ReportSections sections;
        string? provider = null, model = null;

        var aiResult = await _ai.ChatAsync(new ChatRequest
        {
            Messages =
            {
                new AiMessage("system", ReportPrompts.System),
                new AiMessage("user", dataSummary)
            },
            JsonMode = true,
            Temperature = 0.4,
            MaxTokens = 3500
        }, assessmentId: assessmentId, ct: ct);

        if (aiResult.Success && TryParseSections(aiResult.Content, out var parsed))
        {
            sections = parsed;
            provider = aiResult.ProviderName;
            model = aiResult.Model;
        }
        else
        {
            sections = BuildFallbackSections(a, score); // deterministic — demo always yields a report
        }

        var doc = a.Report ?? new ReportDocument { AssessmentId = assessmentId };
        doc.SectionsJson = JsonSerializer.Serialize(sections);
        doc.ExecutiveSummary = sections.ExecutiveSummary;
        doc.PersonalizedInterpretation = sections.PersonalizedInterpretation;
        doc.PhenotypeClassificationText = a.PrimaryPhenotype?.Name;
        doc.ReasoningSummary = sections.ReasoningSummary;
        doc.Limitations = sections.Limitations;
        doc.UsedSyntheticData = score.UsedSyntheticCohort;
        doc.GeneratedByProvider = provider;
        doc.GeneratedByModel = model;

        if (a.Report is null) _db.ReportDocuments.Add(doc);
        a.Status = AssessmentStatus.Reported;
        await _db.SaveChangesAsync(ct);

        await _webhooks.DispatchAsync("report.generated",
            new { assessmentId, subjectCode = a.Subject?.Code, integrated = score.IntegratedSkinQualityScore }, ct);
        await _audit.LogAsync("report.generated", "Assessment", assessmentId, $"Subject {a.Subject?.Code}, integrated {score.IntegratedSkinQualityScore:0}", ct);
        return doc;
    }

    public async Task<byte[]> GeneratePdfAsync(int assessmentId, CancellationToken ct = default)
    {
        var vm = await BuildViewModelAsync(assessmentId, ct);
        return ReportPdf.Build(vm);
    }

    // ---------- helpers ----------

    private Task<Assessment> LoadAsync(int assessmentId, CancellationToken ct) =>
        _db.Assessments
            .Include(a => a.Subject).ThenInclude(s => s!.Sex)
            .Include(a => a.Subject).ThenInclude(s => s!.SkinType)
            .Include(a => a.Subject).ThenInclude(s => s!.FitzpatrickType)
            .Include(a => a.Subject).ThenInclude(s => s!.SmokingStatus)
            .Include(a => a.Subject).ThenInclude(s => s!.AlcoholUse)
            .Include(a => a.Subject).ThenInclude(s => s!.SleepQuality)
            .Include(a => a.Subject).ThenInclude(s => s!.StressLevel)
            .Include(a => a.Subject).ThenInclude(s => s!.UVExposureLevel)
            .Include(a => a.Subject).ThenInclude(s => s!.MenopauseStatus)
            .Include(a => a.InterventionCategory)
            .Include(a => a.SelectedPanel)
            .Include(a => a.PrimaryPhenotype)
            .Include(a => a.ScoreResult)
            .Include(a => a.Report)
            .Include(a => a.Images)
            .Include(a => a.ZoneScores).ThenInclude(z => z.FacialZone)
            .Include(a => a.FacialGeometry)
            .Include(a => a.InstrumentalReadings).ThenInclude(r => r.InstrumentType)
            .Include(a => a.Measurements).ThenInclude(m => m.Biomarker).ThenInclude(b => b!.Domain)
            .Include(a => a.Measurements).ThenInclude(m => m.Biomarker).ThenInclude(b => b!.Unit)
            .FirstOrDefaultAsync(a => a.Id == assessmentId, ct)
            .ContinueWith(t => t.Result ?? throw new InvalidOperationException("Assessment not found."), ct);

    private static string BuildDataSummary(Assessment a, ScoreResult s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Produce the report JSON from these already-computed results.");
        var subj = a.Subject!;
        sb.AppendLine($"Subject: age={subj.Age}, sex={subj.Sex?.Name}, BMI={subj.Bmi:0.0}, skinType={subj.SkinType?.Name}, " +
                      $"fitzpatrick={subj.FitzpatrickType?.Name}, smoking={subj.SmokingStatus?.Name}, alcohol={subj.AlcoholUse?.Name}, " +
                      $"sleep={subj.SleepQuality?.Name}, stress={subj.StressLevel?.Name}, UV={subj.UVExposureLevel?.Name}, " +
                      $"menopause={subj.MenopauseStatus?.Name}.");
        sb.AppendLine($"Intervention category: {a.InterventionCategory?.Name}. Timepoint: {a.TimePoint}.");
        sb.AppendLine($"Panel need scores — Collagen Remodeling: {s.CollagenRemodelingNeedScore:0.0}, " +
                      $"Barrier/Hydration: {s.BarrierHydrationNeedScore:0.0}, Deep ECM/Laxity: {s.DeepEcmLaxityNeedScore:0.0}.");
        sb.AppendLine($"Selected panel: {a.SelectedPanel?.Name}. Mixed phenotype: {s.IsMixedPhenotype}.");
        sb.AppendLine($"Component scores — Panel: {s.PanelScore:0.0}, Image: {s.ImageQualityScore:0.0}, " +
                      $"Lifestyle: {s.LifestyleScore:0.0}. Integrated skin-quality score: {s.IntegratedSkinQualityScore:0.0}/100.");
        sb.AppendLine($"Primary phenotype: {a.PrimaryPhenotype?.Name}.");
        sb.AppendLine($"Model used: {s.ModelUsed}. Rationale: {s.ModelRationale}");
        sb.AppendLine($"Reference type: {(s.UsedRealCohort ? "real cohort" : "synthetic literature-inspired reference (simulated, not a validated clinical range)")}.");
        if (s.CohortPercentileScore.HasValue)
            sb.AppendLine($"Reference-cohort percentile: {s.CohortPercentileScore:0}th of {s.CohortSize} (synthetic).");
        sb.AppendLine("Biomarkers (name, z-score vs young reference):");
        foreach (var m in a.Measurements.Where(m => m.Value.HasValue))
            sb.AppendLine($"  - {m.Biomarker?.Name}: value={m.Value}, z={m.ZScore:0.00}");
        return sb.ToString();
    }

    private static bool TryParseSections(string content, out ReportSections sections)
    {
        sections = new ReportSections();
        try
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start < 0 || end <= start) return false;
            var json = content[start..(end + 1)];
            var parsed = JsonSerializer.Deserialize<ReportSections>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.ExecutiveSummary)) return false;
            sections = parsed;
            return true;
        }
        catch { return false; }
    }

    private static ReportSections BuildFallbackSections(Assessment a, ScoreResult s)
    {
        var panel = a.SelectedPanel?.Name ?? "the selected panel";
        var phenotype = a.PrimaryPhenotype?.Name ?? "a mixed phenotype";
        string refNote = s.UsedSyntheticCohort
            ? "A synthetic, literature-inspired reference cohort was used for demonstration and hypothesis generation only; these are simulated values and not validated clinical reference ranges."
            : "A provided reference cohort was used as the primary comparison.";

        return new ReportSections
        {
            ExecutiveSummary =
                $"This research-use report integrates demographic, lifestyle, protein-biomarker and image-derived signals. " +
                $"The analysis suggests that the profile is most compatible with {panel.ToLowerInvariant()} need " +
                $"(integrated skin-quality score {s.IntegratedSkinQualityScore:0}/100). This does not indicate a disease; " +
                $"it highlights which biological layer may benefit from closer monitoring.",
            SubjectProfileNote =
                "The subject profile summarizes demographic and lifestyle context that should be interpreted together with the biomarker and image findings.",
            ImageAssessmentNote = s.ImageQualityScore >= 0
                ? "Image-derived appearance scores are standardized (0–100) and reflect visible skin-quality attributes only; they should be interpreted together with standardized imaging."
                : null,
            ProteinAnalysisNote =
                $"Protein biomarkers were normalized as robust z-scores against the young reference. {refNote}",
            CohortNote = refNote,
            ModelExplanation = s.ModelRationale,
            PanelSelectionNote =
                $"Panel need scores were compared (Collagen {s.CollagenRemodelingNeedScore:0}, Barrier/Hydration {s.BarrierHydrationNeedScore:0}, " +
                $"Deep ECM/Laxity {s.DeepEcmLaxityNeedScore:0}). {panel} showed the highest biological need" +
                (s.IsMixedPhenotype ? ", though the two leading panels were within 10 points, consistent with a mixed phenotype." : "."),
            PathwayInterpretation =
                $"The findings appear consistent with {phenotype.ToLowerInvariant()} and should be interpreted cautiously and together with clinical evaluation.",
            PersonalizedInterpretation =
                $"Your results suggest that your skin profile is mainly compatible with {panel.ToLowerInvariant()}. " +
                "This is a supportive, non-diagnostic observation that points to the biological layer which may benefit " +
                "from closer attention and longitudinal follow-up.",
            ReasoningSummary =
                $"{panel} was prioritized because its weighted biomarker deviations from the young reference were the largest, " +
                $"and the image and lifestyle components were combined into the integrated score using the pre-specified weighting.",
            Limitations =
                "This analysis is limited by assay platform variability, image standardization, potential confounders, and — where noted — " +
                "the use of a synthetic reference. Results are hypothesis-generating and not diagnostic."
        };
    }

    private static string DirectionLabel(NeedDirection d) => d switch
    {
        NeedDirection.LowIsNeed => "Lower ⟶ more need",
        NeedDirection.HighIsNeed => "Higher ⟶ more need",
        NeedDirection.DysregulationIsNeed => "Deviation ⟶ more need",
        _ => ""
    };

    private static string InterpretMarker(double? z, NeedDirection d)
    {
        if (z is null) return "Not measured";
        double v = z.Value;
        string level = Math.Abs(v) < 0.5 ? "within" : Math.Abs(v) < 1.5 ? "modestly outside" : "clearly outside";
        string dir = v < 0 ? "below" : "above";
        return $"{level} young reference ({dir})";
    }
}
