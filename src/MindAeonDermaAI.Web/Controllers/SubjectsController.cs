using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Infrastructure.Identity;

namespace MindAeonDermaAI.Web.Controllers;

[Authorize]
public class SubjectsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubjectsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var rows = await _db.Subjects
            .Include(s => s.Sex)
            .Include(s => s.SkinType)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new SubjectRowViewModel(
                s.Id,
                s.Code,
                s.DisplayName,
                s.Age,
                s.Sex != null ? s.Sex.Name : null,
                s.SkinType != null ? s.SkinType.Name : null,
                s.Assessments.Count,
                s.CreatedAtUtc))
            .ToListAsync(ct);

        return View(rows);
    }

    [HttpGet]
    public IActionResult Create() => View(new SubjectFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var subject = new Subject
        {
            Code = await NextCodeAsync(ct),
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim(),
            Age = model.Age,
            HeightCm = model.HeightCm,
            WeightKg = model.WeightKg,
            Bmi = ComputeBmi(model.HeightCm, model.WeightKg),
            SexId = model.SexId,
            SkinTypeId = model.SkinTypeId,
            FitzpatrickTypeId = model.FitzpatrickTypeId,
            MenopauseStatusId = model.MenopauseStatusId,
            SmokingStatusId = model.SmokingStatusId,
            AlcoholUseId = model.AlcoholUseId,
            SleepQualityId = model.SleepQualityId,
            StressLevelId = model.StressLevelId,
            UVExposureLevelId = model.UVExposureLevelId,
            CreatedByUserId = _userManager.GetUserId(User)
        };

        // Up to three optional aesthetic procedures, distinct.
        foreach (var procId in new[] { model.AestheticProcedureId1, model.AestheticProcedureId2, model.AestheticProcedureId3 }
                     .Where(id => id.HasValue).Select(id => id!.Value).Distinct())
        {
            subject.AestheticProcedures.Add(new SubjectAestheticProcedure { AestheticProcedureId = procId });
        }

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync(ct);

        TempData["Ok"] = $"Subject {subject.Code} created.";
        return RedirectToAction(nameof(Details), new { id = subject.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var subject = await _db.Subjects
            .Include(s => s.Sex)
            .Include(s => s.SkinType)
            .Include(s => s.FitzpatrickType)
            .Include(s => s.MenopauseStatus)
            .Include(s => s.SmokingStatus)
            .Include(s => s.AlcoholUse)
            .Include(s => s.SleepQuality)
            .Include(s => s.StressLevel)
            .Include(s => s.UVExposureLevel)
            .Include(s => s.AestheticProcedures).ThenInclude(p => p.AestheticProcedure)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (subject is null) return NotFound();

        var assessments = await _db.Assessments
            .Where(a => a.SubjectId == id)
            .Include(a => a.SelectedPanel)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new SubjectAssessmentRow(
                a.Id,
                a.Status.ToString(),
                a.TimePoint.ToString(),
                a.SelectedPanel != null ? a.SelectedPanel.Name : null,
                a.CreatedAtUtc))
            .ToListAsync(ct);

        return View(new SubjectDetailsViewModel(subject, assessments));
    }

    /// <summary>Generates a unique subject code such as "SUBJ-0007".</summary>
    private async Task<string> NextCodeAsync(CancellationToken ct)
    {
        var n = await _db.Subjects.CountAsync(ct) + 1;
        string code;
        do
        {
            code = $"SUBJ-{n:D4}";
            n++;
        } while (await _db.Subjects.AnyAsync(s => s.Code == code, ct));
        return code;
    }

    /// <summary>BMI = weight(kg) / height(m)², rounded to 1 dp; null when inputs missing/invalid.</summary>
    private static double? ComputeBmi(double? heightCm, double? weightKg)
    {
        if (heightCm is not > 0 || weightKg is not > 0) return null;
        var m = heightCm.Value / 100.0;
        return Math.Round(weightKg.Value / (m * m), 1);
    }
}

/// <summary>Row projection for the subjects list.</summary>
public record SubjectRowViewModel(
    int Id, string Code, string? DisplayName, int? Age,
    string? SexName, string? SkinTypeName, int AssessmentCount, DateTime CreatedAtUtc);

/// <summary>Row projection for a subject's assessments on the Details view.</summary>
public record SubjectAssessmentRow(int Id, string Status, string TimePoint, string? Panel, DateTime CreatedAtUtc);

/// <summary>Details view model: the loaded subject plus its assessment rows.</summary>
public record SubjectDetailsViewModel(Subject Subject, IReadOnlyList<SubjectAssessmentRow> Assessments);

/// <summary>
/// Create/Edit form model. Every categorical field is an FK int selected via Smart-Select; selected
/// names are only populated for edit scenarios (null on create).
/// </summary>
public class SubjectFormViewModel
{
    public int Id { get; set; }

    public string? DisplayName { get; set; }
    public int? Age { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }

    public int? SexId { get; set; }
    public string? SexName { get; set; }

    public int? SkinTypeId { get; set; }
    public string? SkinTypeName { get; set; }

    public int? FitzpatrickTypeId { get; set; }
    public string? FitzpatrickTypeName { get; set; }

    public int? MenopauseStatusId { get; set; }
    public string? MenopauseStatusName { get; set; }

    public int? SmokingStatusId { get; set; }
    public string? SmokingStatusName { get; set; }

    public int? AlcoholUseId { get; set; }
    public string? AlcoholUseName { get; set; }

    public int? SleepQualityId { get; set; }
    public string? SleepQualityName { get; set; }

    public int? StressLevelId { get; set; }
    public string? StressLevelName { get; set; }

    public int? UVExposureLevelId { get; set; }
    public string? UVExposureLevelName { get; set; }

    // Up to three optional prior aesthetic procedures.
    public int? AestheticProcedureId1 { get; set; }
    public string? AestheticProcedureName1 { get; set; }
    public int? AestheticProcedureId2 { get; set; }
    public string? AestheticProcedureName2 { get; set; }
    public int? AestheticProcedureId3 { get; set; }
    public string? AestheticProcedureName3 { get; set; }
}
