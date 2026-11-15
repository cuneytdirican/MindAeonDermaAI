using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

public interface IBrandingService
{
    Task<BrandingSettings> GetAsync(CancellationToken ct = default);
    Task SaveAsync(BrandingSettings edited, CancellationToken ct = default);
}

/// <summary>Loads/persists the single white-label branding row, cached in memory.</summary>
public class BrandingService : IBrandingService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string Key = "branding.v1";

    public BrandingService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<BrandingSettings> GetAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(Key, out BrandingSettings? cached) && cached is not null) return cached;

        var row = await _db.BrandingSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        if (row is null)
        {
            row = new BrandingSettings();
            _db.BrandingSettings.Add(row);
            await _db.SaveChangesAsync(ct);
        }
        _cache.Set(Key, row, TimeSpan.FromMinutes(10));
        return row;
    }

    public async Task SaveAsync(BrandingSettings edited, CancellationToken ct = default)
    {
        var row = await _db.BrandingSettings.FirstOrDefaultAsync(ct) ?? new BrandingSettings();
        if (row.Id == 0 || !await _db.BrandingSettings.AnyAsync(ct)) _db.BrandingSettings.Add(row);

        row.ProductName = string.IsNullOrWhiteSpace(edited.ProductName) ? row.ProductName : edited.ProductName.Trim();
        row.Tagline = edited.Tagline?.Trim() ?? row.Tagline;
        row.PrimaryColor = string.IsNullOrWhiteSpace(edited.PrimaryColor) ? row.PrimaryColor : edited.PrimaryColor.Trim();
        row.SecondaryColor = string.IsNullOrWhiteSpace(edited.SecondaryColor) ? row.SecondaryColor : edited.SecondaryColor.Trim();
        row.LogoSvg = string.IsNullOrWhiteSpace(edited.LogoSvg) ? null : edited.LogoSvg;
        row.FooterNote = string.IsNullOrWhiteSpace(edited.FooterNote) ? null : edited.FooterNote.Trim();
        row.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        _cache.Remove(Key);
    }
}
