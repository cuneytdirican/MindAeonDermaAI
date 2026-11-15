using System.ComponentModel.DataAnnotations;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>
/// Base type for every categorical / reference entity in the system. Per project constraint #2,
/// users never free-type these values — they select from a searchable Smart-Select that also allows
/// defining a new value inline. System rows (<see cref="IsSystem"/>) are seeded and cannot be deleted.
/// </summary>
public abstract class LookupBase
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional stable code used by the seeders and analysis engine.</summary>
    [MaxLength(60)]
    public string? Code { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Seeded rows are protected: they cannot be edited away or deleted by users.</summary>
    public bool IsSystem { get; set; }

    /// <summary>Identity user id of the person who defined this value inline (null for system rows).</summary>
    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
