using MindAeonDermaAI.Core.Enums;

namespace MindAeonDermaAI.Infrastructure.Seed;

/// <summary>Static literature-inspired reference data used by the seeder. All reference statistics are
/// synthetic and must never be presented as validated clinical ranges.</summary>
public static class SeedCatalog
{
    public record SimpleLookup(string Code, string Name, string? Description = null);

    public static readonly SimpleLookup[] Sexes =
    {
        new("F", "Female"), new("M", "Male"), new("U", "Prefer not to say")
    };

    public static readonly SimpleLookup[] SkinTypes =
    {
        new("NORMAL", "Normal"), new("DRY", "Dry"), new("OILY", "Oily"),
        new("COMBO", "Combination"), new("SENS", "Sensitive")
    };

    public static readonly SimpleLookup[] Fitzpatrick =
    {
        new("FZ1", "Type I", "Always burns, never tans"),
        new("FZ2", "Type II", "Usually burns, tans minimally"),
        new("FZ3", "Type III", "Sometimes burns, tans uniformly"),
        new("FZ4", "Type IV", "Rarely burns, tans easily"),
        new("FZ5", "Type V", "Very rarely burns, tans very easily"),
        new("FZ6", "Type VI", "Never burns, deeply pigmented")
    };

    public static readonly SimpleLookup[] Smoking =
    {
        new("NEVER", "Never smoker"), new("FORMER", "Former smoker"),
        new("LIGHT", "Active smoker – light"), new("HEAVY", "Active smoker – heavy")
    };

    public static readonly SimpleLookup[] Alcohol =
    {
        new("NONE", "None"), new("OCC", "Occasional"), new("MOD", "Moderate"), new("HEAVY", "Heavy")
    };

    public static readonly SimpleLookup[] Sleep =
    {
        new("GOOD", "Good"), new("FAIR", "Fair"), new("POOR", "Poor")
    };

    public static readonly SimpleLookup[] Stress =
    {
        new("LOW", "Low"), new("MOD", "Moderate"), new("HIGH", "High")
    };

    public static readonly SimpleLookup[] Uv =
    {
        new("LOW", "Low"), new("MOD", "Moderate"), new("HIGH", "High")
    };

    public static readonly SimpleLookup[] Menopause =
    {
        new("NA", "Not applicable"), new("PRE", "Pre-menopausal"),
        new("PERI", "Peri-menopausal"), new("POST", "Post-menopausal")
    };

    public static readonly SimpleLookup[] Procedures =
    {
        new("NONE", "None"), new("BOTOX", "Botulinum toxin"), new("FILLER", "Dermal filler"),
        new("PEEL", "Chemical peel"), new("LASER", "Laser resurfacing"),
        new("MICRO", "Microneedling"), new("MESO", "Mesotherapy"), new("PRP", "PRP / growth factor")
    };

    public static readonly SimpleLookup[] Interventions =
    {
        new("NONE", "No active intervention"), new("BARRIER", "Barrier / hydration cream"),
        new("RETINOID", "Retinoid"), new("PEPTIDE", "Peptide serum"),
        new("ANTIOX", "Antioxidant serum"), new("SUNSCREEN", "Daily sunscreen"),
        new("GF", "Growth-factor serum"), new("EXOSOME", "Exosome therapy")
    };

    public static readonly SimpleLookup[] Units =
    {
        new("RFU", "RFU", "Relative fluorescence units"),
        new("AU", "AU", "Arbitrary units"),
        new("NGML", "ng/mL"), new("PGML", "pg/mL"), new("UGML", "µg/mL"), new("UML", "U/mL")
    };

    public static readonly SimpleLookup[] FacialZones =
    {
        new("FOREHEAD", "Forehead"), new("PERIORBITAL", "Periorbital"),
        new("CHEEK_L", "Left cheek"), new("CHEEK_R", "Right cheek"),
        new("PERIORAL", "Perioral"), new("TZONE", "Nose / T-zone"), new("CHIN", "Chin")
    };

    public static readonly SimpleLookup[] InstrumentTypes =
    {
        new("TEWL", "TEWL (g/m²h)", "Transepidermal water loss — barrier integrity"),
        new("CORNEO", "Corneometry (AU)", "Stratum corneum hydration"),
        new("CUTO", "Cutometer R2", "Skin elasticity / firmness"),
        new("SEBUM", "Sebumetry (µg/cm²)", "Sebum secretion"),
        new("MELANIN", "Melanin index (AU)", "Pigmentation"),
        new("ERYTHEMA", "Erythema index (AU)", "Redness / vascular reactivity")
    };

