using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin-only configuration of AI backends: keys (encrypted), models, defaults, live test.</summary>
[Authorize(Roles = "Admin")]
public class AiProvidersController : Controller
{
    private readonly AppDbContext _db;
    private readonly ISecretProtector _secrets;
    private readonly IAiOrchestrator _ai;

    public AiProvidersController(AppDbContext db, ISecretProtector secrets, IAiOrchestrator ai)
    {
        _db = db;
        _secrets = secrets;
        _ai = ai;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var providers = await _db.AiProviders
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);
        return View(providers);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        return View(new AiProviderFormViewModel
        {
            Id = p.Id,
            Name = p.Name,
            ProviderType = p.ProviderType,
            IsSystem = p.IsSystem,
            HasApiKey = p.HasApiKey,
            BaseUrl = p.BaseUrl,
            DefaultChatModel = p.DefaultChatModel,
            DefaultVisionModel = p.DefaultVisionModel,
            ExtraHeadersJson = p.ExtraHeadersJson,
            IsEnabled = p.IsEnabled
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AiProviderFormViewModel vm, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == vm.Id, ct);
        if (p is null) return NotFound();

        if (!ModelState.IsValid)
        {
            // Preserve read-only display fields on redisplay.
            vm.Name = p.Name;
            vm.ProviderType = p.ProviderType;
            vm.IsSystem = p.IsSystem;
            vm.HasApiKey = p.HasApiKey;
            return View(vm);
        }

        p.BaseUrl = vm.BaseUrl?.Trim() ?? string.Empty;
        p.DefaultChatModel = string.IsNullOrWhiteSpace(vm.DefaultChatModel) ? null : vm.DefaultChatModel.Trim();
        p.DefaultVisionModel = string.IsNullOrWhiteSpace(vm.DefaultVisionModel) ? null : vm.DefaultVisionModel.Trim();
        p.ExtraHeadersJson = string.IsNullOrWhiteSpace(vm.ExtraHeadersJson) ? null : vm.ExtraHeadersJson.Trim();
        p.IsEnabled = vm.IsEnabled;

        // Write-only key: set only when a new value is supplied; blank leaves the stored key untouched.
        if (!string.IsNullOrWhiteSpace(vm.ApiKey))
            p.ApiKeyEncrypted = _secrets.Encrypt(vm.ApiKey.Trim());

        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"Saved “{p.Name}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Create() =>
        View(new AiProviderFormViewModel { ProviderType = AiProviderType.OpenAiCompatible, IsEnabled = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AiProviderFormViewModel vm, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name is required.");
        else if (await _db.AiProviders.AnyAsync(x => x.Name == vm.Name!.Trim(), ct))
            ModelState.AddModelError(nameof(vm.Name), "A provider with this name already exists.");

        if (!ModelState.IsValid) return View(vm);

        var p = new AiProvider
        {
            Name = vm.Name!.Trim(),
            ProviderType = vm.ProviderType,
            BaseUrl = vm.BaseUrl?.Trim() ?? string.Empty,
            DefaultChatModel = string.IsNullOrWhiteSpace(vm.DefaultChatModel) ? null : vm.DefaultChatModel.Trim(),
            DefaultVisionModel = string.IsNullOrWhiteSpace(vm.DefaultVisionModel) ? null : vm.DefaultVisionModel.Trim(),
            ExtraHeadersJson = string.IsNullOrWhiteSpace(vm.ExtraHeadersJson) ? null : vm.ExtraHeadersJson.Trim(),
            IsEnabled = vm.IsEnabled,
            IsSystem = false
        };
        if (!string.IsNullOrWhiteSpace(vm.ApiKey))
            p.ApiKeyEncrypted = _secrets.Encrypt(vm.ApiKey.Trim());

        _db.AiProviders.Add(p);
        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"Added custom provider “{p.Name}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Test(int id, CancellationToken ct)
    {
        var name = await _db.AiProviders.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefaultAsync(ct);
        if (name is null) return NotFound();

        var result = await _ai.TestConnectionAsync(id, ct);
        if (result.Success)
            TempData["Ok"] = $"{name}: connection OK — model replied in {result.LatencyMs} ms.";
        else
            TempData["Err"] = $"{name}: {result.Error ?? "connection failed."}";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        await _db.AiProviders.Where(x => x.IsDefault).ForEachAsync(x => x.IsDefault = false, ct);
        p.IsDefault = true;
        p.IsEnabled = true; // a default must be usable
        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"“{p.Name}” is now the default provider.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        if (p.IsDefault && p.IsEnabled)
        {
            TempData["Err"] = "Cannot disable the default provider. Set another default first.";
            return RedirectToAction(nameof(Index));
        }

        p.IsEnabled = !p.IsEnabled;
        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"“{p.Name}” {(p.IsEnabled ? "enabled" : "disabled")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearKey(int id, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        p.ApiKeyEncrypted = null;
        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"Cleared the API key for “{p.Name}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var p = await _db.AiProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (p.IsSystem)
        {
            TempData["Err"] = "System providers cannot be deleted. Disable it instead.";
            return RedirectToAction(nameof(Index));
        }

        _db.AiProviders.Remove(p);
        await _db.SaveChangesAsync(ct);
        TempData["Ok"] = $"Deleted “{p.Name}”.";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Edit/create form. The API key is write-only — never populated from storage.</summary>
public class AiProviderFormViewModel
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public AiProviderType ProviderType { get; set; }

    /// <summary>Read-only display flag: seeded rows keep their type and cannot be deleted.</summary>
    public bool IsSystem { get; set; }
    /// <summary>Read-only display flag: whether a key is currently stored.</summary>
    public bool HasApiKey { get; set; }

    public string? BaseUrl { get; set; }
    public string? DefaultChatModel { get; set; }
    public string? DefaultVisionModel { get; set; }
    public string? ExtraHeadersJson { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>Write-only: blank keeps the existing key; non-blank replaces it.</summary>
    public string? ApiKey { get; set; }
}
