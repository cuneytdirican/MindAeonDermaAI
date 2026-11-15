using System.ComponentModel.DataAnnotations;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A configured AI backend. Seeded with connection templates for well-known providers (OpenAI,
/// Anthropic/Claude, DeepSeek, Gemini, Azure) plus a Custom template so a customer can register their
/// own LLM server (base URL + key + model). API keys are encrypted at rest.
/// </summary>
public class AiProvider
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public AiProviderType ProviderType { get; set; } = AiProviderType.OpenAiCompatible;

    /// <summary>Base URL of the API (e.g. https://api.deepseek.com). Editable for Custom providers.</summary>
    [MaxLength(400)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Data-Protection–encrypted API key. Null until the admin sets it.</summary>
    public string? ApiKeyEncrypted { get; set; }

    [MaxLength(120)]
    public string? DefaultChatModel { get; set; }

    [MaxLength(120)]
    public string? DefaultVisionModel { get; set; }

    /// <summary>Optional extra HTTP headers as a JSON object (for custom auth schemes).</summary>
    public string? ExtraHeadersJson { get; set; }

    public bool IsEnabled { get; set; } = true;

    /// <summary>Seeded template rows are protected from deletion.</summary>
    public bool IsSystem { get; set; }

    /// <summary>Marks the provider used by default when a task does not specify one.</summary>
    public bool IsDefault { get; set; }

    public ICollection<AiModel> Models { get; set; } = new List<AiModel>();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool HasApiKey => !string.IsNullOrEmpty(ApiKeyEncrypted);
}

/// <summary>A specific model offered by a provider, with its capabilities.</summary>
public class AiModel
{
    public int Id { get; set; }

    public int ProviderId { get; set; }
    public AiProvider? Provider { get; set; }

    [Required, MaxLength(120)]
    public string ModelId { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? DisplayName { get; set; }

    public ModelCapability Capabilities { get; set; } = ModelCapability.Chat;

    public bool IsDefault { get; set; }
}

/// <summary>Audit record of an AI call (no subject PII beyond the assessment reference).</summary>
public class AiRequestLog
{
    public int Id { get; set; }

    public int? ProviderId { get; set; }
    public AiProvider? Provider { get; set; }

    [MaxLength(120)]
    public string? Model { get; set; }

    public AiTaskKind TaskKind { get; set; }

    public int? AssessmentId { get; set; }

    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int LatencyMs { get; set; }

    public bool Success { get; set; }
    [MaxLength(1000)]
    public string? Error { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
