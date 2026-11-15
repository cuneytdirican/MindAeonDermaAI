using System.Text;
using System.Text.Json;

namespace MindAeonDermaAI.Web.Services;

public interface IWebhookDispatcher
{
    /// <summary>Fire-and-forget outbound webhook to the configured endpoint (Webhooks:Url), if any.</summary>
    Task DispatchAsync(string eventName, object payload, CancellationToken ct = default);
}

/// <summary>Posts platform events (e.g. report.generated) to a configured webhook URL. No-op when unset.</summary>
public class WebhookDispatcher : IWebhookDispatcher
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<WebhookDispatcher> _log;

    public WebhookDispatcher(IHttpClientFactory http, IConfiguration cfg, ILogger<WebhookDispatcher> log)
    {
        _http = http;
        _cfg = cfg;
        _log = log;
    }

    public async Task DispatchAsync(string eventName, object payload, CancellationToken ct = default)
    {
        var url = _cfg["Webhooks:Url"];
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            var body = JsonSerializer.Serialize(new { @event = eventName, at = DateTime.UtcNow, data = payload });
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.TryAddWithoutValidation("X-Webhook-Event", eventName);
            var secret = _cfg["Webhooks:Secret"];
            if (!string.IsNullOrEmpty(secret)) req.Headers.TryAddWithoutValidation("X-Webhook-Secret", secret);

            var client = _http.CreateClient("webhook");
            await client.SendAsync(req, ct);
        }
        catch (Exception ex) { _log.LogWarning(ex, "Webhook dispatch failed for {Event}", eventName); }
    }
}
