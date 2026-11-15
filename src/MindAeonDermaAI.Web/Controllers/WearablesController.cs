using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Wearables intake: sleep/stress/HRV/recovery readings that enrich lifestyle & exposome context.
/// Non-diagnostic, research use only.
/// </summary>
[Authorize]
public class WearablesController : Controller
{
    private readonly AppDbContext _db;

    public WearablesController(AppDbContext db) => _db = db;

    /// <summary>Subject + its wearable readings (newest first).</summary>
    public async Task<IActionResult> Subject(int id, CancellationToken ct)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (subject is null) return NotFound();

        var readings = await _db.WearableReadings
            .Include(w => w.WearableSource)
            .Include(w => w.WearableMetricType)
            .Where(w => w.SubjectId == id)
            .OrderByDescending(w => w.RecordedAt)
            .ToListAsync(ct);

        return View(new WearableSubjectVm { Subject = subject, Readings = readings });
    }

    /// <summary>Persist one reading (FK ids validated). RecordedAt defaults to UtcNow.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, int? wearableSourceId, int? wearableMetricTypeId,
        double value, string? unit, DateTime? recordedAt, CancellationToken ct)
    {
        if (!await _db.Subjects.AnyAsync(s => s.Id == id, ct)) return NotFound();

        if (wearableSourceId is null || !await _db.WearableSources.AnyAsync(x => x.Id == wearableSourceId, ct) ||
            wearableMetricTypeId is null || !await _db.WearableMetricTypes.AnyAsync(x => x.Id == wearableMetricTypeId, ct))
        {
            TempData["Err"] = "Please choose a valid source and metric.";
            return RedirectToAction(nameof(Subject), new { id });
        }

        _db.WearableReadings.Add(new WearableReading
        {
            SubjectId = id,
            WearableSourceId = wearableSourceId.Value,
            WearableMetricTypeId = wearableMetricTypeId.Value,
            Value = value,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            RecordedAt = recordedAt ?? DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        TempData["Ok"] = "Wearable reading added.";
        return RedirectToAction(nameof(Subject), new { id });
    }
}

/// <summary>View model for the wearables intake screen.</summary>
public class WearableSubjectVm
{
    public Subject Subject { get; set; } = default!;
    public List<WearableReading> Readings { get; set; } = new();
}
