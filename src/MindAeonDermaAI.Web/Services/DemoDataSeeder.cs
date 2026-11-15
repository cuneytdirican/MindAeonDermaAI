using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

/// <summary>
/// Seeds a rich, realistic, multi-phenotype demonstration dataset (subjects "CASE-####") so the database
/// is immediately understandable: each case is engineered toward a recognizable skin-aging archetype with
/// plausible demographics, lifestyle, biomarker values, a pre-scored image with regional zone scores,
/// instrumental (multi-omics) readings and exposome context. One case includes a baseline+follow-up pair
/// to demonstrate aging velocity. Everything is synthetic and clearly for demonstration only.
/// </summary>
public class DemoDataSeeder
{
    private readonly AppDbContext _db;
    private readonly IAssessmentService _assessment;
    private readonly IReportService _report;

    public DemoDataSeeder(AppDbContext db, IAssessmentService assessment, IReportService report)
    {
        _db = db;
        _assessment = assessment;
        _report = report;
    }

    private record Zone(string Code, int OverallDelta, int WrinkleDelta, int RednessDelta);
    private record Case(
        string Code, string Name, int Age, string Sex, string Skin, string Fitz, string Meno,
        string Smoke, string Alcohol, string Sleep, string Stress, string Uv, string Interv,
        double H, double W, string Batch, double UvIdx, double Pm25, double Humidity,
        int ImgOverall, Dictionary<string, double> Offsets, (string Code, double Val)[] Instruments,
        Dictionary<string, double>? FollowOffsets = null);

    // Regional pattern: periorbital & forehead show wrinkles first; cheeks show redness.
    private static readonly Zone[] Zones =
    {
        new("FOREHEAD", -4, -10, 2), new("PERIORBITAL", -8, -16, 0), new("CHEEK_L", 2, 4, -10),
        new("CHEEK_R", 1, 3, -9), new("PERIORAL", -3, -6, 1), new("TZONE", 0, 2, -4), new("CHIN", 3, 5, -2)
    };

