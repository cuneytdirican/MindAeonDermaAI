using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

/// <summary>
/// Builds (once) a fully synthetic, literature-inspired demonstration assessment and report. All values
/// are simulated for demonstration only and are clearly labelled as such — not a validated clinical output.
/// The synthetic profile is engineered to express a barrier-deficient hydration phenotype.
/// </summary>
public class ExampleReportBuilder
{
    private readonly AppDbContext _db;
    private readonly IAssessmentService _assessment;
    private readonly IReportService _report;

    public const string DemoSubjectCode = "DEMO-0001";

    public ExampleReportBuilder(AppDbContext db, IAssessmentService assessment, IReportService report)
    {
        _db = db;
        _assessment = assessment;
        _report = report;
    }

    public async Task<int> EnsureAsync(CancellationToken ct = default)
    {
        var existing = await _db.Assessments
            .Include(a => a.Subject)
            .Where(a => a.Subject!.Code == DemoSubjectCode)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(ct);
        if (existing is not null) return existing.Id;

        int? L<T>(DbSet<T> set, string code) where T : LookupBase =>
            set.Where(x => x.Code == code).Select(x => (int?)x.Id).FirstOrDefault();

        var subject = new Subject
        {
            Code = DemoSubjectCode,
            DisplayName = "Synthetic demo subject",
            Age = 47,
            HeightCm = 166,
            WeightKg = 63,
            Bmi = Math.Round(63 / Math.Pow(1.66, 2), 1),
            SexId = L(_db.Sexes, "F"),
            SkinTypeId = L(_db.SkinTypes, "DRY"),
            FitzpatrickTypeId = L(_db.FitzpatrickTypes, "FZ2"),
            MenopauseStatusId = L(_db.MenopauseStatuses, "POST"),
            SmokingStatusId = L(_db.SmokingStatuses, "FORMER"),
            AlcoholUseId = L(_db.AlcoholUses, "OCC"),
            SleepQualityId = L(_db.SleepQualities, "POOR"),
            StressLevelId = L(_db.StressLevels, "HIGH"),
            UVExposureLevelId = L(_db.UVExposureLevels, "HIGH")
        };
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync(ct);

        var assessment = new Assessment
        {
            SubjectId = subject.Id,
            InterventionCategoryId = L(_db.InterventionCategories, "BARRIER"),
            TimePoint = TimePointType.Baseline,
            Status = AssessmentStatus.DataEntry,
            AssayBatch = "BATCH-DEMO", ExposomeUvIndex = 6, ExposomePm25 = 42, ExposomeHumidity = 30
        };
        _db.Assessments.Add(assessment);
        await _db.SaveChangesAsync(ct);

        // Synthetic biomarker values expressed as a barrier-deficient / inflammaging-leaning profile.
        // Barrier markers depressed (~-1.6 SD), IL-1α elevated (~+1.6 SD); other domains near reference.
        var offsets = new Dictionary<string, double>
        {
            ["FLG"] = -1.7, ["LOR"] = -1.5, ["AQP3"] = -1.6, ["CD44"] = -1.3, ["HAS2"] = -1.4, ["IL1A"] = +1.7,
            ["COL1A1"] = -0.4, ["COL3A1"] = -0.3, ["MMP1"] = +0.5, ["TIMP1"] = -0.2, ["ELN"] = -0.5, ["TGFB1"] = +0.2,
            ["FN1"] = +0.4, ["POSTN"] = +0.3, ["DCN"] = -0.4, ["MMP2"] = +0.4, ["VEGFA"] = -0.3
        };

        var biomarkers = await _db.ProteinBiomarkers.ToListAsync(ct);
        foreach (var b in biomarkers)
        {
            var offset = offsets.TryGetValue(b.Code ?? "", out var o) ? o : 0;
            var value = Math.Round(b.YoungRefMean + offset * b.YoungRefSd, 1);
            _db.ProteinMeasurements.Add(new ProteinMeasurement
            {
                AssessmentId = assessment.Id,
                BiomarkerId = b.Id,
                Value = value,
                UnitId = b.UnitId
            });
        }

        // Synthetic pre-scored image (no file) so the image component contributes to the demo.
        _db.SkinImages.Add(new SkinImage
        {
            AssessmentId = assessment.Id,
            StoredFileName = "synthetic-demo",
            ContentType = "image/jpeg",
            IsAnalyzed = true,
            HydrationScore = 54, TextureSmoothnessScore = 63, ElasticityScore = 66, WrinkleBurdenScore = 61,
            PigmentationUniformityScore = 70, RednessHealthScore = 58, PoreVisibilityScore = 64,
            BrightnessScore = 60, OverallSkinQualityScore = 61,
            AnalysisNotes = "Synthetic demonstration scores (not from a real image)."
        });

        // Regional zone heatmap + instrumental readings so the example showcases every feature.
        var zoneIds = _db.FacialZones.Where(z => z.Code != null).AsEnumerable().ToDictionary(z => z.Code!, z => z.Id);
        var zonePattern = new (string Code, int O, int W, int R)[]
        {
            ("FOREHEAD", -4, -10, 2), ("PERIORBITAL", -8, -16, 0), ("CHEEK_L", 2, 4, -10), ("CHEEK_R", 1, 3, -9),
            ("PERIORAL", -3, -6, 1), ("TZONE", 0, 2, -4), ("CHIN", 3, 5, -2)
        };
        foreach (var z in zonePattern)
        {
            if (!zoneIds.TryGetValue(z.Code, out var fzId)) continue;
            _db.ZoneScores.Add(new ZoneScore
            {
                AssessmentId = assessment.Id, FacialZoneId = fzId,
                Overall = Math.Clamp(61 + z.O, 0, 100), Hydration = Math.Clamp(54 + z.O, 0, 100),
                Wrinkle = Math.Clamp(61 + z.W, 0, 100), RednessHealth = Math.Clamp(58 + z.R, 0, 100),
                Pigmentation = Math.Clamp(70 + z.O / 2, 0, 100)
            });
        }
        var instrIds = _db.InstrumentTypes.Where(t => t.Code != null).AsEnumerable().ToDictionary(t => t.Code!, t => t.Id);
        foreach (var (code, val) in new[] { ("TEWL", 21.0), ("CORNEO", 33.0), ("ERYTHEMA", 55.0) })
            if (instrIds.TryGetValue(code, out var itId))
                _db.InstrumentalReadings.Add(new InstrumentalReading { AssessmentId = assessment.Id, InstrumentTypeId = itId, Value = val });

        await _db.SaveChangesAsync(ct);

        await _assessment.RunAnalysisAsync(assessment.Id, ct);
        await _report.GenerateAsync(assessment.Id, ct);
        return assessment.Id;
    }
}
