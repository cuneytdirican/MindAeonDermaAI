using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Core.Ai;

/// <summary>Low-level client that talks to one provider family (OpenAI-compatible or Anthropic).</summary>
public interface IAiClient
{
    AiProviderType ProviderType { get; }
    Task<AiResult> ChatAsync(ResolvedProvider provider, ChatRequest request, CancellationToken ct = default);
    Task<AiResult> VisionAsync(ResolvedProvider provider, VisionRequest request, CancellationToken ct = default);
}

/// <summary>
/// Resolves the appropriate provider/model for a task from DB configuration, decrypts the key,
/// dispatches to the right <see cref="IAiClient"/>, and logs the call.
/// </summary>
public interface IAiOrchestrator
{
    /// <summary>Run a chat completion using the configured default (or a specific provider).</summary>
    Task<AiResult> ChatAsync(ChatRequest request, int? providerId = null, int? assessmentId = null, CancellationToken ct = default);

    /// <summary>Run a vision analysis using a vision-capable provider/model.</summary>
    Task<AiResult> VisionAsync(VisionRequest request, int? providerId = null, int? assessmentId = null, CancellationToken ct = default);

    /// <summary>Test connectivity/credentials for a provider without persisting a report.</summary>
    Task<AiResult> TestConnectionAsync(int providerId, CancellationToken ct = default);

    /// <summary>Whether at least one enabled provider has an API key or is a reachable custom endpoint.</summary>
    Task<bool> HasUsableProviderAsync(AiTaskKind kind, CancellationToken ct = default);
}

/// <summary>Encrypts/decrypts provider API keys at rest via ASP.NET Data Protection.</summary>
public interface ISecretProtector
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
