using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>Admin-only audit-log viewer: recent user actions and recent AI calls.</summary>
[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly AppDbContext _db;

    public AuditController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int take = 200, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);

        var actions = await _db.AuditLogs
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);

        // Join provider name for readability; AiRequestLog only stores ProviderId.
        var aiCalls = await _db.AiRequestLogs
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(take)
            .Select(l => new AiCallRow
            {
                Provider = l.Provider != null ? l.Provider.Name : (l.ProviderId != null ? $"#{l.ProviderId}" : "—"),
                Model = l.Model,
                TaskKind = l.TaskKind.ToString(),
                Success = l.Success,
                LatencyMs = l.LatencyMs,
                PromptTokens = l.PromptTokens,
                CompletionTokens = l.CompletionTokens,
                Error = l.Error,
                CreatedAtUtc = l.CreatedAtUtc
            })
            .ToListAsync(ct);

        return View(new AuditViewModel { Take = take, Actions = actions, AiCalls = aiCalls });
    }
}

/// <summary>Two-table audit view: user actions + AI calls.</summary>
public class AuditViewModel
{
    public int Take { get; set; }
    public List<AuditLog> Actions { get; set; } = new();
    public List<AiCallRow> AiCalls { get; set; } = new();
}

/// <summary>Flattened AI-call row for display.</summary>
public class AiCallRow
{
    public string Provider { get; set; } = "—";
    public string? Model { get; set; }
    public string TaskKind { get; set; } = "";
    public bool Success { get; set; }
    public int LatencyMs { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
