namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// One of the three biological skin domains: Fibroblast/Collagen Remodeling,
/// Hydration/Barrier Repair, Deep ECM/Connective-tissue Remodeling.
/// </summary>
public class BiologicalDomain : LookupBase
{
    public ICollection<ProteinBiomarker> Biomarkers { get; set; } = new List<ProteinBiomarker>();
    public Panel? Panel { get; set; }
}