    private static readonly Case[] Cases =
    {
        new("CASE-0001","Young balanced control",26,"F","NORMAL","FZ3","PRE","NEVER","OCC","GOOD","LOW","LOW","NONE",
            168,60,"BATCH-2406A",4,18,52,86,
            new(){["COL1A1"]=0.8,["COL3A1"]=0.8,["TIMP1"]=0.8,["ELN"]=0.8,["CD44"]=0.8,["AQP3"]=0.8,
                  ["FLG"]=0.8,["LOR"]=0.8,["HAS2"]=0.8,["DCN"]=0.8,["MMP1"]=-0.8,["IL1A"]=-0.8,["MMP2"]=-0.8},
            new[]{("TEWL",9.5),("CORNEO",68.0),("CUTO",0.72)}),
        new("CASE-0002","Barrier-deficient / sensitive",34,"F","DRY","FZ2","PRE","NEVER","NONE","FAIR","MOD","MOD","BARRIER",
            165,58,"BATCH-2406A",5,42,28,63,
            new(){["FLG"]=-1.8,["LOR"]=-1.6,["AQP3"]=-1.5,["CD44"]=-1.2,["HAS2"]=-1.3,["IL1A"]=0.7},
            new[]{("TEWL",22.0),("CORNEO",34.0),("ERYTHEMA",58.0)}),
        new("CASE-0003","Collagen-depleted / mature",52,"F","COMBO","FZ2","POST","FORMER","MOD","POOR","HIGH","HIGH","PEPTIDE",
            167,66,"BATCH-2406B",6,55,35,58,
            new(){["COL1A1"]=-2.0,["COL3A1"]=-1.7,["ELN"]=-1.6,["MMP1"]=1.4,["TIMP1"]=-1.0,["DCN"]=-1.1},
            new[]{("CUTO",0.42),("CORNEO",46.0),("TEWL",14.0)}),
        new("CASE-0004","Inflammaging-dominant",45,"M","SENS","FZ3","NA","NEVER","MOD","POOR","HIGH","HIGH","ANTIOX",
            178,84,"BATCH-2406B",8,88,40,60,
            new(){["IL1A"]=2.0,["MMP1"]=1.2,["MMP2"]=1.1,["FLG"]=-0.9,["TGFB1"]=1.0},
            new[]{("ERYTHEMA",72.0),("TEWL",18.0),("SEBUM",180.0)}),
        new("CASE-0005","Deep ECM / laxity",58,"F","NORMAL","FZ2","POST","NEVER","OCC","FAIR","MOD","HIGH","EXOSOME",
            160,70,"BATCH-2406C",7,60,45,55,
            new(){["DCN"]=-1.8,["POSTN"]=1.6,["MMP2"]=1.5,["FN1"]=1.3,["COL1A1"]=-1.2,["VEGFA"]=-1.1},
            new[]{("CUTO",0.36),("CORNEO",52.0),("MELANIN",42.0)}),
        new("CASE-0006","Photoaged smoker",49,"M","COMBO","FZ2","NA","HEAVY","HEAVY","FAIR","MOD","HIGH","RETINOID",
            180,88,"BATCH-2406C",9,95,30,52,
            new(){["COL1A1"]=-1.6,["ELN"]=-1.8,["MMP1"]=1.6,["MMP2"]=1.2,["POSTN"]=1.1,["IL1A"]=1.0},
            new[]{("CUTO",0.40),("MELANIN",58.0),("ERYTHEMA",50.0)}),
        new("CASE-0007","Hydration-deficient (dry climate)",40,"F","DRY","FZ3","PRE","NEVER","NONE","FAIR","MOD","MOD","BARRIER",
            170,62,"BATCH-2406D",6,35,18,64,
            new(){["AQP3"]=-1.7,["HAS2"]=-1.6,["FLG"]=-1.3,["CD44"]=-1.2,["LOR"]=-1.0},
            new[]{("TEWL",20.0),("CORNEO",30.0),("SEBUM",60.0)}),
        new("CASE-0008","Mechanically stressed tissue",55,"F","NORMAL","FZ2","POST","NEVER","OCC","GOOD","HIGH","MOD","GF",
            163,68,"BATCH-2406D",5,48,44,57,
            new(){["POSTN"]=1.9,["MMP2"]=1.4,["FN1"]=1.5,["DCN"]=-1.0,["TGFB1"]=1.2},
            new[]{("CUTO",0.45),("CORNEO",55.0),("TEWL",13.0)}),
        new("CASE-0009","Mixed phenotype",47,"F","COMBO","FZ3","PERI","FORMER","MOD","FAIR","HIGH","HIGH","SUNSCREEN",
            166,64,"BATCH-2406E",7,70,38,59,
            new(){["COL1A1"]=-1.2,["FLG"]=-1.2,["IL1A"]=1.1,["MMP1"]=1.0,["DCN"]=-1.0,["HAS2"]=-1.0},
            new[]{("TEWL",16.0),("CORNEO",44.0),("ERYTHEMA",46.0)}),
        // Longitudinal: barrier-support responder — follow-up improves the barrier markers.
        new("CASE-0010","Longitudinal responder (barrier support)",44,"F","DRY","FZ2","PRE","NEVER","OCC","POOR","MOD","MOD","BARRIER",
            169,63,"BATCH-2406E",5,40,30,60,
            new(){["FLG"]=-1.7,["LOR"]=-1.5,["AQP3"]=-1.4,["IL1A"]=1.3,["HAS2"]=-1.2},
            new[]{("TEWL",21.0),("CORNEO",33.0),("ERYTHEMA",55.0)},
            FollowOffsets: new(){["FLG"]=-0.6,["LOR"]=-0.5,["AQP3"]=-0.4,["IL1A"]=0.3,["HAS2"]=-0.3})
    };

