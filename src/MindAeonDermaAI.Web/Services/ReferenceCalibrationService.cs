using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

public interface IReferenceCalibrationService
{
    /// <summary>Recompute each biomarker's cohort reference (mean/sd/n) from the platform's own measurements.
    /// Once a biomarker has ≥ minN readings, the engine prefers this self-calibrated reference.</summary>
    Task<int> RecalibrateAsync(int minN = 8, CancellationToken ct = default);
}

/// <summary>Self-calibrating reference: the platform's reference ranges improve as its own data grows.</summary>
public class ReferenceCalibrationService : IReferenceCalibrationService
{
    private readonly AppDbContext _db;
    public ReferenceCalibrationService(AppDbContext db) => _db = db;

    public async Task<int> RecalibrateAsync(int minN = 8, CancellationToken ct = default)
    {
        // Aggregate measured values per biomarker (baseline timepoints only, to avoid double-counting follow-ups).
        var grouped = await _db.ProteinMeasurements
            .Where(m => m.Value != null && m.Assessment!.TimePoint == Core.Enums.TimePointType.Baseline)
            .GroupBy(m => m.BiomarkerId)
            .Select(g => new { BiomarkerId = g.Key, Values = g.Select(x => x.Value!.Value).ToList() })
            .ToListAsync(ct);

        var biomarkers = await _db.ProteinBiomarkers.ToListAsync(ct);
        int updated = 0;
        foreach (var g in grouped)
        {
            if (g.Values.Count < minN) continue;
            var b = biomarkers.FirstOrDefault(x => x.Id == g.BiomarkerId);
            if (b is null) continue;
            double mean = g.Values.Average();
            double sd = Math.Sqrt(g.Values.Sum(v => (v - mean) * (v - mean)) / g.Values.Count);
            b.CohortRefMean = Math.Round(mean, 2);
            b.CohortRefSd = Math.Round(sd <= 0 ? b.YoungRefSd : sd, 2);
            b.CohortRefN = g.Values.Count;
            updated++;
        }
        await _db.SaveChangesAsync(ct);
        return updated;
    }
}
