using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Longitudinal progress view: charts a subject's analyzed assessments over time. Non-diagnostic.</summary>
[Authorize]
public class TimelineController : Controller
{
    private readonly AppDbContext _db;

    public TimelineController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Subject(int id, CancellationToken ct)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (subject is null) return NotFound();

        // All assessments for the subject, oldest first, with the pieces the timeline needs.
        var assessments = await _db.Assessments
            .Where(a => a.SubjectId == id)
            .Include(a => a.ScoreResult)
            .Include(a => a.SelectedPanel)
            .Include(a => a.PrimaryPhenotype)
            .OrderBy(a => a.AssessmentDate)
            .ToListAsync(ct);

        // One point per assessment that actually has a computed score.
        var points = assessments
            .Where(a => a.ScoreResult is not null)
            .Select(a => new TimelineVm.Point(
                a.AssessmentDate,
                a.TimePoint.ToString(),
                a.ScoreResult!.IntegratedSkinQualityScore,
                a.ScoreResult.CollagenRemodelingNeedScore,
                a.ScoreResult.BarrierHydrationNeedScore,
                a.ScoreResult.DeepEcmLaxityNeedScore,
                a.ScoreResult.BiologicalSkinAge,
                a.SelectedPanel?.Name,
                a.PrimaryPhenotype?.Name,
                a.Id))
            .ToList();

        var vm = new TimelineVm
        {
            SubjectId = subject.Id,
            SubjectCode = subject.Code,
            SubjectName = subject.DisplayName,
            Points = points,
            HasTrend = points.Count >= 2
        };

        return View(vm);
    }
}

/// <summary>View model for the subject progress timeline.</summary>
public class TimelineVm
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string? SubjectName { get; set; }
    public List<Point> Points { get; set; } = new();

    /// <summary>True when there are at least two analyzed points to compare.</summary>
    public bool HasTrend { get; set; }

    /// <summary>A single analyzed assessment on the timeline.</summary>
    public record Point(
        DateTime Date,
        string TimePoint,
        double Integrated,
        double CollagenNeed,
        double BarrierNeed,
        double DeepEcmNeed,
        double? BiologicalSkinAge,
        string? Panel,
        string? Phenotype,
        int AssessmentId);
}
