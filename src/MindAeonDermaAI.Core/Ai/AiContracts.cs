namespace MindAeonDermaAI.Core.Ai;

/// <summary>A single chat message.</summary>
public record AiMessage(string Role, string Content);

/// <summary>Request for a text/chat completion.</summary>
public class ChatRequest
{
    public string? Model { get; set; }
    public List<AiMessage> Messages { get; set; } = new();
    public double Temperature { get; set; } = 0.4;
    public int MaxTokens { get; set; } = 4000;
    /// <summary>When true, ask the provider to return strict JSON.</summary>
    public bool JsonMode { get; set; }
}

/// <summary>Request to analyze an image with a vision-capable model.</summary>
public class VisionRequest
{
    public string? Model { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    /// <summary>Raw image bytes.</summary>
    public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    public string ImageContentType { get; set; } = "image/jpeg";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 2000;
    public bool JsonMode { get; set; } = true;
}

/// <summary>Result of an AI call.</summary>
public class AiResult
{
    public bool Success { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int LatencyMs { get; set; }
    public string? Model { get; set; }
    public string? ProviderName { get; set; }

    public static AiResult Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>Resolved provider connection details (decrypted) ready for an HTTP call.</summary>
public class ResolvedProvider
{
    public int ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Enums.AiProviderType ProviderType { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
    public Dictionary<string, string> ExtraHeaders { get; set; } = new();
}
