using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindAeonDermaAI.Core.Lookups;
using MindAeonDermaAI.Infrastructure.Identity;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Generic API behind every Smart-Select: search active items and define new ones inline.
/// Reviewers may search but not create; Clinicians/Admins may create.
/// </summary>
[ApiController]
[Route("api/lookup")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly ILookupService _lookups;
    private readonly UserManager<ApplicationUser> _userManager;

    public LookupController(ILookupService lookups, UserManager<ApplicationUser> userManager)
    {
        _lookups = lookups;
        _userManager = userManager;
    }

    public record CreateRequest(string Name, string? Description);

    [HttpGet("{entity}")]
    public async Task<IActionResult> Search(string entity, [FromQuery] string? q, CancellationToken ct)
    {
        if (!_lookups.IsKnownEntity(entity)) return NotFound(new { error = "Unknown entity." });
        var items = await _lookups.SearchAsync(entity, q, 30, ct);
        return Ok(items);
    }

    [HttpPost("{entity}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Clinician")]
    public async Task<IActionResult> Create(string entity, [FromBody] CreateRequest body, CancellationToken ct)
    {
        if (!_lookups.IsKnownEntity(entity)) return NotFound(new { success = false, error = "Unknown entity." });
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
            return Ok(new LookupCreateResult(false, null, "Please enter a name."));

        var userId = _userManager.GetUserId(User);
        var result = await _lookups.CreateAsync(entity, body.Name, body.Description, userId, ct);
        return Ok(result);
    }
}
