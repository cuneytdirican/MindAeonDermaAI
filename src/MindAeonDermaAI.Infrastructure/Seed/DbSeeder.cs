using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Infrastructure.Seed;

/// <summary>
/// Idempotently seeds all reference/system data (lookups, biological domains, panels, biomarkers,
/// phenotypes and AI provider templates). Safe to run on every startup: existing system rows are
/// matched by <c>Code</c>/<c>Name</c> and never duplicated.
/// </summary>
public class DbSeeder
{
    private readonly AppDbContext _db;

    public DbSeeder(AppDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedSimpleAsync(_db.Sexes, SeedCatalog.Sexes, ct);
        await SeedSimpleAsync(_db.SkinTypes, SeedCatalog.SkinTypes, ct);
        await SeedSimpleAsync(_db.FitzpatrickTypes, SeedCatalog.Fitzpatrick, ct);
        await SeedSimpleAsync(_db.SmokingStatuses, SeedCatalog.Smoking, ct);
        await SeedSimpleAsync(_db.AlcoholUses, SeedCatalog.Alcohol, ct);
        await SeedSimpleAsync(_db.SleepQualities, SeedCatalog.Sleep, ct);
        await SeedSimpleAsync(_db.StressLevels, SeedCatalog.Stress, ct);
        await SeedSimpleAsync(_db.UVExposureLevels, SeedCatalog.Uv, ct);
        await SeedSimpleAsync(_db.MenopauseStatuses, SeedCatalog.Menopause, ct);
        await SeedSimpleAsync(_db.AestheticProcedures, SeedCatalog.Procedures, ct);
        await SeedSimpleAsync(_db.InterventionCategories, SeedCatalog.Interventions, ct);
        await SeedSimpleAsync(_db.MeasurementUnits, SeedCatalog.Units, ct);
        await SeedSimpleAsync(_db.PhenotypeTypes, SeedCatalog.Phenotypes, ct);
        await SeedSimpleAsync(_db.FacialZones, SeedCatalog.FacialZones, ct);
        await SeedSimpleAsync(_db.InstrumentTypes, SeedCatalog.InstrumentTypes, ct);
        await SeedSimpleAsync(_db.WearableSources, SeedCatalog.WearableSources, ct);
        await SeedSimpleAsync(_db.WearableMetricTypes, SeedCatalog.WearableMetrics, ct);
        await SeedSimpleAsync(_db.OmicsTypes, SeedCatalog.OmicsTypes, ct);

        await SeedBiologyAsync(ct);
        await SeedProvidersAsync(ct);
    }

    private async Task SeedSimpleAsync<T>(DbSet<T> set, SeedCatalog.SimpleLookup[] items, CancellationToken ct)
        where T : LookupBase, new()
    {
        var existingCodes = await set.Where(x => x.IsSystem).Select(x => x.Code).ToListAsync(ct);
        var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        int order = 0;
        foreach (var i in items)
        {
            order += 10;
            if (existing.Contains(i.Code)) continue;
            set.Add(new T
            {
                Code = i.Code,
                Name = i.Name,
                Description = i.Description,
                DisplayOrder = order,
                IsActive = true,
                IsSystem = true
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedBiologyAsync(CancellationToken ct)
    {
        // Domains
        var domainByCode = await _db.BiologicalDomains.ToDictionaryAsync(d => d.Code!, ct);
        int order = 0;
        foreach (var d in SeedCatalog.Domains)
        {
            order += 10;
            if (domainByCode.ContainsKey(d.Code)) continue;
            var entity = new BiologicalDomain { Code = d.Code, Name = d.Name, Description = d.Description, DisplayOrder = order, IsSystem = true };
            _db.BiologicalDomains.Add(entity);
            domainByCode[d.Code] = entity;
        }
        await _db.SaveChangesAsync(ct);

        // Biomarkers
        var unitByCode = await _db.MeasurementUnits.ToDictionaryAsync(u => u.Code!, ct);
        var bioByCode = await _db.ProteinBiomarkers.ToDictionaryAsync(b => b.Code!, ct);
        foreach (var b in SeedCatalog.Biomarkers)
        {
            if (bioByCode.ContainsKey(b.Code)) continue;
            var entity = new ProteinBiomarker
            {
                Code = b.Code,
                Name = b.Name,
                DomainId = domainByCode[b.DomainCode].Id,
                NeedDirection = b.Need,
                YoungRefMean = b.YoungMean,
                YoungRefSd = b.YoungSd,
                // Stratified reference: structural/protective proteins decline with age; proteases rise.
                AgeAdjustPerDecade = b.Need switch
                {
                    Core.Enums.NeedDirection.LowIsNeed => -0.12 * b.YoungSd,
                    Core.Enums.NeedDirection.HighIsNeed => 0.10 * b.YoungSd,
                    _ => 0.05 * b.YoungSd
                },
                UnitId = unitByCode.TryGetValue(b.Unit, out var u) ? u.Id : null,
                IsSyntheticReference = true,
                IsSystem = true
            };
            _db.ProteinBiomarkers.Add(entity);
            bioByCode[b.Code] = entity;
        }
        await _db.SaveChangesAsync(ct);

        // Panels
        var panelByCode = await _db.Panels.ToDictionaryAsync(p => p.Code!, ct);
        order = 0;
        foreach (var p in SeedCatalog.Panels)
        {
            order += 10;
            if (panelByCode.ContainsKey(p.Code)) continue;
            var entity = new Panel
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                DomainId = domainByCode[p.DomainCode].Id,
                DisplayOrder = order,
                IsSystem = true
            };
            _db.Panels.Add(entity);
            panelByCode[p.Code] = entity;
        }
        await _db.SaveChangesAsync(ct);

        // Panel membership
        var existingMembers = await _db.PanelBiomarkers
            .Select(pb => new { pb.PanelId, pb.BiomarkerId }).ToListAsync(ct);
        var memberSet = existingMembers.Select(m => (m.PanelId, m.BiomarkerId)).ToHashSet();
        foreach (var m in SeedCatalog.PanelMembers)
        {
            var panelId = panelByCode[m.PanelCode].Id;
            var bioId = bioByCode[m.BiomarkerCode].Id;
            if (memberSet.Contains((panelId, bioId))) continue;
            _db.PanelBiomarkers.Add(new PanelBiomarker { PanelId = panelId, BiomarkerId = bioId, Weight = m.Weight });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedProvidersAsync(CancellationToken ct)
    {
        var existing = (await _db.AiProviders.Select(p => p.Name).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var p in SeedCatalog.Providers)
        {
            if (existing.Contains(p.Name)) continue;
            _db.AiProviders.Add(new AiProvider
            {
                Name = p.Name,
                ProviderType = p.Type,
                BaseUrl = p.BaseUrl,
                DefaultChatModel = p.ChatModel,
                DefaultVisionModel = p.VisionModel,
                IsEnabled = true,
                IsSystem = true,
                IsDefault = p.IsDefault
            });
        }
        await _db.SaveChangesAsync(ct);
    }
}
