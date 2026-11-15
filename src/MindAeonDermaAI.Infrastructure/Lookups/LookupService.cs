using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Lookups;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Infrastructure.Lookups;

/// <summary>
/// Generic implementation backing every Smart-Select. A registry maps a stable entity key to typed
/// delegates over a concrete DbSet, so searching and inline creation work uniformly across all lookup
/// tables while staying fully type-safe.
/// </summary>
public class LookupService : ILookupService
{
    private readonly AppDbContext _db;
    private readonly Dictionary<string, Registration> _registry;

    private sealed record Registration(
        Func<string?, int, CancellationToken, Task<IReadOnlyList<LookupItem>>> Search,
        Func<int, CancellationToken, Task<LookupItem?>> Get,
        Func<string, string?, string?, CancellationToken, Task<LookupCreateResult>> Create);

    public LookupService(AppDbContext db)
    {
        _db = db;
        _registry = new(StringComparer.OrdinalIgnoreCase);
        Register<Sex>("sex");
        Register<SkinType>("skinType");
        Register<FitzpatrickType>("fitzpatrickType");
        Register<SmokingStatus>("smokingStatus");
        Register<AlcoholUse>("alcoholUse");
        Register<SleepQuality>("sleepQuality");
        Register<StressLevel>("stressLevel");
        Register<UVExposureLevel>("uvExposureLevel");
        Register<MenopauseStatus>("menopauseStatus");
        Register<AestheticProcedure>("aestheticProcedure");
        Register<InterventionCategory>("interventionCategory");
        Register<MeasurementUnit>("measurementUnit");
        Register<PhenotypeType>("phenotypeType");
        Register<FacialZone>("facialZone");
        Register<InstrumentType>("instrumentType");
        Register<WearableSource>("wearableSource");
        Register<WearableMetricType>("wearableMetricType");
        Register<OmicsType>("omicsType");
    }

    private void Register<T>(string key) where T : LookupBase, new()
    {
        _registry[key] = new Registration(
            Search: async (query, take, ct) =>
            {
                IQueryable<T> q = _db.Set<T>().AsNoTracking().Where(x => x.IsActive);
                if (!string.IsNullOrWhiteSpace(query))
                {
                    var term = query.Trim();
                    q = q.Where(x => x.Name.Contains(term) || (x.Description != null && x.Description.Contains(term)));
                }
                return await q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
                    .Take(take)
                    .Select(x => new LookupItem(x.Id, x.Name, x.Description, x.IsSystem))
                    .ToListAsync(ct);
            },
            Get: async (id, ct) => await _db.Set<T>().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new LookupItem(x.Id, x.Name, x.Description, x.IsSystem))
                .FirstOrDefaultAsync(ct),
            Create: async (name, description, userId, ct) =>
            {
                name = (name ?? "").Trim();
                if (name.Length is < 1 or > 120)
                    return new LookupCreateResult(false, null, "Name must be between 1 and 120 characters.");
                if (await _db.Set<T>().AnyAsync(x => x.Name == name, ct))
                    return new LookupCreateResult(false, null, $"\"{name}\" already exists.");

                var entity = new T
                {
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    IsActive = true,
                    IsSystem = false,
                    CreatedByUserId = userId,
                    CreatedAtUtc = DateTime.UtcNow,
                    DisplayOrder = 1000
                };
                _db.Add(entity);
                await _db.SaveChangesAsync(ct);
                return new LookupCreateResult(true, new LookupItem(entity.Id, entity.Name, entity.Description, false), null);
            });
    }

    public IReadOnlyCollection<string> EntityKeys => _registry.Keys;
    public bool IsKnownEntity(string entityKey) => _registry.ContainsKey(entityKey);

    public Task<IReadOnlyList<LookupItem>> SearchAsync(string entityKey, string? query, int take = 30, CancellationToken ct = default)
        => _registry.TryGetValue(entityKey, out var r)
            ? r.Search(query, take, ct)
            : Task.FromResult<IReadOnlyList<LookupItem>>(Array.Empty<LookupItem>());

    public Task<LookupItem?> GetByIdAsync(string entityKey, int id, CancellationToken ct = default)
        => _registry.TryGetValue(entityKey, out var r) ? r.Get(id, ct) : Task.FromResult<LookupItem?>(null);

    public Task<LookupCreateResult> CreateAsync(string entityKey, string name, string? description, string? userId, CancellationToken ct = default)
        => _registry.TryGetValue(entityKey, out var r)
            ? r.Create(name, description, userId, ct)
            : Task.FromResult(new LookupCreateResult(false, null, "Unknown entity."));
}
