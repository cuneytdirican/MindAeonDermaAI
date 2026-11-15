using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindAeonDermaAI.Core.Lookups;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Admin browse surface for every reference (lookup) category. Read/browse plus inline
/// "add value" via the same Smart-Select widget used across the app — demonstrating the
/// "define without leaving the screen" rule. Creation itself flows through /api/lookup/{entity}.
/// </summary>
[Authorize(Roles = "Admin")]
public class LookupsController : Controller
{
    private readonly ILookupService _lookups;

    public LookupsController(ILookupService lookups)
    {
        _lookups = lookups;
    }

    // Grouped categories shown in the left nav (key + friendly label).
    private static readonly IReadOnlyList<LookupCategoryGroup> Groups = new List<LookupCategoryGroup>
    {
        new("Demographics", new List<LookupCategory>
        {
            new("sex", "Sex"),
            new("skinType", "Skin type"),
            new("fitzpatrickType", "Fitzpatrick type"),
            new("menopauseStatus", "Menopause status"),
        }),
        new("Lifestyle", new List<LookupCategory>
        {
            new("smokingStatus", "Smoking status"),
            new("alcoholUse", "Alcohol use"),
            new("sleepQuality", "Sleep quality"),
            new("stressLevel", "Stress level"),
            new("uvExposureLevel", "UV exposure"),
        }),
        new("Clinical", new List<LookupCategory>
        {
            new("aestheticProcedure", "Aesthetic procedure"),
            new("interventionCategory", "Intervention category"),
            new("measurementUnit", "Measurement unit"),
            new("phenotypeType", "Phenotype type"),
        }),
    };

    public async Task<IActionResult> Index(string? entity, CancellationToken ct)
    {
        // Default to the first known category when none/unknown is requested.
        if (string.IsNullOrWhiteSpace(entity) || !_lookups.IsKnownEntity(entity))
            entity = "sex";

        var label = Groups.SelectMany(g => g.Categories)
            .FirstOrDefault(c => c.Key == entity)?.Label ?? entity;

        var items = await _lookups.SearchAsync(entity, null, 200, ct);

        return View(new LookupsIndexViewModel
        {
            Groups = Groups,
            SelectedEntity = entity,
            SelectedLabel = label,
            Items = items,
        });
    }
}

/// <summary>A single reference category (stable key + friendly label).</summary>
public record LookupCategory(string Key, string Label);

/// <summary>A titled group of related reference categories for the left nav.</summary>
public record LookupCategoryGroup(string Title, IReadOnlyList<LookupCategory> Categories);

/// <summary>View model for the reference-data browse page.</summary>
public class LookupsIndexViewModel
{
    public IReadOnlyList<LookupCategoryGroup> Groups { get; init; } = new List<LookupCategoryGroup>();
    public string SelectedEntity { get; init; } = string.Empty;
    public string SelectedLabel { get; init; } = string.Empty;
    public IReadOnlyList<LookupItem> Items { get; init; } = new List<LookupItem>();
}
