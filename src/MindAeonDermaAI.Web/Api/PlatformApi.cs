using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Api;

/// <summary>Public REST API v1 (X-Api-Key auth). Read + limited create for integration/LIS/CRM clients.</summary>
[ApiController]
[AllowAnonymous]
[ApiKey]
[Route("api/v1/subjects")]
[Produces("application/json")]
public class SubjectsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public SubjectsApiController(AppDbContext db) => _db = db;

    public record SubjectDto(int Id, string Code, string? DisplayName, int? Age, double? Bmi, bool ConsentGiven, int AssessmentCount);
    public record CreateSubjectDto(string? DisplayName, int? Age, double? HeightCm, double? WeightKg, bool ConsentGiven);

    /// <summary>List subjects.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _db.Subjects.OrderByDescending(s => s.Id)
            .Select(s => new SubjectDto(s.Id, s.Code, s.Anonymized ? null : s.DisplayName, s.Age, s.Bmi, s.ConsentGiven, s.Assessments.Count))
            .ToListAsync(ct);
        return Ok(rows);
    }

    /// <summary>Get a single subject.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var s = await _db.Subjects.Where(x => x.Id == id)
            .Select(x => new SubjectDto(x.Id, x.Code, x.Anonymized ? null : x.DisplayName, x.Age, x.Bmi, x.ConsentGiven, x.Assessments.Count))
            .FirstOrDefaultAsync(ct);
        return s is null ? NotFound() : Ok(s);
    }

    /// <summary>Create a subject (auto-generates the code).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDto body, CancellationToken ct)
    {
        var n = await _db.Subjects.CountAsync(ct) + 1;
        var subject = new Subject
        {
            Code = $"API-{n:D4}",
            DisplayName = body.DisplayName,
            Age = body.Age,
            HeightCm = body.HeightCm,
            WeightKg = body.WeightKg,
            Bmi = body is { HeightCm: > 0, WeightKg: > 0 } ? Math.Round(body.WeightKg!.Value / Math.Pow(body.HeightCm!.Value / 100, 2), 1) : null,
            ConsentGiven = body.ConsentGiven,
            ConsentAtUtc = body.ConsentGiven ? DateTime.UtcNow : null
        };
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = subject.Id },
            new SubjectDto(subject.Id, subject.Code, subject.DisplayName, subject.Age, subject.Bmi, subject.ConsentGiven, 0));
    }
}

/// <summary>Public REST API v1 — assessments + scores.</summary>
[ApiController]
[AllowAnonymous]
[ApiKey]
[Route("api/v1/assessments")]
[Produces("application/json")]
public class AssessmentsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public AssessmentsApiController(AppDbContext db) => _db = db;

    public record AssessmentDto(int Id, string SubjectCode, string TimePoint, DateTime Date, string? Panel, string? Phenotype,
        double? Integrated, double? BiologicalSkinAge, double? OverallConfidence, double? BenchmarkPercentile);

    /// <summary>List analyzed assessments with their headline scores.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _db.Assessments.Include(a => a.Subject).Include(a => a.SelectedPanel).Include(a => a.PrimaryPhenotype).Include(a => a.ScoreResult)
            .OrderByDescending(a => a.Id)
            .Select(a => new AssessmentDto(a.Id, a.Subject!.Code, a.TimePoint.ToString(), a.AssessmentDate,
                a.SelectedPanel != null ? a.SelectedPanel.Name : null,
                a.PrimaryPhenotype != null ? a.PrimaryPhenotype.Name : null,
                a.ScoreResult != null ? a.ScoreResult.IntegratedSkinQualityScore : (double?)null,
                a.ScoreResult != null ? a.ScoreResult.BiologicalSkinAge : null,
                a.ScoreResult != null ? a.ScoreResult.OverallConfidence : null,
                a.ScoreResult != null ? a.ScoreResult.BenchmarkPercentile : null))
            .ToListAsync(ct);
        return Ok(rows);
    }

    /// <summary>Get one assessment with scores.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var a = await _db.Assessments.Include(x => x.Subject).Include(x => x.SelectedPanel).Include(x => x.PrimaryPhenotype).Include(x => x.ScoreResult)
            .Where(x => x.Id == id)
            .Select(x => new AssessmentDto(x.Id, x.Subject!.Code, x.TimePoint.ToString(), x.AssessmentDate,
                x.SelectedPanel != null ? x.SelectedPanel.Name : null,
                x.PrimaryPhenotype != null ? x.PrimaryPhenotype.Name : null,
                x.ScoreResult != null ? x.ScoreResult.IntegratedSkinQualityScore : (double?)null,
                x.ScoreResult != null ? x.ScoreResult.BiologicalSkinAge : null,
                x.ScoreResult != null ? x.ScoreResult.OverallConfidence : null,
                x.ScoreResult != null ? x.ScoreResult.BenchmarkPercentile : null))
            .FirstOrDefaultAsync(ct);
        return a is null ? NotFound() : Ok(a);
    }
}
