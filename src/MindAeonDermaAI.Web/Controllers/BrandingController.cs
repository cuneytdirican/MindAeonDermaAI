using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Web.Services;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin white-label branding: product name, tagline, colors, logo.</summary>
[Authorize(Roles = "Admin")]
public class BrandingController : Controller
{
    private readonly IBrandingService _branding;
    public BrandingController(IBrandingService branding) => _branding = branding;

    public async Task<IActionResult> Index(CancellationToken ct) => View(await _branding.GetAsync(ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BrandingSettings model, CancellationToken ct)
    {
        await _branding.SaveAsync(model, ct);
        TempData["Ok"] = "Branding updated. It applies across the app, landing page and PDF.";
        return RedirectToAction(nameof(Index));
    }
}
