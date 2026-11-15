namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// A protein panel aligned to a biological domain. The engine computes a "need score" per panel and
/// selects the highest; panels within 10 points classify as a mixed biological phenotype.
/// </summary>
public class Panel : LookupBase
{
    public int DomainId { get; set; }
    public BiologicalDomain? Domain { get; set; }

    public ICollection<PanelBiomarker> PanelBiomarkers { get; set; } = new List<PanelBiomarker>();
}

/// <summary>Join entity mapping a biomarker into a panel with its scoring weight.</summary>
public class PanelBiomarker
{
    public int Id { get; set; }

    public int PanelId { get; set; }
    public Panel? Panel { get; set; }

    public int BiomarkerId { get; set; }
    public ProteinBiomarker? Biomarker { get; set; }

    /// <summary>Relative contribution of this biomarker to the panel's need score.</summary>
    public double Weight { get; set; } = 1.0;
}
