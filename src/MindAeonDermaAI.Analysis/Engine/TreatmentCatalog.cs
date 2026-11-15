namespace MindAeonDermaAI.Analysis.Engine;

/// <summary>
/// Curated, literature-consistent catalog of interventions with a per-biomarker "improvement magnitude"
/// (0–1) — how strongly each treatment tends to move a marker toward its healthier direction. Effect
/// magnitudes are illustrative defaults for simulation, not clinical claims.
/// </summary>
public record Treatment(string Key, string Name, string Category, double BaseResponsiveness, Dictionary<string, double> Effects);

public static class TreatmentCatalog
{
    public static readonly Treatment[] All =
    {
        new("retinol", "Retinol", "Collagen", 70,
            new() { ["COL1A1"] = 0.8, ["COL3A1"] = 0.6, ["ELN"] = 0.4, ["MMP1"] = 0.5, ["POSTN"] = 0.2 }),
        new("peptide", "Peptide serum", "Collagen", 60,
            new() { ["COL1A1"] = 0.6, ["COL3A1"] = 0.5, ["ELN"] = 0.3, ["DCN"] = 0.3 }),
        new("vitaminc", "Vitamin C", "Antioxidant", 55,
            new() { ["COL1A1"] = 0.5, ["MMP1"] = 0.3, ["VEGFA"] = 0.2, ["IL1A"] = 0.2 }),
        new("microneedling", "Microneedling", "Procedure", 65,
            new() { ["COL1A1"] = 0.7, ["COL3A1"] = 0.6, ["ELN"] = 0.4, ["FN1"] = 0.3 }),
        new("prp", "PRP", "Procedure", 68,
            new() { ["COL1A1"] = 0.6, ["VEGFA"] = 0.4, ["TGFB1"] = 0.3, ["ELN"] = 0.3 }),
        new("laser", "Laser resurfacing", "Procedure", 72,
            new() { ["COL1A1"] = 0.7, ["ELN"] = 0.4, ["MMP2"] = 0.3, ["POSTN"] = 0.2 }),
        new("niacinamide", "Niacinamide", "Barrier", 60,
            new() { ["FLG"] = 0.6, ["AQP3"] = 0.4, ["IL1A"] = 0.5, ["HAS2"] = 0.3 }),
        new("ceramide", "Ceramide barrier cream", "Barrier", 58,
            new() { ["FLG"] = 0.7, ["LOR"] = 0.6, ["AQP3"] = 0.4, ["CD44"] = 0.3, ["IL1A"] = 0.3 }),
        new("ha", "Hyaluronic acid", "Hydration", 50,
            new() { ["HAS2"] = 0.6, ["CD44"] = 0.5, ["AQP3"] = 0.4 }),
        new("exosome", "Exosome therapy", "Advanced", 66,
            new() { ["COL1A1"] = 0.6, ["DCN"] = 0.4, ["POSTN"] = 0.3, ["HAS2"] = 0.3, ["IL1A"] = 0.3 })
    };
}
