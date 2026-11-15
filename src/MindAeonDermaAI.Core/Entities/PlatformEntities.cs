using System.ComponentModel.DataAnnotations;

namespace MindAeonDermaAI.Core.Entities;

/// <summary>Immutable audit record of a user action (compliance / traceability).</summary>
public class AuditLog
{
    public int Id { get; set; }
    [MaxLength(450)] public string? UserId { get; set; }
    [MaxLength(160)] public string? UserName { get; set; }
    [MaxLength(80)] public string Action { get; set; } = "";
    [MaxLength(80)] public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    [MaxLength(600)] public string? Details { get; set; }
    [MaxLength(60)] public string? Ip { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>A wearable/biometric reading feeding lifestyle & exposome context (sleep, stress, HRV, recovery).</summary>
public class WearableReading
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int WearableSourceId { get; set; }
    public WearableSource? WearableSource { get; set; }
    public int WearableMetricTypeId { get; set; }
    public WearableMetricType? WearableMetricType { get; set; }

    public double Value { get; set; }
    [MaxLength(30)] public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Genomics-ready: a genetic/omics marker for a subject (import placeholder for future DNA modules).</summary>
public class OmicsMarker
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int OmicsTypeId { get; set; }
    public OmicsType? OmicsType { get; set; }

    [MaxLength(60)] public string Marker { get; set; } = "";  // e.g. gene / rsID
    [MaxLength(60)] public string? Value { get; set; }         // genotype / expression level
    [MaxLength(300)] public string? Interpretation { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
