using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Infrastructure.Ai;

/// <summary>
/// Resolves provider config from the DB, decrypts the API key, dispatches to the correct client family,
/// and audits every call. Vision tasks prefer a provider that has a configured vision model.
/// </summary>
public class AiOrchestrator : IAiOrchestrator
{
    private readonly AppDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly IEnumerable<IAiClient> _clients;
    private readonly IServiceScopeFactory _scopeFactory;

    public AiOrchestrator(AppDbContext db, ISecretProtector protector, IEnumerable<IAiClient> clients,
        IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _protector = protector;
        _clients = clients;
        _scopeFactory = scopeFactory;
    }

    public async Task<AiResult> ChatAsync(ChatRequest request, int? providerId = null, int? assessmentId = null, CancellationToken ct = default)
    {
        var provider = await PickProviderAsync(AiTaskKind.Chat, providerId, ct);
        if (provider is null) return AiResult.Fail("No enabled AI provider with an API key is configured.");

        var resolved = Resolve(provider, AiTaskKind.Chat, request.Model);
        var client = ClientFor(provider.ProviderType);
        if (client is null) return AiResult.Fail($"No client for provider type {provider.ProviderType}.");

        var result = await client.ChatAsync(resolved, request, ct);
        await LogAsync(provider.Id, resolved.Model, AiTaskKind.Chat, assessmentId, result, ct);
        return result;
    }

    public async Task<AiResult> VisionAsync(VisionRequest request, int? providerId = null, int? assessmentId = null, CancellationToken ct = default)
    {
        var provider = await PickProviderAsync(AiTaskKind.Vision, providerId, ct);
        if (provider is null) return AiResult.Fail("No enabled vision-capable AI provider is configured.");

        var resolved = Resolve(provider, AiTaskKind.Vision, request.Model);
        var client = ClientFor(provider.ProviderType);
        if (client is null) return AiResult.Fail($"No client for provider type {provider.ProviderType}.");

        var result = await client.VisionAsync(resolved, request, ct);
        await LogAsync(provider.Id, resolved.Model, AiTaskKind.Vision, assessmentId, result, ct);
        return result;
    }

    public async Task<AiResult> TestConnectionAsync(int providerId, CancellationToken ct = default)
    {
        var provider = await _db.AiProviders.AsNoTracking().FirstOrDefaultAsync(p => p.Id == providerId, ct);
        if (provider is null) return AiResult.Fail("Provider not found.");

        var resolved = Resolve(provider, AiTaskKind.Chat, null);
        var client = ClientFor(provider.ProviderType);
        if (client is null) return AiResult.Fail($"No client for provider type {provider.ProviderType}.");

        var req = new ChatRequest
        {
            Messages = { new AiMessage("user", "Reply with the single word: OK") },
            MaxTokens = 20,
            Temperature = 0
        };
        var result = await client.ChatAsync(resolved, req, ct);
        await LogAsync(provider.Id, resolved.Model, AiTaskKind.Chat, null, result, ct);
        return result;
    }

    public async Task<bool> HasUsableProviderAsync(AiTaskKind kind, CancellationToken ct = default)
        => await PickProviderAsync(kind, null, ct) is not null;

    private async Task<AiProvider?> PickProviderAsync(AiTaskKind kind, int? providerId, CancellationToken ct)
    {
        var query = _db.AiProviders.AsNoTracking().Where(p => p.IsEnabled);
        if (providerId is not null)
            return await query.FirstOrDefaultAsync(p => p.Id == providerId, ct);

        // Usable = enabled provider that has a configured (encrypted) API key. Keyless local servers
        // must have a token set (any value if the server ignores it) to be auto-selected.
        var candidates = await query
            .Where(p => p.ApiKeyEncrypted != null)
            .OrderByDescending(p => p.IsDefault)
            .ToListAsync(ct);

        if (kind == AiTaskKind.Vision)
            return candidates.FirstOrDefault(p => !string.IsNullOrEmpty(p.DefaultVisionModel)) ?? candidates.FirstOrDefault();

        return candidates.FirstOrDefault(p => !string.IsNullOrEmpty(p.DefaultChatModel)) ?? candidates.FirstOrDefault();
    }

    private ResolvedProvider Resolve(AiProvider p, AiTaskKind kind, string? overrideModel)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(p.ExtraHeadersJson))
        {
            try { headers = JsonSerializer.Deserialize<Dictionary<string, string>>(p.ExtraHeadersJson) ?? headers; }
            catch { /* ignore malformed */ }
        }

        var model = overrideModel
                    ?? (kind == AiTaskKind.Vision ? p.DefaultVisionModel : p.DefaultChatModel)
                    ?? p.DefaultChatModel;

        return new ResolvedProvider
        {
            ProviderId = p.Id,
            Name = p.Name,
            ProviderType = p.ProviderType,
            BaseUrl = p.BaseUrl,
            ApiKey = string.IsNullOrEmpty(p.ApiKeyEncrypted) ? null : _protector.Decrypt(p.ApiKeyEncrypted),
            Model = model,
            ExtraHeaders = headers
        };
    }

    private IAiClient? ClientFor(AiProviderType type)
    {
        // Custom endpoints speak the OpenAI-compatible protocol by default.
        var effective = type == AiProviderType.Custom ? AiProviderType.OpenAiCompatible : type;
        return _clients.FirstOrDefault(c => c.ProviderType == effective);
    }

    private async Task LogAsync(int providerId, string? model, AiTaskKind kind, int? assessmentId, AiResult result, CancellationToken ct)
    {
        // Write the audit record on its own DbContext so it never flushes unrelated pending changes
        // tracked by the request-scoped context.
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AiRequestLogs.Add(new AiRequestLog
        {
            ProviderId = providerId,
            Model = model,
            TaskKind = kind,
            AssessmentId = assessmentId,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            LatencyMs = result.LatencyMs,
            Success = result.Success,
            Error = result.Error is { Length: > 1000 } e ? e[..1000] : result.Error
        });
        await db.SaveChangesAsync(ct);
    }
}
