using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Analysis.Engine;
using MindAeonDermaAI.Analysis.Imaging;
using MindAeonDermaAI.Analysis.Models;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

public interface IAssessmentService
{
    /// <summary>Analyze every not-yet-analyzed image on the assessment via the vision model.</summary>
    Task<(int analyzed, int failed, string? error)> AnalyzeImagesAsync(int assessmentId, CancellationToken ct = default);

    /// <summary>Run the deterministic scoring engine and persist the ScoreResult + selections.</summary>
    Task<ScoreResult> RunAnalysisAsync(int assessmentId, CancellationToken ct = default);
}

/// <summary>
/// Application service that wires persisted entities to the pure analysis engine and image analyzer,
/// then stores the results. Keeps the engine free of EF concerns.
/// </summary>
public class AssessmentService : IAssessmentService
{
    private readonly AppDbContext _db;
    private readonly IAnalysisEngine _engine;
    private readonly IImageAnalyzer _imageAnalyzer;
    private readonly IWebHostEnvironment _env;

    public AssessmentService(AppDbContext db, IAnalysisEngine engine, IImageAnalyzer imageAnalyzer, IWebHostEnvironment env)
    {
        _db = db;
        _engine = engine;
        _imageAnalyzer = imageAnalyzer;
        _env = env;
    }

    public async Task<(int analyzed, int failed, string? error)> AnalyzeImagesAsync(int assessmentId, CancellationToken ct = default)
    {
        var images = await _db.SkinImages.Where(i => i.AssessmentId == assessmentId && !i.IsAnalyzed).ToListAsync(ct);
        int ok = 0, fail = 0; string? firstError = null;
        var allZones = new List<ZoneOutcome>();

        foreach (var img in images)
        {
            var path = Path.Combine(UploadRoot(), img.StoredFileName);
            if (!File.Exists(path)) { fail++; firstError ??= "Stored image file not found."; continue; }

            var bytes = await File.ReadAllBytesAsync(path, ct);
            var outcome = await _imageAnalyzer.AnalyzeAsync(bytes, img.ContentType ?? "image/jpeg", assessmentId, ct);
            if (!outcome.Success) { fail++; firstError ??= outcome.Error; continue; }

            var s = outcome.Scores;
            img.HydrationScore = s.Hydration;
            img.TextureSmoothnessScore = s.Texture;
            img.ElasticityScore = s.Elasticity;
            img.WrinkleBurdenScore = s.Wrinkle;
            img.PigmentationUniformityScore = s.Pigmentation;
            img.RednessHealthScore = s.RednessHealth;
            img.PoreVisibilityScore = s.Pore;
            img.BrightnessScore = s.Brightness;
            img.OverallSkinQualityScore = s.Overall;
            img.HasLightingArtifact = outcome.HasLightingArtifact;
            img.HasMakeup = outcome.HasMakeup;
            img.HasFilter = outcome.HasFilter;
            img.NonStandardAngle = outcome.NonStandardAngle;
            img.AnalysisNotes = outcome.Notes;
            img.IsAnalyzed = true;
            allZones.AddRange(outcome.Zones);
            ok++;
        }

        // Aggregate per-zone scores across images into the regional heatmap rows.
        if (allZones.Count > 0)
        {
            var zoneIds = await _db.FacialZones.Where(z => z.Code != null).ToDictionaryAsync(z => z.Code!, z => z.Id, ct);
            var existing = await _db.ZoneScores.Where(z => z.AssessmentId == assessmentId).ToListAsync(ct);
            foreach (var g in allZones.GroupBy(z => z.ZoneCode))
            {
                if (!zoneIds.TryGetValue(g.Key, out var fzId)) continue;
                double? Avg(Func<ZoneOutcome, double?> s)
                {
                    var v = g.Select(s).Where(x => x.HasValue).Select(x => x!.Value).ToList();
                    return v.Count > 0 ? Math.Round(v.Average(), 0) : (double?)null;
                }
                var row = existing.FirstOrDefault(z => z.FacialZoneId == fzId) ?? new ZoneScore { AssessmentId = assessmentId, FacialZoneId = fzId };
                row.Overall = Avg(z => z.Overall); row.Hydration = Avg(z => z.Hydration);
                row.Wrinkle = Avg(z => z.Wrinkle); row.RednessHealth = Avg(z => z.RednessHealth); row.Pigmentation = Avg(z => z.Pigmentation);
                if (row.Id == 0) _db.ZoneScores.Add(row);
            }
        }

        await _db.SaveChangesAsync(ct);
        return (ok, fail, firstError);
    }

