using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Curated, non-diagnostic literature/evidence background for each seeded biomarker.</summary>
[Authorize]
public class LiteratureController : Controller
{
    private readonly AppDbContext _db;

    public LiteratureController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Pull seeded biomarkers with their domain, ordered by domain then name.
        var biomarkers = await _db.ProteinBiomarkers
            .Include(b => b.Domain)
            .OrderBy(b => b.Domain!.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync(ct);

        // Join to the curated catalog by Code; keep only biomarkers we have evidence for.
        var rows = biomarkers
            .Select(b => new LiteratureRow(
                b.Name,
                b.Domain?.Name ?? "—",
                b.Code is null ? null : BiomarkerEvidenceCatalog.Get(b.Code)))
            .Where(r => r.Evidence is not null)
            .ToList();

        return View(rows);
    }
}

/// <summary>One biomarker row for the literature view: display name, domain name and curated evidence.</summary>
public record LiteratureRow(string BiomarkerName, string DomainName, BiomarkerEvidenceCatalog.Evidence? Evidence);
