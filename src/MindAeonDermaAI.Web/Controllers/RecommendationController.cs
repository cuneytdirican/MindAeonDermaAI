using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Cosmetic-ingredient (INCI) recommendation + product suitability views for an assessment.
/// Everything served here is cosmetic education only and NON-DIAGNOSTIC (research/prototyping use).
/// </summary>
[Authorize]
public class RecommendationController : Controller
{
    private readonly AppDbContext _db;

    public RecommendationController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Curated ingredient suggestions derived from the assessment's deterministic score.</summary>
    public async Task<IActionResult> Index(int id, CancellationToken ct)
    {
        var assessment = await _db.Assessments
            .Include(a => a.Subject)
            .Include(a => a.ScoreResult)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (assessment is null) return NotFound();
        if (assessment.ScoreResult is null)
        {
            TempData["Err"] = "This assessment has no score yet. Run the analysis before requesting recommendations.";
            return RedirectToAction("Details", "Assessments", new { id });
        }

        var vm = new RecommendationVm
        {
            Assessment = assessment,
            Subject = assessment.Subject,
            Score = assessment.ScoreResult,
            Recommendations = RecommendationEngine.Recommend(assessment.ScoreResult)
        };
        return View(vm);
    }

    /// <summary>GET: show the assessment context and an empty INCI paste form.</summary>
    [HttpGet]
    public async Task<IActionResult> Ingredients(int id, CancellationToken ct)
    {
        var vm = await LoadInciVmAsync(id, ct);
        if (vm is null)
        {
            if (TempData["Err"] is not null) return RedirectToAction("Details", "Assessments", new { id });
            return NotFound();
        }
        return View(vm);
    }

    /// <summary>POST: analyze a pasted INCI list against the assessment's profile.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingredients(int id, string inci, CancellationToken ct)
    {
        var vm = await LoadInciVmAsync(id, ct);
        if (vm is null)
        {
            if (TempData["Err"] is not null) return RedirectToAction("Details", "Assessments", new { id });
            return NotFound();
        }

        vm.InciText = inci;
        vm.Result = RecommendationEngine.AnalyzeInci(inci ?? string.Empty, vm.Score!);
        return View(vm);
    }

    // Loads the shared INCI view-model; sets TempData["Err"] and returns null when there is no score.
    private async Task<InciVm?> LoadInciVmAsync(int id, CancellationToken ct)
    {
        var assessment = await _db.Assessments
            .Include(a => a.Subject)
            .Include(a => a.ScoreResult)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (assessment is null) return null;
        if (assessment.ScoreResult is null)
        {
            TempData["Err"] = "This assessment has no score yet. Run the analysis before checking ingredients.";
            return null;
        }

        return new InciVm
        {
            Assessment = assessment,
            Subject = assessment.Subject,
            Score = assessment.ScoreResult
        };
    }
}

/// <summary>View-model for the ingredient-recommendations page.</summary>
public class RecommendationVm
{
    public Assessment Assessment { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ScoreResult Score { get; set; } = null!;
    public List<RecommendationEngine.IngredientRec> Recommendations { get; set; } = new();
}

/// <summary>View-model for the INCI-suitability page (form + optional result).</summary>
public class InciVm
{
    public Assessment Assessment { get; set; } = null!;
    public Subject? Subject { get; set; }
    public ScoreResult? Score { get; set; }
    public string? InciText { get; set; }
    public RecommendationEngine.InciResult? Result { get; set; }
}
