using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Infrastructure.Ai;

/// <summary>Client for the Anthropic (Claude) Messages API, including vision content blocks.</summary>
public class AnthropicClient : IAiClient
{
    private readonly IHttpClientFactory _httpFactory;
    private const string AnthropicVersion = "2023-06-01";
    public AiProviderType ProviderType => AiProviderType.Anthropic;

    public AnthropicClient(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    public Task<AiResult> ChatAsync(ResolvedProvider p, ChatRequest request, CancellationToken ct = default)
    {
        string? system = null;
        var messages = new JsonArray();
        foreach (var m in request.Messages)
        {
            if (m.Role == "system") { system = m.Content; continue; }
            messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Content });
        }

        var body = new JsonObject
        {
            ["model"] = request.Model ?? p.Model,
            ["max_tokens"] = request.MaxTokens,
            ["temperature"] = request.Temperature,
            ["messages"] = messages
        };
        if (system is not null) body["system"] = system;

        return SendAsync(p, body, ct);
    }

    public Task<AiResult> VisionAsync(ResolvedProvider p, VisionRequest request, CancellationToken ct = default)
    {
        var content = new JsonArray
        {
            new JsonObject
            {
                ["type"] = "image",
                ["source"] = new JsonObject
                {
                    ["type"] = "base64",
                    ["media_type"] = request.ImageContentType,
                    ["data"] = Convert.ToBase64String(request.ImageBytes)
                }
            },
            new JsonObject { ["type"] = "text", ["text"] = request.UserPrompt }
        };

        var body = new JsonObject
        {
            ["model"] = request.Model ?? p.Model,
            ["max_tokens"] = request.MaxTokens,
            ["temperature"] = request.Temperature,
            ["system"] = request.SystemPrompt,
            ["messages"] = new JsonArray { new JsonObject { ["role"] = "user", ["content"] = content } }
        };

        return SendAsync(p, body, ct);
    }

    private async Task<AiResult> SendAsync(ResolvedProvider p, JsonObject body, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = _httpFactory.CreateClient("ai");
            var url = $"{p.BaseUrl.TrimEnd('/')}/messages";
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(p.ApiKey)) req.Headers.TryAddWithoutValidation("x-api-key", p.ApiKey);
            req.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
            foreach (var h in p.ExtraHeaders) req.Headers.TryAddWithoutValidation(h.Key, h.Value);

            using var resp = await client.SendAsync(req, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            sw.Stop();

            if (!resp.IsSuccessStatusCode)
                return new AiResult { Success = false, Error = $"HTTP {(int)resp.StatusCode}: {Truncate(text)}", LatencyMs = (int)sw.ElapsedMilliseconds, ProviderName = p.Name };

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var sb = new System.Text.StringBuilder();
            foreach (var block in root.GetProperty("content").EnumerateArray())
                if (block.TryGetProperty("text", out var t)) sb.Append(t.GetString());

            int? pt = null, ctk = null;
            if (root.TryGetProperty("usage", out var usage))
            {
                if (usage.TryGetProperty("input_tokens", out var v1)) pt = v1.GetInt32();
                if (usage.TryGetProperty("output_tokens", out var v2)) ctk = v2.GetInt32();
            }

            return new AiResult
            {
                Success = true,
                Content = sb.ToString(),
                PromptTokens = pt,
                CompletionTokens = ctk,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                Model = body["model"]?.GetValue<string>(),
                ProviderName = p.Name
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new AiResult { Success = false, Error = ex.Message, LatencyMs = (int)sw.ElapsedMilliseconds, ProviderName = p.Name };
        }
    }

    private static string Truncate(string s) => s.Length > 500 ? s[..500] : s;
}
