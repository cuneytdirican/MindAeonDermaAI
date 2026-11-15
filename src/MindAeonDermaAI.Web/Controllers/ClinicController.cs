using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Clinic performance dashboard: throughput, per-intervention outcomes, recent reports. Research use, non-diagnostic.</summary>
[Authorize]
public class ClinicController : Controller
{
    private readonly AppDbContext _db;

    public ClinicController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new ClinicVm();

        // ---- KPIs ----
        vm.SubjectCount = await _db.Subjects.CountAsync(ct);
        vm.AssessmentCount = await _db.Assessments.CountAsync(ct);
        vm.AnalyzedCount = await _db.ScoreResults.CountAsync(ct);
        vm.AvgIntegratedScore = await _db.ScoreResults.AnyAsync(ct)
            ? await _db.ScoreResults.AverageAsync(s => s.IntegratedSkinQualityScore, ct)
            : (double?)null;

        // Analyzed assessments only (those with a score). Pull the fields we need to memory,
        // then group in LINQ-to-objects — EF cannot translate GroupBy over a nav key into a record.
        var analyzed = await _db.Assessments
            .Where(a => a.ScoreResult != null)
            .Select(a => new
            {
                a.CreatedByUserId,
                Intervention = a.InterventionCategory != null ? a.InterventionCategory.Name : null,
                Integrated = a.ScoreResult!.IntegratedSkinQualityScore
            })
            .ToListAsync(ct);

        // ---- Per-clinician throughput (only user-id strings available) ----
        vm.Clinicians = analyzed
            .GroupBy(a => a.CreatedByUserId)
            .Select(g => new ClinicianRow(
                string.IsNullOrWhiteSpace(g.Key)
                    ? "Unassigned"
                    : $"Clinician {g.Key![..Math.Min(8, g.Key.Length)]}",
                g.Count(),
                g.Average(x => x.Integrated)))
            .OrderByDescending(x => x.Count).ToList();

        // ---- Per-intervention-category outcome ----
        vm.Interventions = analyzed
            .GroupBy(a => a.Intervention ?? "Uncategorized")
            .Select(g => new InterventionRow(g.Key, g.Count(), g.Average(x => x.Integrated)))
            .OrderByDescending(x => x.Count).ToList();

        // ---- Recent reports (latest 8 analyzed) ----
        vm.Recent = await _db.Assessments
            .Where(a => a.ScoreResult != null)
            .OrderByDescending(a => a.AssessmentDate).ThenByDescending(a => a.Id)
            .Take(8)
            .Select(a => new RecentReportRow(
                a.Id,
                a.Subject != null ? a.Subject.Code : "—",
                a.SelectedPanel != null ? a.SelectedPanel.Name : "—",
                a.PrimaryPhenotype != null ? a.PrimaryPhenotype.Name : "—",
                a.ScoreResult!.IntegratedSkinQualityScore,
                a.AssessmentDate))
            .ToListAsync(ct);

        return View(vm);
    }
}

// ---- View model (in-file to keep the clinic area self-contained) ----

public class ClinicVm
{
    public int SubjectCount { get; set; }
    public int AssessmentCount { get; set; }
    public int AnalyzedCount { get; set; }
    public double? AvgIntegratedScore { get; set; }

    public List<ClinicianRow> Clinicians { get; set; } = new();
    public List<InterventionRow> Interventions { get; set; } = new();
    public List<RecentReportRow> Recent { get; set; } = new();
}

public record ClinicianRow(string Label, int Count, double AvgIntegrated);
public record InterventionRow(string Name, int Count, double AvgIntegrated);
public record RecentReportRow(int AssessmentId, string SubjectCode, string Panel, string Phenotype, double Integrated, DateTime Date);
