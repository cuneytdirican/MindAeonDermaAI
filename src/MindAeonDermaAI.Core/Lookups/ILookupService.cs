namespace MindAeonDermaAI.Core.Lookups;

/// <summary>Lightweight item returned to the Smart-Select widget.</summary>
public record LookupItem(int Id, string Name, string? Description, bool IsSystem);

/// <summary>Result of an inline create attempt.</summary>
public record LookupCreateResult(bool Success, LookupItem? Item, string? Error);

/// <summary>
/// Generic service powering every Smart-Select. Entities are addressed by a stable key
/// (e.g. "skinType"). Supports searching active items and defining a new item inline.
/// </summary>
public interface ILookupService
{
    /// <summary>Whether the given entity key maps to a known lookup table.</summary>
    bool IsKnownEntity(string entityKey);

    /// <summary>The set of entity keys exposed to the UI.</summary>
    IReadOnlyCollection<string> EntityKeys { get; }

    Task<IReadOnlyList<LookupItem>> SearchAsync(string entityKey, string? query, int take = 30, CancellationToken ct = default);

    Task<LookupItem?> GetByIdAsync(string entityKey, int id, CancellationToken ct = default);

    /// <summary>Create a new lookup value inline. Returns an error if it duplicates an existing name.</summary>
    Task<LookupCreateResult> CreateAsync(string entityKey, string name, string? description, string? userId, CancellationToken ct = default);
}