    public async Task<ScoreResult> RunAnalysisAsync(int assessmentId, CancellationToken ct = default)
    {
        var assessment = await _db.Assessments
            .Include(a => a.Subject)
                .ThenInclude(s => s!.SmokingStatus)
            .Include(a => a.Subject).ThenInclude(s => s!.Sex)
            .Include(a => a.Subject).ThenInclude(s => s!.AlcoholUse)
            .Include(a => a.Subject).ThenInclude(s => s!.SleepQuality)
            .Include(a => a.Subject).ThenInclude(s => s!.StressLevel)
            .Include(a => a.Subject).ThenInclude(s => s!.UVExposureLevel)
            .Include(a => a.Measurements).ThenInclude(m => m.Biomarker).ThenInclude(b => b!.Domain)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == assessmentId, ct)
            ?? throw new InvalidOperationException("Assessment not found.");

        var input = await BuildInputAsync(assessment, ct);
        var result = _engine.Analyze(input);

        // Persist z-scores / percent-changes back onto the measurements.
        foreach (var m in assessment.Measurements)
        {
            if (m.Biomarker?.Code is { } code)
            {
                if (result.ZScores.TryGetValue(code, out var z)) m.ZScore = z;
                if (result.PercentChanges.TryGetValue(code, out var pc)) m.PercentChange = pc;
                if (result.FoldChanges.TryGetValue(code, out var fc)) m.FoldChange = fc;
                m.IsOutlier = result.OutlierCodes.Contains(code);
            }
            m.IsMissing = m.Value is null;
        }

        // Map selected panel + primary phenotype codes to entity ids.
        var panel = await _db.Panels.FirstOrDefaultAsync(p => p.Code == result.SelectedPanelCode, ct);
        assessment.SelectedPanelId = panel?.Id;

        if (result.PhenotypeCodes.Count > 0)
        {
            var primaryCode = result.PhenotypeCodes[0];
            var phenotype = await _db.PhenotypeTypes.FirstOrDefaultAsync(p => p.Code == primaryCode, ct);
            assessment.PrimaryPhenotypeId = phenotype?.Id;
        }

        var score = await _db.ScoreResults.FirstOrDefaultAsync(s => s.AssessmentId == assessmentId, ct);
        if (score is null)
        {
            score = new ScoreResult { AssessmentId = assessmentId };
            _db.ScoreResults.Add(score);
        }
        score.CollagenRemodelingNeedScore = result.CollagenRemodelingNeedScore;
        score.BarrierHydrationNeedScore = result.BarrierHydrationNeedScore;
        score.DeepEcmLaxityNeedScore = result.DeepEcmLaxityNeedScore;
        score.IsMixedPhenotype = result.IsMixedPhenotype;
        score.PanelScore = result.PanelScore;
        score.ImageQualityScore = result.ImageQualityScore;
        score.LifestyleScore = result.LifestyleScore;
        score.CohortPercentileScore = result.CohortPercentileScore;
        score.IntegratedSkinQualityScore = result.IntegratedSkinQualityScore;
        score.Confidence = result.Confidence;
        score.UsedRealCohort = result.UsedRealCohort;
        score.UsedSyntheticCohort = result.UsedSyntheticCohort;
        score.CohortSize = result.SyntheticCohortSize;
        score.PhenotypeCodesCsv = string.Join(",", result.PhenotypeCodes);
        score.ModelUsed = result.ModelUsed;
        score.ModelRationale = result.ModelRationale;

