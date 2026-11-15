using System.ComponentModel.DataAnnotations;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>Single-row white-label branding applied across the UI, landing page and PDF.</summary>
public class BrandingSettings
{
    public int Id { get; set; } // singleton (first row)

    [Required, MaxLength(120)]
    public string ProductName { get; set; } = "MindAeon Dermalytics™";

    [MaxLength(200)]
    public string Tagline { get; set; } = "AI-Powered Multimodal Skin Biology Intelligence";

    [MaxLength(20)]
    public string PrimaryColor { get; set; } = "#12a594";

    [MaxLength(20)]
    public string SecondaryColor { get; set; } = "#0b2340";

    /// <summary>Optional custom logo as inline SVG markup (falls back to the default mark when empty).</summary>
    public string? LogoSvg { get; set; }

    [MaxLength(300)]
    public string? FooterNote { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
