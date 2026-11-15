using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

public interface IAuditLogger
{
    Task LogAsync(string action, string? entityType = null, int? entityId = null, string? details = null, CancellationToken ct = default);
}

/// <summary>Writes an immutable audit record on its own DbContext scope (compliance / traceability).</summary>
public class AuditLogger : IAuditLogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _http;

    public AuditLogger(IServiceScopeFactory scopeFactory, IHttpContextAccessor http)
    {
        _scopeFactory = scopeFactory;
        _http = http;
    }

    public async Task LogAsync(string action, string? entityType = null, int? entityId = null, string? details = null, CancellationToken ct = default)
    {
        try
        {
            var user = _http.HttpContext?.User;
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditLogs.Add(new AuditLog
            {
                UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = user?.FindFirstValue(ClaimTypes.Name) ?? user?.Identity?.Name,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details is { Length: > 600 } d ? d[..600] : details,
                Ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
            });
            await db.SaveChangesAsync(ct);
        }
        catch { /* auditing must never break the request */ }
    }
}