        // Advanced multimodal outputs.
        score.CrossModalConcordance = result.CrossModalConcordance;
        score.IsSubclinical = result.IsSubclinical;
        score.SubclinicalNote = result.SubclinicalNote;
        score.RemodelingBalanceIndex = result.RemodelingBalanceIndex;
        score.HyaluronanAxisIndex = result.HyaluronanAxisIndex;
        score.BarrierResilienceIndex = result.BarrierResilienceIndex;
        score.CounterfactualScore = result.CounterfactualScore;
        score.CounterfactualNote = result.CounterfactualNote;
        score.ExposomeBurden = result.ExposomeBurden;
        score.FaceAge = result.FaceAge; score.ProteinAge = result.ProteinAge; score.BarrierAge = result.BarrierAge;
        score.CollagenAge = result.CollagenAge; score.BiologicalSkinAge = result.BiologicalSkinAge;
        score.ProteinConfidence = result.ProteinConfidence; score.ImageConfidence = result.ImageConfidence;
        score.OverallConfidence = result.OverallConfidence;
        score.BenchmarkPercentile = result.BenchmarkPercentile; score.BenchmarkGroupLabel = result.BenchmarkGroupLabel;

        // Response likelihood → JSON with resolved category names, ranked high→low.
        if (result.ResponseLikelihood.Count > 0)
        {
            var catNames = await _db.InterventionCategories.Where(c => c.Code != null)
                .ToDictionaryAsync(c => c.Code!, c => c.Name, ct);
            var ranked = result.ResponseLikelihood
                .Select(kv => new { category = catNames.TryGetValue(kv.Key, out var n) ? n : kv.Key, likelihood = kv.Value })
                .OrderByDescending(x => x.likelihood).ToList();
            score.ResponseLikelihoodJson = JsonSerializer.Serialize(ranked);
        }

        // Aging velocity ("biological momentum") vs the most recent prior baseline.
        if (assessment.TimePoint == TimePointType.FollowUp)
        {
            var prior = await _db.Assessments
                .Where(x => x.SubjectId == assessment.SubjectId && x.TimePoint == TimePointType.Baseline
                            && x.Id != assessment.Id && x.ScoreResult != null && x.AssessmentDate <= assessment.AssessmentDate)
                .OrderByDescending(x => x.AssessmentDate)
                .Select(x => new { x.AssessmentDate, x.ScoreResult!.IntegratedSkinQualityScore })
                .FirstOrDefaultAsync(ct);
            if (prior is not null)
            {
                var days = Math.Max(1, (assessment.AssessmentDate - prior.AssessmentDate).TotalDays);
                // Positive = quality declining (worsening) per 30 days.
                score.AgingVelocity = Math.Round((prior.IntegratedSkinQualityScore - result.IntegratedSkinQualityScore) / days * 30.0, 1);
            }
        }

