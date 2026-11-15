using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin-only system health: DB reachability, data counts and AI readiness.</summary>
[Authorize(Roles = "Admin")]
public class HealthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAiOrchestrator _ai;

    public HealthController(AppDbContext db, IAiOrchestrator ai)
    {
        _db = db;
        _ai = ai;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new HealthViewModel();

        // DB reachability — counts only make sense if the DB answers.
        try
        {
            vm.DbReachable = await _db.Database.CanConnectAsync(ct);
        }
        catch
        {
            vm.DbReachable = false;
        }

        if (vm.DbReachable)
        {
            vm.SubjectCount = await _db.Subjects.CountAsync(ct);
            vm.AssessmentCount = await _db.Assessments.CountAsync(ct);
            vm.ReportCount = await _db.ReportDocuments.CountAsync(ct);

            // Errors among the most recent 100 AI calls.
            var recentIds = await _db.AiRequestLogs
                .OrderByDescending(l => l.CreatedAtUtc)
                .Take(100)
                .Select(l => l.Success)
                .ToListAsync(ct);
            vm.RecentAiSample = recentIds.Count;
            vm.RecentAiErrors = recentIds.Count(s => !s);
        }

        vm.ChatReady = await _ai.HasUsableProviderAsync(AiTaskKind.Chat, ct);
        vm.VisionReady = await _ai.HasUsableProviderAsync(AiTaskKind.Vision, ct);

        return View(vm);
    }
}

/// <summary>System-health snapshot.</summary>
public class HealthViewModel
{
    public bool DbReachable { get; set; }
    public int SubjectCount { get; set; }
    public int AssessmentCount { get; set; }
    public int ReportCount { get; set; }
    public bool ChatReady { get; set; }
    public bool VisionReady { get; set; }
    public int RecentAiSample { get; set; }
    public int RecentAiErrors { get; set; }
}
