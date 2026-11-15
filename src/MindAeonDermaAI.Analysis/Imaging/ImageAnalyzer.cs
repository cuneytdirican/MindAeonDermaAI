using System.Text.Json;
using MindAeonDermaAI.Analysis.Models;
using MindAeonDermaAI.Core.Ai;

namespace MindAeonDermaAI.Analysis.Imaging;

/// <summary>Outcome of analyzing one image: scores, QC flags and a short rationale.</summary>
public class ImageAnalysisOutcome
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public ImageScoreInput Scores { get; set; } = new();
    public bool HasLightingArtifact { get; set; }
    public bool HasMakeup { get; set; }
    public bool HasFilter { get; set; }
    public bool NonStandardAngle { get; set; }
    public string? Notes { get; set; }
    /// <summary>Per-facial-zone scores for the regional heatmap.</summary>
    public List<ZoneOutcome> Zones { get; set; } = new();
}

/// <summary>Per-zone appearance scores. ZoneCode matches the seeded FacialZone.Code.</summary>
public record ZoneOutcome(string ZoneCode, double? Overall, double? Hydration, double? Wrinkle, double? RednessHealth, double? Pigmentation);

public interface IImageAnalyzer
{
    Task<ImageAnalysisOutcome> AnalyzeAsync(byte[] imageBytes, string contentType, int? assessmentId = null, CancellationToken ct = default);
}

/// <summary>
/// Sends a skin/face image to a vision-capable model with a strict structured-scoring prompt and parses
/// the standardized 0–100 scores. Redness is reversed to a health-oriented score. No identity or
/// unrelated sensitive characteristic is requested or returned.
/// </summary>
public class ImageAnalyzer : IImageAnalyzer
{
    private readonly IAiOrchestrator _ai;

    public ImageAnalyzer(IAiOrchestrator ai) => _ai = ai;

    private const string SystemPrompt =
        "You are a cautious, objective skin-image assessment assistant for cosmetic skin-biology research. " +
        "You evaluate only visible skin-quality attributes. You must NOT identify the person, infer age, " +
        "ethnicity, identity, emotion, health conditions or any unrelated sensitive characteristic. " +
        "Return ONLY strict JSON. Every score is an integer 0–100 where 100 is the healthiest/most favorable " +
        "appearance. For redness, report 'rednessHealth' where 100 means no visible redness. If the image is " +
        "unsuitable (heavy makeup, filter, poor lighting, non-standard angle), still estimate conservatively " +
        "and set the matching quality flag to true.";

    private const string UserPrompt =
        "Assess this skin image and return JSON with exactly these keys: " +
        "hydration, texture, elasticity, wrinkle, pigmentation, rednessHealth, pore, brightness, overall " +
        "(each 0–100 integer), booleans lighting, makeup, filter, nonStandardAngle, a short string 'notes' " +
        "(max 240 chars), and a 'zones' object with per-region scores. 'zones' has keys forehead, periorbital, " +
        "cheek_left, cheek_right, perioral, tzone, chin; each is an object with overall, hydration, wrinkle, " +
        "rednessHealth, pigmentation (0–100 integers). 'wrinkle' is a wrinkle-smoothness score (100 = no wrinkles). " +
        "'overall' is the overall skin-quality score. Output JSON only, no prose.";

    private static readonly (string Json, string Code)[] ZoneMap =
    {
        ("forehead", "FOREHEAD"), ("periorbital", "PERIORBITAL"), ("cheek_left", "CHEEK_L"),
        ("cheek_right", "CHEEK_R"), ("perioral", "PERIORAL"), ("tzone", "TZONE"), ("chin", "CHIN")
    };

    public async Task<ImageAnalysisOutcome> AnalyzeAsync(byte[] imageBytes, string contentType, int? assessmentId = null, CancellationToken ct = default)
    {
        var req = new VisionRequest
        {
            SystemPrompt = SystemPrompt,
            UserPrompt = UserPrompt,
            ImageBytes = imageBytes,
            ImageContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType,
            JsonMode = true
        };

        var ai = await _ai.VisionAsync(req, assessmentId: assessmentId, ct: ct);
        if (!ai.Success)
            return new ImageAnalysisOutcome { Success = false, Error = ai.Error };

        try
        {
            var json = ExtractJson(ai.Content);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            double? Num(string key) => root.TryGetProperty(key, out var v) && v.ValueKind is JsonValueKind.Number
                ? Math.Clamp(v.GetDouble(), 0, 100) : (double?)null;
            bool Flag(string key) => root.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.True;

            return new ImageAnalysisOutcome
            {
                Success = true,
                Scores = new ImageScoreInput
                {
                    Hydration = Num("hydration"),
                    Texture = Num("texture"),
                    Elasticity = Num("elasticity"),
                    Wrinkle = Num("wrinkle"),
                    Pigmentation = Num("pigmentation"),
                    RednessHealth = Num("rednessHealth"),
                    Pore = Num("pore"),
                    Brightness = Num("brightness"),
                    Overall = Num("overall")
                },
                HasLightingArtifact = Flag("lighting"),
                HasMakeup = Flag("makeup"),
                HasFilter = Flag("filter"),
                NonStandardAngle = Flag("nonStandardAngle"),
                Notes = root.TryGetProperty("notes", out var n) ? n.GetString() : null,
                Zones = ParseZones(root)
            };
        }
        catch (Exception ex)
        {
            return new ImageAnalysisOutcome { Success = false, Error = $"Could not parse model output: {ex.Message}" };
        }
    }

    private static List<ZoneOutcome> ParseZones(JsonElement root)
    {
        var list = new List<ZoneOutcome>();
        if (!root.TryGetProperty("zones", out var zones) || zones.ValueKind != JsonValueKind.Object) return list;
        double? Num(JsonElement o, string k) => o.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number
            ? Math.Clamp(v.GetDouble(), 0, 100) : (double?)null;
        foreach (var (json, code) in ZoneMap)
        {
            if (!zones.TryGetProperty(json, out var z) || z.ValueKind != JsonValueKind.Object) continue;
            list.Add(new ZoneOutcome(code, Num(z, "overall"), Num(z, "hydration"), Num(z, "wrinkle"),
                Num(z, "rednessHealth"), Num(z, "pigmentation")));
        }
        return list;
    }

    /// <summary>Extracts the first JSON object from a possibly fenced/wrapped response.</summary>
    private static string ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        return start >= 0 && end > start ? content[start..(end + 1)] : content;
    }
}
