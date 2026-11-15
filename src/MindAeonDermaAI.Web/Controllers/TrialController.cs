using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Clinical-trial mode: each intervention category is a trial "arm" with baseline vs follow-up means and change. Research use, non-diagnostic.</summary>
[Authorize]
public class TrialController : Controller
{
    private readonly AppDbContext _db;

    public TrialController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new TrialVm();

        // Pull analyzed assessments (with score) to memory, then aggregate per arm in LINQ-to-objects.
        var rows = await _db.Assessments
            .Where(a => a.ScoreResult != null)
            .Select(a => new
            {
                Arm = a.InterventionCategory != null ? a.InterventionCategory.Name : "Uncategorized",
                a.SubjectId,
                a.TimePoint,
                Integrated = a.ScoreResult!.IntegratedSkinQualityScore
            })
            .ToListAsync(ct);

        vm.Arms = rows
            .GroupBy(r => r.Arm)
            .Select(g =>
            {
                var baseline = g.Where(x => x.TimePoint == TimePointType.Baseline).ToList();
                var follow = g.Where(x => x.TimePoint == TimePointType.FollowUp).ToList();
                double? meanBase = baseline.Count > 0 ? baseline.Average(x => x.Integrated) : null;
                double? meanFollow = follow.Count > 0 ? follow.Average(x => x.Integrated) : null;

                // Mean paired change: per subject, follow-up mean − baseline mean, where both exist.
                var baseBySubj = baseline.GroupBy(x => x.SubjectId).ToDictionary(s => s.Key, s => s.Average(x => x.Integrated));
                var followBySubj = follow.GroupBy(x => x.SubjectId).ToDictionary(s => s.Key, s => s.Average(x => x.Integrated));
                var deltas = baseBySubj.Keys.Intersect(followBySubj.Keys)
                    .Select(id => followBySubj[id] - baseBySubj[id]).ToList();
                double? meanChange = deltas.Count > 0 ? deltas.Average() : null;

                return new ArmRow(
                    g.Key,
                    g.Select(x => x.SubjectId).Distinct().Count(),
                    meanBase,
                    meanFollow,
                    meanChange,
                    deltas.Count);
            })
            .OrderByDescending(a => a.SubjectCount).ToList();

        return View(vm);
    }
}

// ---- View model (in-file to keep the trial area self-contained) ----

public class TrialVm
{
    public List<ArmRow> Arms { get; set; } = new();
}

/// <param name="PairedN">Subjects with both baseline and follow-up (used for mean change).</param>
public record ArmRow(
    string Arm, int SubjectCount,
    double? MeanBaseline, double? MeanFollowUp, double? MeanChange, int PairedN);
