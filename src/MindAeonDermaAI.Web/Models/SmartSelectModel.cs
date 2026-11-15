namespace MindAeonDermaAI.Web.Models;

/// <summary>View model for the reusable Smart-Select partial.</summary>
public class SmartSelectModel
{
    /// <summary>The hidden input's name (the bound FK property, e.g. "SkinTypeId").</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Lookup entity key understood by the Lookup API (e.g. "skinType").</summary>
    public string Entity { get; set; } = string.Empty;

    public int? SelectedId { get; set; }
    public string? SelectedName { get; set; }

    public string Placeholder { get; set; } = "Select…";
    public bool AllowCreate { get; set; } = true;
    public string CreateLabel { get; set; } = "Add new";
}
