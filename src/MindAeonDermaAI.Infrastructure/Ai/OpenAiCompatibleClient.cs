using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Infrastructure.Ai;

/// <summary>
/// Client for any OpenAI-compatible Chat Completions API. Handles OpenAI, DeepSeek, Gemini(-compat),
/// Azure OpenAI and any Custom endpoint. Vision uses the standard image_url content parts.
/// </summary>
public class OpenAiCompatibleClient : IAiClient
{
    private readonly IHttpClientFactory _httpFactory;
    public AiProviderType ProviderType => AiProviderType.OpenAiCompatible;

    public OpenAiCompatibleClient(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    public Task<AiResult> ChatAsync(ResolvedProvider p, ChatRequest request, CancellationToken ct = default)
    {
        var messages = new JsonArray();
        foreach (var m in request.Messages)
            messages.Add(new JsonObject { ["role"] = m.Role, ["content"] = m.Content });

        var body = new JsonObject
        {
            ["model"] = request.Model ?? p.Model,
            ["messages"] = messages,
            ["temperature"] = request.Temperature,
            ["max_tokens"] = request.MaxTokens
        };
        if (request.JsonMode)
            body["response_format"] = new JsonObject { ["type"] = "json_object" };

        return SendAsync(p, body, ct);
    }

    public Task<AiResult> VisionAsync(ResolvedProvider p, VisionRequest request, CancellationToken ct = default)
    {
        var dataUrl = $"data:{request.ImageContentType};base64,{Convert.ToBase64String(request.ImageBytes)}";
        var userContent = new JsonArray
        {
            new JsonObject { ["type"] = "text", ["text"] = request.UserPrompt },
            new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject { ["url"] = dataUrl }
            }
        };

        var messages = new JsonArray
        {
            new JsonObject { ["role"] = "system", ["content"] = request.SystemPrompt },
            new JsonObject { ["role"] = "user", ["content"] = userContent }
        };

        var body = new JsonObject
        {
            ["model"] = request.Model ?? p.Model,
            ["messages"] = messages,
            ["temperature"] = request.Temperature,
            ["max_tokens"] = request.MaxTokens
        };
        if (request.JsonMode)
            body["response_format"] = new JsonObject { ["type"] = "json_object" };

        return SendAsync(p, body, ct);
    }

    private async Task<AiResult> SendAsync(ResolvedProvider p, JsonObject body, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = _httpFactory.CreateClient("ai");
            var url = $"{p.BaseUrl.TrimEnd('/')}/chat/completions";
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(p.ApiKey))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", p.ApiKey);
            foreach (var h in p.ExtraHeaders)
                req.Headers.TryAddWithoutValidation(h.Key, h.Value);

            using var resp = await client.SendAsync(req, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            sw.Stop();

            if (!resp.IsSuccessStatusCode)
                return new AiResult { Success = false, Error = $"HTTP {(int)resp.StatusCode}: {Truncate(text)}", LatencyMs = (int)sw.ElapsedMilliseconds, ProviderName = p.Name };

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            int? pt = null, ctk = null;
            if (root.TryGetProperty("usage", out var usage))
            {
                if (usage.TryGetProperty("prompt_tokens", out var v1)) pt = v1.GetInt32();
                if (usage.TryGetProperty("completion_tokens", out var v2)) ctk = v2.GetInt32();
            }

            return new AiResult
            {
                Success = true,
                Content = content,
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