    public static readonly SimpleLookup[] WearableSources =
    {
        new("APPLE", "Apple Watch"), new("GARMIN", "Garmin"), new("FITBIT", "Fitbit"),
        new("OURA", "Oura"), new("WHOOP", "Whoop"), new("MANUAL", "Manual entry")
    };

    public static readonly SimpleLookup[] WearableMetrics =
    {
        new("SLEEP_H", "Sleep duration", "hours/night"), new("SLEEP_Q", "Sleep quality", "0–100"),
        new("STRESS", "Stress", "0–100"), new("HRV", "Heart-rate variability", "ms"),
        new("RECOVERY", "Recovery", "0–100"), new("STEPS", "Steps", "count/day"), new("RHR", "Resting heart rate", "bpm")
    };

    public static readonly SimpleLookup[] OmicsTypes =
    {
        new("GENE", "Gene variant"), new("SNP", "SNP (rsID)"), new("EXPR", "Gene expression"), new("METH", "Methylation")
    };

    public static readonly SimpleLookup[] Phenotypes =
    {
        new("BARRIER_DEF", "Barrier-deficient hydration phenotype"),
        new("COLLAGEN_DEP", "Collagen-depleted remodeling phenotype"),
        new("INFLAMMAGING", "Inflammaging-dominant phenotype"),
        new("DEEP_ECM", "Deep ECM / laxity phenotype"),
        new("FIBROSIS", "Fibrosis-prone remodeling phenotype"),
        new("MECH_STRESS", "Mechanically stressed connective tissue phenotype"),
        new("YOUNG_BALANCED", "Young-like balanced phenotype"),
        new("MIXED", "Mixed phenotype")
    };

    // ---- Biological domains ----
    public const string DomainCollagen = "DOMAIN_COLLAGEN";
    public const string DomainBarrier = "DOMAIN_BARRIER";
    public const string DomainDeepEcm = "DOMAIN_DEEPECM";

    public record DomainSeed(string Code, string Name, string Description);

    public static readonly DomainSeed[] Domains =
    {
        new(DomainCollagen, "Fibroblast & Collagen Remodeling",
            "Collagen synthesis, fibroblast activity, dermal matrix strength, collagen degradation and remodeling."),
        new(DomainBarrier, "Hydration & Barrier Repair",
            "Epidermal barrier function, water transport, hyaluronic-acid signaling, superficial inflammation and hydration."),
        new(DomainDeepEcm, "Deep ECM & Connective Tissue Remodeling",
            "Deep extracellular-matrix organization, mechanical support, mechanotransduction, angiogenesis and tissue laxity.")
    };

    // ---- Panels (one per domain) ----
    public record PanelSeed(string Code, string Name, string DomainCode, string Description);

    public static readonly PanelSeed[] Panels =
    {
        new("PANEL_COLLAGEN", "Collagen Remodeling Panel", DomainCollagen,
            "Reflects collagen synthesis and degradation balance."),
        new("PANEL_BARRIER", "Barrier / Hydration Panel", DomainBarrier,
            "Reflects epidermal barrier repair and hydration biology."),
        new("PANEL_DEEPECM", "Deep ECM / Laxity Panel", DomainDeepEcm,
            "Reflects deep connective-tissue remodeling and laxity biology.")
    };

    // ---- Biomarkers ----  YoungRefMean/Sd are synthetic (AU-scaled).
    public record BiomarkerSeed(string Code, string Name, string DomainCode, NeedDirection Need,
        double YoungMean, double YoungSd, string Unit = "RFU");