        assessment.Status = AssessmentStatus.Analyzed;
        await _db.SaveChangesAsync(ct);
        return score;
    }

    private async Task<AnalysisInput> BuildInputAsync(Assessment a, CancellationToken ct)
    {
        var subject = a.Subject!;
        var input = new AnalysisInput
        {
            Subject = new SubjectInput
            {
                Age = subject.Age,
                Bmi = subject.Bmi,
                SexCode = subject.Sex?.Code,
                SmokingCode = subject.SmokingStatus?.Code,
                AlcoholCode = subject.AlcoholUse?.Code,
                SleepCode = subject.SleepQuality?.Code,
                StressCode = subject.StressLevel?.Code,
                UvCode = subject.UVExposureLevel?.Code
            },
            IsFollowUp = a.TimePoint == TimePointType.FollowUp,
            ExposomeUvIndex = a.ExposomeUvIndex,
            ExposomePm25 = a.ExposomePm25,
            ExposomeHumidity = a.ExposomeHumidity
        };

        // Proteins with a stratified / self-calibrating reference:
        //  • if enough platform-cohort data exists, use the recalibrated cohort mean/sd;
        //  • otherwise use the young reference shifted by age (AgeAdjustPerDecade beyond age 30).
        var age = subject.Age;
        foreach (var m in a.Measurements)
        {
            var b = m.Biomarker!;
            double mean, sd;
            if (b.CohortRefN >= 8 && b.CohortRefMean is { } cm && b.CohortRefSd is { } csd && csd > 0)
            {
                mean = cm; sd = csd;
            }
            else
            {
                double decades = age is > 30 ? (age.Value - 30) / 10.0 : 0;
                mean = b.YoungRefMean + b.AgeAdjustPerDecade * decades;
                sd = b.YoungRefSd;
            }
            input.Proteins.Add(new ProteinInput
            {
                BiomarkerId = b.Id,
                Code = b.Code ?? b.Name,
                Name = b.Name,
                DomainCode = b.Domain?.Code ?? "",
                Need = b.NeedDirection,
                Value = m.Value,
                YoungMean = mean,
                YoungSd = sd
            });
        }

        // Follow-up: link the most recent prior Baseline assessment so fold/percent-change can be computed.
        if (input.IsFollowUp)
        {
            var baseline = await _db.Assessments
                .Where(x => x.SubjectId == a.SubjectId && x.TimePoint == TimePointType.Baseline
                            && x.AssessmentDate <= a.AssessmentDate && x.Id != a.Id)
                .OrderByDescending(x => x.AssessmentDate).ThenByDescending(x => x.Id)
                .Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (baseline != 0)
            {
                var baseValues = await _db.ProteinMeasurements
                    .Where(m => m.AssessmentId == baseline && m.Value != null)
                    .ToDictionaryAsync(m => m.BiomarkerId, m => m.Value!.Value, ct);
                foreach (var p in input.Proteins)
                    if (baseValues.TryGetValue(p.BiomarkerId, out var bv)) p.BaselineValue = bv;
            }
        }

        // Panel membership map (code + weight).
        var panelMembers = await _db.PanelBiomarkers
            .Include(pb => pb.Panel)
            .Include(pb => pb.Biomarker)
            .ToListAsync(ct);
        input.PanelMembers = panelMembers
            .GroupBy(pb => pb.Panel!.Code!)
            .ToDictionary(g => g.Key, g => g.Select(pb => (pb.Biomarker!.Code ?? pb.Biomarker!.Name, pb.Weight)).ToList());

        // Aggregate analyzed image scores (mean across images).
        var analyzed = a.Images.Where(i => i.IsAnalyzed).ToList();
        if (analyzed.Count > 0)
        {
            double? Avg(Func<SkinImage, double?> sel)
            {
                var vals = analyzed.Select(sel).Where(v => v.HasValue).Select(v => v!.Value).ToList();
                return vals.Count > 0 ? Math.Round(vals.Average(), 1) : null;
            }
            input.Image = new ImageScoreInput
            {
                Hydration = Avg(i => i.HydrationScore),
                Texture = Avg(i => i.TextureSmoothnessScore),
                Elasticity = Avg(i => i.ElasticityScore),
                Wrinkle = Avg(i => i.WrinkleBurdenScore),
                Pigmentation = Avg(i => i.PigmentationUniformityScore),
                RednessHealth = Avg(i => i.RednessHealthScore),
                Pore = Avg(i => i.PoreVisibilityScore),
                Brightness = Avg(i => i.BrightnessScore),
                Overall = Avg(i => i.OverallSkinQualityScore),
                // Distinct QC flag types raised across analyzed images (0–4): makeup / filter / lighting / angle.
                QcFlagCount = (analyzed.Any(i => i.HasMakeup) ? 1 : 0) + (analyzed.Any(i => i.HasFilter) ? 1 : 0)
                            + (analyzed.Any(i => i.HasLightingArtifact) ? 1 : 0) + (analyzed.Any(i => i.NonStandardAngle) ? 1 : 0)
            };
        }

        return input;
    }

    private string UploadRoot()
    {
        var root = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(root);
        return root;
    }
}
