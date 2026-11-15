using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Security &amp; compliance: posture overview, consent management and pseudonymization.</summary>
[Authorize(Roles = "Admin")]
public class ComplianceController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;

    public ComplianceController(AppDbContext db, IAuditLogger audit)
    {
        _db = db;
        _audit = audit;
    }

    public class ComplianceVm
    {
        public int Subjects { get; set; }
        public int Consented { get; set; }
        public int Anonymized { get; set; }
        public int AuditEntries { get; set; }
        public List<Row> Rows { get; set; } = new();
        public record Row(int Id, string Code, string? Name, bool ConsentGiven, DateTime? ConsentAtUtc, bool Anonymized);
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new ComplianceVm
        {
            Subjects = await _db.Subjects.CountAsync(ct),
            Consented = await _db.Subjects.CountAsync(s => s.ConsentGiven, ct),
            Anonymized = await _db.Subjects.CountAsync(s => s.Anonymized, ct),
            AuditEntries = await _db.AuditLogs.CountAsync(ct),
            Rows = await _db.Subjects.OrderByDescending(s => s.Id).Take(100)
                .Select(s => new ComplianceVm.Row(s.Id, s.Code, s.DisplayName, s.ConsentGiven, s.ConsentAtUtc, s.Anonymized))
                .ToListAsync(ct)
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetConsent(int subjectId, bool given, string? purpose, CancellationToken ct)
    {
        var s = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId, ct);
        if (s is not null)
        {
            s.ConsentGiven = given;
            s.ConsentAtUtc = given ? DateTime.UtcNow : null;
            s.ConsentPurpose = given ? (purpose ?? "Research & cosmetic skin-biology analysis") : null;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(given ? "consent.granted" : "consent.withdrawn", "Subject", subjectId, s.Code, ct);
            TempData["Ok"] = $"Consent {(given ? "recorded" : "withdrawn")} for {s.Code}.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAnonymize(int subjectId, CancellationToken ct)
    {
        var s = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId, ct);
        if (s is not null)
        {
            s.Anonymized = !s.Anonymized;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(s.Anonymized ? "subject.anonymized" : "subject.deanonymized", "Subject", subjectId, s.Code, ct);
            TempData["Ok"] = $"{s.Code} is now {(s.Anonymized ? "pseudonymized" : "identifiable")}.";
        }
        return RedirectToAction(nameof(Index));
    }
}