    public async Task<int> SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Subjects.AnyAsync(s => s.Code == "CASE-0001", ct)) return 0;

        Dictionary<string, int> Map<T>(DbSet<T> set) where T : LookupBase =>
            set.Where(x => x.Code != null).AsEnumerable().ToDictionary(x => x.Code!, x => x.Id);

        var sex = Map(_db.Sexes); var skin = Map(_db.SkinTypes); var fitz = Map(_db.FitzpatrickTypes);
        var meno = Map(_db.MenopauseStatuses); var smoke = Map(_db.SmokingStatuses); var alc = Map(_db.AlcoholUses);
        var sleep = Map(_db.SleepQualities); var stress = Map(_db.StressLevels); var uv = Map(_db.UVExposureLevels);
        var interv = Map(_db.InterventionCategories); var zones = Map(_db.FacialZones); var instr = Map(_db.InstrumentTypes);
        var biomarkers = await _db.ProteinBiomarkers.ToListAsync(ct);

        int created = 0;
        foreach (var c in Cases)
        {
            int? L(Dictionary<string, int> m, string code) => m.TryGetValue(code, out var id) ? id : null;

            var subject = new Subject
            {
                Code = c.Code, DisplayName = c.Name, Age = c.Age, HeightCm = c.H, WeightKg = c.W,
                Bmi = Math.Round(c.W / Math.Pow(c.H / 100.0, 2), 1),
                SexId = L(sex, c.Sex), SkinTypeId = L(skin, c.Skin), FitzpatrickTypeId = L(fitz, c.Fitz),
                MenopauseStatusId = L(meno, c.Meno), SmokingStatusId = L(smoke, c.Smoke), AlcoholUseId = L(alc, c.Alcohol),
                SleepQualityId = L(sleep, c.Sleep), StressLevelId = L(stress, c.Stress), UVExposureLevelId = L(uv, c.Uv)
            };
            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync(ct);

            var baseId = await CreateAssessmentAsync(subject.Id, c, c.Offsets, TimePointType.Baseline,
                DateTime.UtcNow.AddDays(-90), c.ImgOverall, biomarkers, interv, zones, instr, ct);
            await _assessment.RunAnalysisAsync(baseId, ct);
            await _report.GenerateAsync(baseId, ct);
            created++;

            if (c.FollowOffsets is not null)
            {
                var fupId = await CreateAssessmentAsync(subject.Id, c, c.FollowOffsets, TimePointType.FollowUp,
                    DateTime.UtcNow.AddDays(-8), Math.Min(90, c.ImgOverall + 9), biomarkers, interv, zones, instr, ct);
                await _assessment.RunAnalysisAsync(fupId, ct);
                await _report.GenerateAsync(fupId, ct);
                created++;
            }
        }
        return created;
    }

    private async Task<int> CreateAssessmentAsync(int subjectId, Case c, Dictionary<string, double> offsets,
        TimePointType tp, DateTime date, int imgOverall, List<ProteinBiomarker> biomarkers,
        Dictionary<string, int> interv, Dictionary<string, int> zones, Dictionary<string, int> instr, CancellationToken ct)
    {
        var a = new Assessment
        {
            SubjectId = subjectId,
            InterventionCategoryId = interv.TryGetValue(c.Interv, out var iv) ? iv : null,
            TimePoint = tp, AssessmentDate = date, Status = AssessmentStatus.DataEntry,
            AssayBatch = c.Batch, ExposomeUvIndex = c.UvIdx, ExposomePm25 = c.Pm25, ExposomeHumidity = c.Humidity
        };
        _db.Assessments.Add(a);
        await _db.SaveChangesAsync(ct);

        // Protein measurements relative to the age-adjusted reference so the phenotype is expressed reliably.
        double decades = c.Age > 30 ? (c.Age - 30) / 10.0 : 0;
        foreach (var b in biomarkers)
        {
            double refMean = b.YoungRefMean + b.AgeAdjustPerDecade * decades;
            double offset = offsets.TryGetValue(b.Code ?? "", out var o) ? o : 0;
            _db.ProteinMeasurements.Add(new ProteinMeasurement
            {
                AssessmentId = a.Id, BiomarkerId = b.Id,
                Value = Math.Round(refMean + offset * b.YoungRefSd, 1), UnitId = b.UnitId
            });
        }

        // Pre-scored synthetic image with the standard appearance metrics.
        int redness = Math.Clamp(imgOverall - 4, 0, 100);
        _db.SkinImages.Add(new SkinImage
        {
            AssessmentId = a.Id, StoredFileName = "synthetic-demo", ContentType = "image/jpeg", IsAnalyzed = true,
            OverallSkinQualityScore = imgOverall, HydrationScore = imgOverall - 6, TextureSmoothnessScore = imgOverall + 2,
            ElasticityScore = imgOverall - 3, WrinkleBurdenScore = imgOverall - 5, PigmentationUniformityScore = imgOverall + 6,
            RednessHealthScore = redness, PoreVisibilityScore = imgOverall + 3, BrightnessScore = imgOverall,
            AnalysisNotes = "Synthetic demonstration scores (not from a real image)."
        });

        // Regional zone heatmap rows.
        foreach (var z in Zones)
        {
            if (!zones.TryGetValue(z.Code, out var fzId)) continue;
            _db.ZoneScores.Add(new ZoneScore
            {
                AssessmentId = a.Id, FacialZoneId = fzId,
                Overall = Math.Clamp(imgOverall + z.OverallDelta, 0, 100),
                Hydration = Math.Clamp(imgOverall - 6 + z.OverallDelta, 0, 100),
                Wrinkle = Math.Clamp(imgOverall - 5 + z.WrinkleDelta, 0, 100),
                RednessHealth = Math.Clamp(redness + z.RednessDelta, 0, 100),
                Pigmentation = Math.Clamp(imgOverall + 6 + z.OverallDelta / 2, 0, 100)
            });
        }

        // Instrumental / multi-omics readings.
        foreach (var (code, val) in c.Instruments)
            if (instr.TryGetValue(code, out var itId))
                _db.InstrumentalReadings.Add(new InstrumentalReading { AssessmentId = a.Id, InstrumentTypeId = itId, Value = val });

        await _db.SaveChangesAsync(ct);
        return a.Id;
    }
}