    public static readonly BiomarkerSeed[] Biomarkers =
    {
        // Collagen domain
        new("COL1A1", "COL1A1 (Collagen type I)", DomainCollagen, NeedDirection.LowIsNeed, 100, 18),
        new("COL3A1", "COL3A1 (Collagen type III)", DomainCollagen, NeedDirection.LowIsNeed, 90, 16),
        new("MMP1", "MMP-1 (Matrix metalloproteinase-1)", DomainCollagen, NeedDirection.HighIsNeed, 40, 10),
        new("TIMP1", "TIMP-1 (Metalloproteinase inhibitor-1)", DomainCollagen, NeedDirection.LowIsNeed, 70, 12),
        new("ELN", "Elastin (ELN)", DomainCollagen, NeedDirection.LowIsNeed, 85, 15),
        new("TGFB1", "TGF-β1 (Transforming growth factor-β1)", DomainCollagen, NeedDirection.DysregulationIsNeed, 60, 12),
        // Barrier domain
        new("CD44", "CD44 (Hyaluronan receptor)", DomainBarrier, NeedDirection.LowIsNeed, 80, 14),
        new("AQP3", "AQP3 (Aquaporin-3)", DomainBarrier, NeedDirection.LowIsNeed, 75, 13),
        new("FLG", "Filaggrin (FLG)", DomainBarrier, NeedDirection.LowIsNeed, 95, 17),
        new("LOR", "Loricrin (LOR)", DomainBarrier, NeedDirection.LowIsNeed, 88, 15),
        new("IL1A", "IL-1α (Interleukin-1α)", DomainBarrier, NeedDirection.HighIsNeed, 35, 9),
        new("HAS2", "HAS2 (Hyaluronan synthase-2)", DomainBarrier, NeedDirection.LowIsNeed, 78, 14),
        // Deep ECM domain
        new("FN1", "Fibronectin (FN1)", DomainDeepEcm, NeedDirection.DysregulationIsNeed, 82, 15),
        new("POSTN", "Periostin (POSTN)", DomainDeepEcm, NeedDirection.DysregulationIsNeed, 55, 12),
        new("DCN", "Decorin (DCN)", DomainDeepEcm, NeedDirection.LowIsNeed, 84, 15),
        new("MMP2", "MMP-2 (Matrix metalloproteinase-2)", DomainDeepEcm, NeedDirection.HighIsNeed, 45, 11),
        new("VEGFA", "VEGF-A (Vascular endothelial growth factor-A)", DomainDeepEcm, NeedDirection.DysregulationIsNeed, 58, 12)
    };

    // ---- Panel membership (biomarker codes per panel, with weights) ----
    public record PanelMember(string PanelCode, string BiomarkerCode, double Weight);

    public static readonly PanelMember[] PanelMembers =
    {
        // Collagen Remodeling
        new("PANEL_COLLAGEN", "COL1A1", 1.2), new("PANEL_COLLAGEN", "COL3A1", 1.0),
        new("PANEL_COLLAGEN", "MMP1", 1.1), new("PANEL_COLLAGEN", "TIMP1", 0.9),
        new("PANEL_COLLAGEN", "ELN", 1.0), new("PANEL_COLLAGEN", "TGFB1", 0.9),
        // Barrier / Hydration
        new("PANEL_BARRIER", "CD44", 1.0), new("PANEL_BARRIER", "AQP3", 1.0),
        new("PANEL_BARRIER", "FLG", 1.2), new("PANEL_BARRIER", "LOR", 1.0),
        new("PANEL_BARRIER", "IL1A", 1.1), new("PANEL_BARRIER", "HAS2", 1.0),
        // Deep ECM / Laxity (COL1A1 shared)
        new("PANEL_DEEPECM", "COL1A1", 1.1), new("PANEL_DEEPECM", "FN1", 1.0),
        new("PANEL_DEEPECM", "POSTN", 1.0), new("PANEL_DEEPECM", "DCN", 1.1),
        new("PANEL_DEEPECM", "MMP2", 1.0), new("PANEL_DEEPECM", "VEGFA", 0.9)
    };

    // ---- AI provider templates (keys blank until admin sets them) ----
    public record ProviderSeed(string Name, AiProviderType Type, string BaseUrl,
        string? ChatModel, string? VisionModel, bool IsDefault = false);

    public static readonly ProviderSeed[] Providers =
    {
        new("DeepSeek", AiProviderType.OpenAiCompatible, "https://api.deepseek.com/v1",
            "deepseek-chat", null, IsDefault: true),
        new("OpenAI", AiProviderType.OpenAiCompatible, "https://api.openai.com/v1",
            "gpt-4o", "gpt-4o"),
        new("Anthropic (Claude)", AiProviderType.Anthropic, "https://api.anthropic.com/v1",
            "claude-sonnet-5", "claude-sonnet-5"),
        new("Google Gemini", AiProviderType.OpenAiCompatible,
            "https://generativelanguage.googleapis.com/v1beta/openai", "gemini-2.0-flash", "gemini-2.0-flash"),
        new("Azure OpenAI", AiProviderType.OpenAiCompatible, "https://YOUR-RESOURCE.openai.azure.com/openai/deployments/YOUR-DEPLOYMENT",
            "gpt-4o", "gpt-4o"),
        new("Custom LLM Server", AiProviderType.Custom, "http://localhost:1234/v1",
            "local-model", null)
    };
}
