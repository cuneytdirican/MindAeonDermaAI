using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Genomics-ready intake: captures genetic/omics markers as a placeholder for future DNA modules.
/// Non-diagnostic, research use only.
/// </summary>
[Authorize]
public class GenomicsController : Controller
{
    private readonly AppDbContext _db;

    public GenomicsController(AppDbContext db) => _db = db;

    /// <summary>Subject + its omics markers (newest first).</summary>
    public async Task<IActionResult> Subject(int id, CancellationToken ct)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (subject is null) return NotFound();

        var markers = await _db.OmicsMarkers
            .Include(o => o.OmicsType)
            .Where(o => o.SubjectId == id)
            .OrderByDescending(o => o.RecordedAt)
            .ToListAsync(ct);

        return View(new GenomicsSubjectVm { Subject = subject, Markers = markers });
    }

    /// <summary>Persist one omics marker (FK validated). RecordedAt defaults to UtcNow.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, int? omicsTypeId, string marker, string? value,
        string? interpretation, CancellationToken ct)
    {
        if (!await _db.Subjects.AnyAsync(s => s.Id == id, ct)) return NotFound();

        if (omicsTypeId is null || !await _db.OmicsTypes.AnyAsync(x => x.Id == omicsTypeId, ct) ||
            string.IsNullOrWhiteSpace(marker))
        {
            TempData["Err"] = "Please choose an omics type and enter a marker.";
            return RedirectToAction(nameof(Subject), new { id });
        }

        _db.OmicsMarkers.Add(new OmicsMarker
        {
            SubjectId = id,
            OmicsTypeId = omicsTypeId.Value,
            Marker = marker.Trim(),
            Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim(),
            Interpretation = string.IsNullOrWhiteSpace(interpretation) ? null : interpretation.Trim(),
            RecordedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        TempData["Ok"] = "Omics marker added.";
        return RedirectToAction(nameof(Subject), new { id });
    }
}

/// <summary>View model for the genomics-ready intake screen.</summary>
public class GenomicsSubjectVm
{
    public Subject Subject { get; set; } = default!;
    public List<OmicsMarker> Markers { get; set; } = new();
}
