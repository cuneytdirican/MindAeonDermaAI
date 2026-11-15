using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin-only platform analytics: population make-up, self-calibrated references, assay batch-drift.</summary>
[Authorize(Roles = "Admin")]
public class AnalyticsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IReferenceCalibrationService _calibration;

    public AnalyticsController(AppDbContext db, IReferenceCalibrationService calibration)
    {
        _db = db;
        _calibration = calibration;
    }

    public async Task<IActionResult> Index(int? biomarkerId, CancellationToken ct)
    {
        var vm = new AnalyticsViewModel();

        // ---- Population KPIs ----
        vm.SubjectCount = await _db.Subjects.CountAsync(ct);
        vm.AnalyzedCount = await _db.ScoreResults.CountAsync(ct);
        vm.AvgIntegratedScore = await _db.ScoreResults.AnyAsync(ct)
            ? await _db.ScoreResults.AverageAsync(s => s.IntegratedSkinQualityScore, ct)
            : (double?)null;
        vm.SyntheticCohortCount = await _db.ScoreResults.CountAsync(s => s.UsedSyntheticCohort, ct);

        // ---- Phenotype distribution (analyzed assessments with a primary phenotype) ----
        // Group client-side: EF cannot translate GroupBy over a navigation key into a record projection.
        var phenoNames = await _db.Assessments
            .Where(a => a.PrimaryPhenotypeId != null)
            .Select(a => a.PrimaryPhenotype!.Name).ToListAsync(ct);
        vm.PhenotypeDistribution = phenoNames
            .GroupBy(n => n).Select(g => new NameCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).ToList();

        // ---- Panel distribution ----
        var panelNames = await _db.Assessments
            .Where(a => a.SelectedPanel != null)
            .Select(a => a.SelectedPanel!.Name).ToListAsync(ct);
        vm.PanelDistribution = panelNames
            .GroupBy(n => n).Select(g => new NameCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count).ToList();

        // ---- Biomarker reference table ----
        vm.References = await _db.ProteinBiomarkers
            .OrderBy(b => b.Domain!.DisplayOrder).ThenBy(b => b.Name)
            .Select(b => new BiomarkerRefRow(
                b.Name,
                b.Domain != null ? b.Domain.Name : "—",
                b.YoungRefMean,
                b.YoungRefSd,
                b.CohortRefMean,
                b.CohortRefSd,
                b.CohortRefN))
            .ToListAsync(ct);

        // ---- Biomarker selector (for batch-drift) ----
        vm.Biomarkers = await _db.ProteinBiomarkers
            .OrderBy(b => b.Id)
            .Select(b => new NameId(b.Id, b.Name))
            .ToListAsync(ct);

        vm.SelectedBiomarkerId = biomarkerId ?? vm.Biomarkers.Select(b => (int?)b.Id).FirstOrDefault();
        vm.SelectedBiomarkerName = vm.Biomarkers.FirstOrDefault(b => b.Id == vm.SelectedBiomarkerId)?.Name;

        // ---- Assay batch-drift for the selected biomarker ----
        if (vm.SelectedBiomarkerId is int bid)
        {
            var raw = await _db.ProteinMeasurements
                .Where(m => m.BiomarkerId == bid && m.Value != null && m.Assessment!.AssayBatch != null)
                .Select(m => new { Batch = m.Assessment!.AssayBatch!, Val = m.Value!.Value })
                .ToListAsync(ct);
            vm.BatchDrift = raw
                .GroupBy(x => x.Batch)
                .Select(g => new BatchMean(g.Key, g.Average(y => y.Val), g.Count()))
                .OrderBy(x => x.Batch).ToList();
        }

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Recalibrate(CancellationToken ct)
    {
        var n = await _calibration.RecalibrateAsync(ct: ct);
        if (n > 0)
            TempData["Ok"] = $"Recalibrated reference ranges for {n} biomarker(s) from the platform cohort.";
        else
            TempData["Err"] = "No biomarker has enough platform measurements yet (minimum 8 baseline readings). References stay on young reference until the cohort grows.";
        return RedirectToAction(nameof(Index));
    }
}

// ---- View model (in-file to keep the analytics area self-contained) ----

public class AnalyticsViewModel
{
    // Population KPIs
    public int SubjectCount { get; set; }
    public int AnalyzedCount { get; set; }
    public double? AvgIntegratedScore { get; set; }
    public int SyntheticCohortCount { get; set; }

    public List<NameCount> PhenotypeDistribution { get; set; } = new();
    public List<NameCount> PanelDistribution { get; set; } = new();
    public List<BiomarkerRefRow> References { get; set; } = new();

    // Batch-drift
    public List<NameId> Biomarkers { get; set; } = new();
    public int? SelectedBiomarkerId { get; set; }
    public string? SelectedBiomarkerName { get; set; }
    public List<BatchMean> BatchDrift { get; set; } = new();
}

public record NameCount(string Name, int Count);
public record NameId(int Id, string Name);
public record BiomarkerRefRow(
    string Name, string Domain,
    double YoungRefMean, double YoungRefSd,
    double? CohortRefMean, double? CohortRefSd, int CohortRefN);
public record BatchMean(string Batch, double Mean, int Count);
