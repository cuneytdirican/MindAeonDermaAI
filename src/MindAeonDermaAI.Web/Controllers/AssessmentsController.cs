using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Analysis.Engine;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Infrastructure.Identity;
using MindAeonDermaAI.Web.Models;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

public class AssessmentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAssessmentService _assessmentService;
    private readonly IReportService _reportService;
    private readonly IAiOrchestrator _ai;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ExampleReportBuilder _example;

    public AssessmentsController(AppDbContext db, IAssessmentService assessmentService, IReportService reportService,
        IAiOrchestrator ai, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, ExampleReportBuilder example)
    {
        _db = db;
        _assessmentService = assessmentService;
        _reportService = reportService;
        _ai = ai;
        _env = env;
        _userManager = userManager;
        _example = example;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rows = await _db.Assessments
            .Include(a => a.Subject).Include(a => a.SelectedPanel).Include(a => a.ScoreResult)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new AssessmentListRow
            {
                Id = a.Id,
                SubjectCode = a.Subject!.Code,
                SubjectName = a.Subject.DisplayName,
                Status = a.Status.ToString(),
                TimePoint = a.TimePoint.ToString(),
                Panel = a.SelectedPanel != null ? a.SelectedPanel.Name : null,
                Score = a.ScoreResult != null ? a.ScoreResult.IntegratedSkinQualityScore : (double?)null,
                Created = a.CreatedAtUtc
            }).ToListAsync(ct);
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? subjectId, CancellationToken ct)
    {
        var vm = await BuildCreateVmAsync(ct);
        if (subjectId is not null && vm.Subjects.Any(s => s.Id == subjectId))
            vm.SubjectId = subjectId.Value;
        if (vm.Subjects.Count == 0)
        {
            TempData["Err"] = "Please create a subject first.";
            return RedirectToAction("Create", "Subjects");
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssessmentCreateViewModel model, Dictionary<int, double?>? values,
        List<IFormFile>? images, List<int?>? instrumentTypeIds, List<double?>? instrumentValues, CancellationToken ct)
    {
        if (!await _db.Subjects.AnyAsync(s => s.Id == model.SubjectId, ct))
        {
            ModelState.AddModelError(nameof(model.SubjectId), "Please select a subject.");
        }
        if (!ModelState.IsValid)
        {
            var vm = await BuildCreateVmAsync(ct);
            vm.SubjectId = model.SubjectId;
            vm.TimePoint = model.TimePoint;
            return View(vm);
        }

        var assessment = new Assessment
        {
            SubjectId = model.SubjectId,
            InterventionCategoryId = model.InterventionCategoryId,
            TimePoint = model.TimePoint,
            Status = AssessmentStatus.DataEntry,
            AssayBatch = string.IsNullOrWhiteSpace(model.AssayBatch) ? null : model.AssayBatch.Trim(),
            ExposomeUvIndex = model.ExposomeUvIndex,
            ExposomePm25 = model.ExposomePm25,
            ExposomeHumidity = model.ExposomeHumidity,
            CreatedByUserId = _userManager.GetUserId(User)
        };
        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync(ct);

        // Protein measurements (blank = not measured / missing).
        if (values is not null)
        {
            foreach (var (biomarkerId, value) in values)
            {
                if (!await _db.ProteinBiomarkers.AnyAsync(b => b.Id == biomarkerId, ct)) continue;
                var bio = await _db.ProteinBiomarkers.FindAsync(new object[] { biomarkerId }, ct);
                _db.ProteinMeasurements.Add(new ProteinMeasurement
                {
                    AssessmentId = assessment.Id,
                    BiomarkerId = biomarkerId,
                    Value = value,
                    UnitId = bio?.UnitId,
                    IsMissing = value is null
                });
            }
        }

        // Image uploads.
        if (images is not null)
        {
            var dir = UploadRoot();
            foreach (var file in images.Where(f => f.Length > 0))
            {
                var ext = Path.GetExtension(file.FileName);
                var name = $"{assessment.Id}_{Guid.NewGuid():N}{ext}";
                var path = Path.Combine(dir, name);
                await using (var fs = System.IO.File.Create(path)) await file.CopyToAsync(fs, ct);
                _db.SkinImages.Add(new SkinImage
                {
                    AssessmentId = assessment.Id,
                    StoredFileName = name,
                    ContentType = file.ContentType
                });
            }
        }

        // Instrumental / multi-omics readings (paired arrays; keep rows with both a type and a value).
        if (instrumentTypeIds is not null && instrumentValues is not null)
        {
            for (int i = 0; i < instrumentTypeIds.Count && i < instrumentValues.Count; i++)
            {
                var typeId = instrumentTypeIds[i];
                var val = instrumentValues[i];
                if (typeId is null or 0 || val is null) continue;
                if (!await _db.InstrumentTypes.AnyAsync(t => t.Id == typeId, ct)) continue;
                _db.InstrumentalReadings.Add(new InstrumentalReading
                {
                    AssessmentId = assessment.Id, InstrumentTypeId = typeId.Value, Value = val.Value
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = "Assessment created. Analyze images and run the analysis to generate the report.";
        return RedirectToAction(nameof(Details), new { id = assessment.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var a = await _db.Assessments
            .Include(x => x.Subject)
            .Include(x => x.InterventionCategory)
            .Include(x => x.SelectedPanel)
            .Include(x => x.PrimaryPhenotype)
            .Include(x => x.ScoreResult)
            .Include(x => x.Images)
            .Include(x => x.ZoneScores)
            .Include(x => x.InstrumentalReadings).ThenInclude(r => r.InstrumentType)
            .Include(x => x.Measurements).ThenInclude(m => m.Biomarker).ThenInclude(b => b!.Domain)
            .Include(x => x.Measurements).ThenInclude(m => m.Biomarker).ThenInclude(b => b!.Unit)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound();

        var vm = new AssessmentDetailsViewModel
        {
            Id = a.Id,
            SubjectCode = a.Subject!.Code,
            SubjectName = a.Subject.DisplayName,
            TimePoint = a.TimePoint.ToString(),
            InterventionCategory = a.InterventionCategory?.Name,
            Status = a.Status,
            Created = a.CreatedAtUtc,
            HasScore = a.ScoreResult != null,
            IntegratedScore = a.ScoreResult?.IntegratedSkinQualityScore,
            SelectedPanel = a.SelectedPanel?.Name,
            Phenotype = a.PrimaryPhenotype?.Name,
            AiReady = await _ai.HasUsableProviderAsync(AiTaskKind.Vision, ct),
            AnalyzedImageCount = a.Images.Count(i => i.IsAnalyzed),
            AssayBatch = a.AssayBatch,
            ExposomeUvIndex = a.ExposomeUvIndex,
            ExposomePm25 = a.ExposomePm25,
            ExposomeHumidity = a.ExposomeHumidity,
            ZoneCount = a.ZoneScores.Count,
            Instruments = a.InstrumentalReadings
                .Select(r => new AssessmentDetailsViewModel.InstrumentItem(r.InstrumentType?.Name ?? "", r.Value)).ToList(),
            Images = a.Images.Select(i => new AssessmentDetailsViewModel.ImageItem(
                i.Id, i.IsAnalyzed, i.OverallSkinQualityScore, i.AnalysisNotes,
                i.HasMakeup, i.HasFilter, i.HasLightingArtifact, i.NonStandardAngle)).ToList(),
            Measurements = a.Measurements
                .OrderBy(m => m.Biomarker!.Domain!.DisplayOrder).ThenBy(m => m.Biomarker!.Name)
                .Select(m => new AssessmentDetailsViewModel.MeasurementItem(
                    m.Biomarker!.Name, m.Biomarker.Domain?.Name ?? "", m.Value, m.Biomarker.Unit?.Name ?? "AU", m.ZScore))
                .ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AnalyzeImages(int id, CancellationToken ct)
    {
        var (ok, fail, error) = await _assessmentService.AnalyzeImagesAsync(id, ct);
        if (ok > 0) TempData["Ok"] = $"Analyzed {ok} image(s).";
        if (fail > 0) TempData["Err"] = $"{fail} image(s) could not be analyzed. {error}";
        if (ok == 0 && fail == 0) TempData["Ok"] = "No pending images to analyze.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RunAnalysis(int id, CancellationToken ct)
    {
        await _assessmentService.RunAnalysisAsync(id, ct);
        TempData["Ok"] = "Analysis complete. You can now generate the report.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int id, CancellationToken ct)
    {
        var a = await _db.Assessments.Include(x => x.ScoreResult).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return NotFound();
        if (a.ScoreResult is null) await _assessmentService.RunAnalysisAsync(id, ct);
        await _reportService.GenerateAsync(id, ct);
        return RedirectToAction(nameof(Report), new { id });
    }

    public async Task<IActionResult> Report(int id, CancellationToken ct)
    {
        try
        {
            var vm = await _reportService.BuildViewModelAsync(id, ct);
            return View(vm);
        }
        catch (InvalidOperationException)
        {
            TempData["Err"] = "This assessment has not been analyzed yet.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public async Task<IActionResult> Pdf(int id, CancellationToken ct)
    {
        var bytes = await _reportService.GeneratePdfAsync(id, ct);
        return File(bytes, "application/pdf", $"MindAeon-DermaAI-Report-{id}.pdf");
    }

    /// <summary>Streams a stored upload (files live outside wwwroot in App_Data/uploads).</summary>
    public async Task<IActionResult> Image(int id, CancellationToken ct)
    {
        var img = await _db.SkinImages.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (img is null) return NotFound();
        var path = Path.Combine(UploadRoot(), img.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        var bytes = await System.IO.File.ReadAllBytesAsync(path, ct);
        return File(bytes, img.ContentType ?? "image/jpeg");
    }

    /// <summary>Digital-twin: projected integrated score + per-biomarker trajectory at 3/6/12 months.</summary>
    public async Task<IActionResult> DigitalTwin(int id, CancellationToken ct)
    {
        var a = await _db.Assessments
            .Include(x => x.Subject).Include(x => x.ScoreResult)
            .Include(x => x.Measurements).ThenInclude(m => m.Biomarker)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a?.ScoreResult is null) { TempData["Err"] = "Run the analysis first."; return RedirectToAction(nameof(Details), new { id }); }

        var vm = new DigitalTwinViewModel { AssessmentId = id, SubjectCode = a.Subject!.Code, Baseline = a.ScoreResult.IntegratedSkinQualityScore };

        // Response likelihoods (ranked) drive the projection.
        var ranked = new List<(string Category, double Likelihood)>();
        if (a.ScoreResult.ResponseLikelihoodJson is { } j)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(j);
                foreach (var el in doc.RootElement.EnumerateArray())
                    ranked.Add((el.GetProperty("category").GetString() ?? "", el.GetProperty("likelihood").GetDouble()));
            }
            catch { }
        }
        var top = ranked.FirstOrDefault();
        vm.TopTreatment = top.Category ?? "supportive care";
        vm.TopLikelihood = top.Likelihood;
        vm.Integrated = Projection.TrajectoryMonths(vm.Baseline, vm.TopLikelihood, vm.Months);
        foreach (var alt in ranked.Take(3))
            vm.Alternatives.Add((alt.Category, alt.Likelihood, Projection.TrajectoryMonths(vm.Baseline, alt.Likelihood, vm.Months)));

        // Top biomarkers driving need → per-biomarker projection under the top treatment.
        var drivers = a.Measurements.Where(m => m.Value.HasValue && m.Biomarker != null && m.ZScore.HasValue)
            .Select(m => new { m, drive = NeedDrive(m.ZScore!.Value, m.Biomarker!.NeedDirection) })
            .Where(x => x.drive > 0.4).OrderByDescending(x => x.drive).Take(5).ToList();
        foreach (var d in drivers)
        {
            var need = d.m.Biomarker!.NeedDirection;
            double bv = d.m.Value!.Value;
            var m3 = Projection.BiomarkerTrajectory(bv, need, vm.TopLikelihood, 3);
            var m6 = Projection.BiomarkerTrajectory(bv, need, vm.TopLikelihood, 6);
            var m12 = Projection.BiomarkerTrajectory(bv, need, vm.TopLikelihood, 12);
            vm.Biomarkers.Add(new DigitalTwinViewModel.BiomarkerProjection(
                d.m.Biomarker!.Name, bv, m3.Projected, m6.Projected, m12.Projected, m6.PercentChange,
                need == Core.Enums.NeedDirection.LowIsNeed ? "↑ toward reference" : need == Core.Enums.NeedDirection.HighIsNeed ? "↓ toward reference" : "→ normalizing"));
        }
        return View(vm);
    }

    /// <summary>Treatment simulation: compare projected outcomes of candidate interventions.</summary>
    public async Task<IActionResult> Simulate(int id, CancellationToken ct)
    {
        var a = await _db.Assessments
            .Include(x => x.Subject).Include(x => x.ScoreResult)
            .Include(x => x.Measurements).ThenInclude(m => m.Biomarker)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a?.ScoreResult is null) { TempData["Err"] = "Run the analysis first."; return RedirectToAction(nameof(Details), new { id }); }

        var markers = a.Measurements
            .Where(m => m.Value.HasValue && m.ZScore.HasValue && m.Biomarker != null)
            .Select(m => new TreatmentSimulator.MarkerState(
                m.Biomarker!.Code ?? m.Biomarker.Name, m.Biomarker.Name, m.ZScore!.Value, m.Biomarker.NeedDirection, m.Value!.Value))
            .ToList();

        var vm = new SimulateViewModel
        {
            AssessmentId = id,
            SubjectCode = a.Subject!.Code,
            Baseline = a.ScoreResult.IntegratedSkinQualityScore
        };
        vm.Outcomes = TreatmentSimulator.Simulate(vm.Baseline, markers, vm.Months);
        return View(vm);
    }

    private static double NeedDrive(double z, Core.Enums.NeedDirection need) => need switch
    {
        Core.Enums.NeedDirection.LowIsNeed => Math.Max(0, -z),
        Core.Enums.NeedDirection.HighIsNeed => Math.Max(0, z),
        _ => Math.Max(0, Math.Abs(z) - 0.5)
    };

    /// <summary>Before/after comparison of two assessments (image morph slider + score/biomarker deltas).</summary>
    public async Task<IActionResult> Compare(int? a, int? b, CancellationToken ct)
    {
        var vm = new CompareViewModel
        {
            Assessments = await _db.Assessments.Include(x => x.Subject)
                .OrderBy(x => x.Subject!.Code).ThenBy(x => x.TimePoint)
                .Select(x => new CompareViewModel.Option(x.Id,
                    x.Subject!.Code + (x.Subject.DisplayName != null ? " · " + x.Subject.DisplayName : "") + " [" + x.TimePoint + "]"))
                .ToListAsync(ct)
        };

        // Default to the first subject that has two timepoints (a real before/after).
        if (a is null || b is null)
        {
            var pair = await _db.Assessments
                .Where(x => _db.Assessments.Count(y => y.SubjectId == x.SubjectId) >= 2)
                .OrderBy(x => x.Subject!.Code).ThenBy(x => x.TimePoint)
                .Select(x => new { x.SubjectId, x.Id, x.TimePoint }).Take(40).ToListAsync(ct);
            var grp = pair.GroupBy(p => p.SubjectId).FirstOrDefault(g => g.Count() >= 2);
            if (grp is not null)
            {
                var ids = grp.OrderBy(x => x.TimePoint).Select(x => x.Id).Take(2).ToList();
                a ??= ids[0]; b ??= ids[1];
            }
        }

        vm.AId = a; vm.BId = b;
        if (a is int ai) vm.A = await BuildSideAsync(ai, ct);
        if (b is int bi) vm.B = await BuildSideAsync(bi, ct);

        if (vm.A is not null && vm.B is not null)
        {
            if (vm.A.Integrated.HasValue && vm.B.Integrated.HasValue)
                vm.IntegratedDelta = Math.Round(vm.B.Integrated.Value - vm.A.Integrated.Value, 1);

            foreach (var zone in vm.A.Zones.Keys.Union(vm.B.Zones.Keys))
            {
                vm.A.Zones.TryGetValue(zone, out var za); vm.B.Zones.TryGetValue(zone, out var zb);
                vm.ZoneDeltas.Add(new CompareViewModel.ZoneDelta(zone, za, zb,
                    za.HasValue && zb.HasValue ? Math.Round(zb.Value - za.Value, 0) : null));
            }

            int aId = vm.A.AssessmentId, bId = vm.B.AssessmentId;
            var ma = await _db.ProteinMeasurements.Include(m => m.Biomarker)
                .Where(m => m.AssessmentId == aId && m.Value != null).ToListAsync(ct);
            var mb = await _db.ProteinMeasurements.Include(m => m.Biomarker)
                .Where(m => m.AssessmentId == bId && m.Value != null).ToListAsync(ct);
            var mbByCode = mb.ToDictionary(m => m.BiomarkerId, m => m.Value!.Value);
            foreach (var m in ma.OrderBy(m => m.Biomarker!.Domain != null ? m.Biomarker.Domain.DisplayOrder : 0).ThenBy(m => m.Biomarker!.Name))
            {
                double av = m.Value!.Value;
                double? bv = mbByCode.TryGetValue(m.BiomarkerId, out var v) ? v : null;
                double? pct = bv.HasValue && av != 0 ? Math.Round((bv.Value - av) / av * 100, 1) : null;
                vm.BiomarkerDeltas.Add(new CompareViewModel.BiomarkerDelta(m.Biomarker!.Name, av, bv, pct));
            }
        }
        return View(vm);
    }

    private async Task<CompareViewModel.Side?> BuildSideAsync(int id, CancellationToken ct)
    {
        var a = await _db.Assessments
            .Include(x => x.Subject).Include(x => x.SelectedPanel).Include(x => x.PrimaryPhenotype)
            .Include(x => x.ScoreResult).Include(x => x.Images)
            .Include(x => x.ZoneScores).ThenInclude(z => z.FacialZone)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return null;
        var img = a.Images.FirstOrDefault(i => i.IsAnalyzed) ?? a.Images.FirstOrDefault();
        return new CompareViewModel.Side
        {
            AssessmentId = a.Id,
            SubjectCode = a.Subject!.Code,
            Name = a.Subject.DisplayName,
            TimePoint = a.TimePoint.ToString(),
            Date = a.AssessmentDate,
            ImageId = img?.Id,
            ImageOverall = img?.OverallSkinQualityScore,
            Integrated = a.ScoreResult?.IntegratedSkinQualityScore,
            Panel = a.SelectedPanel?.Name,
            Phenotype = a.PrimaryPhenotype?.Name,
            Concordance = a.ScoreResult?.CrossModalConcordance,
            RemodelIdx = a.ScoreResult?.RemodelingBalanceIndex,
            HyalIdx = a.ScoreResult?.HyaluronanAxisIndex,
            BarrierIdx = a.ScoreResult?.BarrierResilienceIndex,
            Zones = a.ZoneScores.OrderBy(z => z.FacialZone!.DisplayOrder)
                .ToDictionary(z => z.FacialZone!.Name, z => z.Overall)
        };
    }

    /// <summary>Immersive live webcam "Skin Scan Studio".</summary>
    [HttpGet]
    public IActionResult Scan() => View();

    /// <summary>
    /// Receives a captured webcam frame, creates a subject + assessment, runs the vision analyzer when a
    /// provider is available, otherwise falls back to the browser's on-device pixel metrics. Returns the
    /// scores + regional zone heatmap for the animated reveal.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> ScanCapture(IFormFile image, double? brightness, double? redness,
        double? sharpness, double? colorfulness,
        double? symmetry, double? jawline, double? thirds, double? fifths, double? yaw, double? quality,
        bool? eyesOpen, bool? facePresent, CancellationToken ct)
    {
        if (image is null || image.Length == 0) return BadRequest(new { error = "No image captured." });

        var n = await _db.Subjects.CountAsync(ct) + 1;
        var subject = new Subject { Code = $"SCAN-{n:D4}", DisplayName = "Live skin scan", CreatedByUserId = _userManager.GetUserId(User) };
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync(ct);

        var assessment = new Assessment
        {
            SubjectId = subject.Id, TimePoint = TimePointType.Baseline, Status = AssessmentStatus.DataEntry,
            CreatedByUserId = _userManager.GetUserId(User)
        };
        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync(ct);

        var dir = UploadRoot();
        var fileName = $"{assessment.Id}_{Guid.NewGuid():N}.jpg";
        await using (var fs = System.IO.File.Create(Path.Combine(dir, fileName)))
            await image.CopyToAsync(fs, ct);
        _db.SkinImages.Add(new SkinImage { AssessmentId = assessment.Id, StoredFileName = fileName, ContentType = image.ContentType ?? "image/jpeg" });
        await _db.SaveChangesAsync(ct);

        // Try the real vision pipeline (also stores zone scores); fall back to on-device metrics.
        var (ok, _, _) = await _assessmentService.AnalyzeImagesAsync(assessment.Id, ct);
        bool aiUsed = ok > 0;

        if (!aiUsed)
        {
            var img = await _db.SkinImages.FirstAsync(i => i.AssessmentId == assessment.Id, ct);
            double br = Clamp(brightness ?? 60), rd = Math.Clamp(redness ?? 0.15, 0, 1), sh = Clamp(sharpness ?? 55);
            img.BrightnessScore = Math.Round(br);
            img.HydrationScore = Math.Round(Clamp(0.6 * br + 18));
            img.RednessHealthScore = Math.Round(Clamp(100 - rd * 100));
            img.TextureSmoothnessScore = Math.Round(sh);
            img.PoreVisibilityScore = Math.Round(Clamp(sh - 5));
            img.PigmentationUniformityScore = Math.Round(colorfulness.HasValue ? Clamp(100 - colorfulness.Value) : 66);
            img.ElasticityScore = 62; img.WrinkleBurdenScore = 63;
            var vals = new[] { img.HydrationScore, img.TextureSmoothnessScore, img.ElasticityScore, img.WrinkleBurdenScore,
                img.PigmentationUniformityScore, img.RednessHealthScore, img.PoreVisibilityScore, img.BrightnessScore }
                .Where(v => v.HasValue).Select(v => v!.Value).ToList();
            img.OverallSkinQualityScore = Math.Round(Clamp(vals.Average()));
            img.IsAnalyzed = true;
            img.AnalysisNotes = "On-device preliminary metrics (vision AI provider not configured).";

            // Synthesize regional zones with a realistic spatial pattern.
            var zoneIds = await _db.FacialZones.Where(z => z.Code != null).ToDictionaryAsync(z => z.Code!, z => z.Id, ct);
            var baseO = (int)(img.OverallSkinQualityScore ?? 60);
            var baseR = (int)(img.RednessHealthScore ?? 58);
            foreach (var (code, dO, dW, dR) in ZonePattern)
            {
                if (!zoneIds.TryGetValue(code, out var fzId)) continue;
                _db.ZoneScores.Add(new ZoneScore
                {
                    AssessmentId = assessment.Id, FacialZoneId = fzId,
                    Overall = Math.Clamp(baseO + dO, 0, 100), Hydration = Math.Clamp(baseO - 6 + dO, 0, 100),
                    Wrinkle = Math.Clamp(baseO - 4 + dW, 0, 100), RednessHealth = Math.Clamp(baseR + dR, 0, 100),
                    Pigmentation = Math.Clamp(baseO + 6 + dO / 2, 0, 100)
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        // Persist landmark-derived facial geometry + capture quality (from MediaPipe, when provided).
        if (symmetry.HasValue || jawline.HasValue || quality.HasValue || facePresent.HasValue)
        {
            _db.FacialGeometries.Add(new FacialGeometry
            {
                AssessmentId = assessment.Id,
                SymmetryScore = symmetry, JawlineAngle = jawline, ThirdsBalance = thirds, FifthsBalance = fifths,
                Yaw = yaw, ImageQualityPercent = quality, EyesOpen = eyesOpen ?? true, FacePresent = facePresent ?? true
            });
            await _db.SaveChangesAsync(ct);
        }

        var final = await _db.SkinImages.FirstAsync(i => i.AssessmentId == assessment.Id, ct);
        var zones = await _db.ZoneScores.Include(z => z.FacialZone)
            .Where(z => z.AssessmentId == assessment.Id)
            .OrderBy(z => z.FacialZone!.DisplayOrder)
            .Select(z => new { code = z.FacialZone!.Code, name = z.FacialZone.Name, overall = z.Overall, wrinkle = z.Wrinkle, rednessHealth = z.RednessHealth, hydration = z.Hydration })
            .ToListAsync(ct);

        return Ok(new
        {
            assessmentId = assessment.Id,
            subjectCode = subject.Code,
            aiUsed,
            message = aiUsed ? "Analyzed with the configured vision model." : "On-device preliminary analysis (configure a vision AI provider for full accuracy).",
            scores = new
            {
                overall = final.OverallSkinQualityScore, hydration = final.HydrationScore, texture = final.TextureSmoothnessScore,
                elasticity = final.ElasticityScore, wrinkle = final.WrinkleBurdenScore, pigmentation = final.PigmentationUniformityScore,
                rednessHealth = final.RednessHealthScore, pore = final.PoreVisibilityScore, brightness = final.BrightnessScore
            },
            zones
        });
    }

    private static double Clamp(double v) => Math.Max(0, Math.Min(100, v));

    // (zoneCode, overallDelta, wrinkleDelta, rednessDelta) — periorbital/forehead age first; cheeks redden.
    private static readonly (string Code, int O, int W, int R)[] ZonePattern =
    {
        ("FOREHEAD", -4, -10, 2), ("PERIORBITAL", -8, -16, 0), ("CHEEK_L", 2, 4, -10),
        ("CHEEK_R", 1, 3, -9), ("PERIORAL", -3, -6, 1), ("TZONE", 0, 2, -4), ("CHIN", 3, 5, -2)
    };

    /// <summary>Builds (once) and shows a fully synthetic, clearly-labelled demonstration report.</summary>
    public async Task<IActionResult> Example(CancellationToken ct)
    {
        var id = await _example.EnsureAsync(ct);
        return RedirectToAction(nameof(Report), new { id });
    }

    private async Task<AssessmentCreateViewModel> BuildCreateVmAsync(CancellationToken ct)
    {
        var subjects = await _db.Subjects.OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new AssessmentCreateViewModel.SubjectOption(s.Id,
                s.Code + (s.DisplayName != null ? " · " + s.DisplayName : "")))
            .ToListAsync(ct);

        var biomarkers = await _db.ProteinBiomarkers.Include(b => b.Domain).Include(b => b.Unit)
            .OrderBy(b => b.Domain!.DisplayOrder).ThenBy(b => b.Name).ToListAsync(ct);

        var domains = biomarkers
            .GroupBy(b => b.Domain!.Name)
            .Select(g => new AssessmentCreateViewModel.DomainGroup(g.Key,
                g.Select(b => new AssessmentCreateViewModel.BiomarkerField(b.Id, b.Name, b.Unit?.Name ?? "AU", b.YoungRefMean)).ToList()))
            .ToList();

        return new AssessmentCreateViewModel { Subjects = subjects, Domains = domains };
    }

    private string UploadRoot()
    {
        var root = Path.Combine(_env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(root);
        return root;
    }
}
