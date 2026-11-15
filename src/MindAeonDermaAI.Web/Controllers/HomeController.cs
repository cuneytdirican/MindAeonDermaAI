using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Models;

namespace MindAeonDermaAI.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAiOrchestrator _ai;

    public HomeController(AppDbContext db, IAiOrchestrator ai)
    {
        _db = db;
        _ai = ai;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new DashboardViewModel
        {
            SubjectCount = await _db.Subjects.CountAsync(ct),
            AssessmentCount = await _db.Assessments.CountAsync(ct),
            ReportCount = await _db.ReportDocuments.CountAsync(ct),
            ConfiguredProviders = await _db.AiProviders.CountAsync(p => p.IsEnabled && p.ApiKeyEncrypted != null, ct),
            AiReady = await _ai.HasUsableProviderAsync(AiTaskKind.Chat, ct),
            Recent = await _db.Assessments
                .Include(a => a.Subject)
                .Include(a => a.SelectedPanel)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Take(6)
                .Select(a => new DashboardViewModel.RecentRow(
                    a.Id,
                    a.Subject!.Code,
                    a.Subject.DisplayName,
                    a.Status.ToString(),
                    a.SelectedPanel != null ? a.SelectedPanel.Name : null,
                    a.ScoreResult != null ? a.ScoreResult.IntegratedSkinQualityScore : (double?)null,
                    a.CreatedAtUtc))
                .ToListAsync(ct)
        };

        // Panel distribution across analyzed assessments.
        vm.PanelDistribution = await _db.Assessments
            .Where(a => a.SelectedPanel != null)
            .GroupBy(a => a.SelectedPanel!.Name)
            .Select(g => new DashboardViewModel.NameCount(g.Key, g.Count()))
            .ToListAsync(ct);

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
