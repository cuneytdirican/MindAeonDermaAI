using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Research data export: flat CSV tables and a combined JSON bundle for downstream statistical tooling
/// (R, Python/pandas, SPSS, SAS). Restricted to Admin/Clinician roles. Anonymized subjects have their
/// display name suppressed; the non-identifying code is retained so records remain linkable across files.
/// </summary>
[Authorize(Roles = "Admin,Clinician")]
public class ExportController : Controller
{
    private readonly AppDbContext _db;

    public ExportController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Landing page with download buttons and a research-use note/disclaimer.</summary>
    public IActionResult Index() => View();

    // ---- CSV exports ----

    public async Task<IActionResult> SubjectsCsv(CancellationToken ct)
    {
        // Project to memory first (no string-building inside the SQL translation).
        var rows = await _db.Subjects
            .OrderBy(s => s.Code)
            .Select(s => new
            {
                s.Code,
                s.DisplayName,
                s.Age,
                Sex = s.Sex != null ? s.Sex.Name : null,
                s.Bmi,
                s.ConsentGiven,
                s.Anonymized,
                s.CreatedAtUtc
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.Append(CsvRow("Code", "DisplayName", "Age", "Sex", "Bmi", "ConsentGiven", "Anonymized", "CreatedAtUtc"));
        foreach (var r in rows)
        {
            // Respect anonymization: suppress display name, keep the (non-identifying) code.
            var name = r.Anonymized ? "" : r.DisplayName ?? "";
            sb.Append(CsvRow(
                r.Code,
                name,
                Num(r.Age),
                r.Sex ?? "",
                Num(r.Bmi),
                r.ConsentGiven ? "true" : "false",
                r.Anonymized ? "true" : "false",
                Iso(r.CreatedAtUtc)));
        }

        return CsvFile(sb, "subjects.csv");
    }

    public async Task<IActionResult> AssessmentsCsv(CancellationToken ct)
    {
        var rows = await _db.Assessments
            .Where(a => a.ScoreResult != null)
            .OrderBy(a => a.Subject!.Code).ThenBy(a => a.Id)
            .Select(a => new
            {
                SubjectCode = a.Subject!.Code,
                AssessmentId = a.Id,
                a.TimePoint,
                a.AssessmentDate,
                a.SelectedPanelId,
                a.PrimaryPhenotypeId,
                Score = a.ScoreResult!
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.Append(CsvRow(
            "SubjectCode", "AssessmentId", "TimePoint", "AssessmentDate",
            "IntegratedSkinQualityScore", "CollagenRemodelingNeedScore", "BarrierHydrationNeedScore",
            "DeepEcmLaxityNeedScore", "CrossModalConcordance", "BiologicalSkinAge", "OverallConfidence",
            "BenchmarkPercentile", "SelectedPanelId", "PrimaryPhenotypeId"));
        foreach (var r in rows)
        {
            var s = r.Score;
            sb.Append(CsvRow(
                r.SubjectCode,
                r.AssessmentId.ToString(CultureInfo.InvariantCulture),
                r.TimePoint.ToString(),
                Iso(r.AssessmentDate),
                Num(s.IntegratedSkinQualityScore),
                Num(s.CollagenRemodelingNeedScore),
                Num(s.BarrierHydrationNeedScore),
                Num(s.DeepEcmLaxityNeedScore),
                Num(s.CrossModalConcordance),
                Num(s.BiologicalSkinAge),
                Num(s.OverallConfidence),
                Num(s.BenchmarkPercentile),
                Num(r.SelectedPanelId),
                Num(r.PrimaryPhenotypeId)));
        }

        return CsvFile(sb, "assessments.csv");
    }

    public async Task<IActionResult> MeasurementsCsv(CancellationToken ct)
    {
        // Long / tidy format: one row per (assessment, biomarker) measurement.
        var rows = await _db.ProteinMeasurements
            .OrderBy(m => m.Assessment!.Subject!.Code).ThenBy(m => m.AssessmentId).ThenBy(m => m.BiomarkerId)
            .Select(m => new
            {
                SubjectCode = m.Assessment!.Subject!.Code,
                m.AssessmentId,
                BiomarkerCode = m.Biomarker!.Code,
                BiomarkerName = m.Biomarker!.Name,
                m.Value,
                m.ZScore,
                m.PercentChange,
                m.IsOutlier
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.Append(CsvRow(
            "SubjectCode", "AssessmentId", "BiomarkerCode", "BiomarkerName",
            "Value", "ZScore", "PercentChange", "IsOutlier"));
        foreach (var r in rows)
        {
            sb.Append(CsvRow(
                r.SubjectCode,
                r.AssessmentId.ToString(CultureInfo.InvariantCulture),
                r.BiomarkerCode ?? "",
                r.BiomarkerName,
                Num(r.Value),
                Num(r.ZScore),
                Num(r.PercentChange),
                r.IsOutlier ? "true" : "false"));
        }

        return CsvFile(sb, "measurements.csv");
    }

    // ---- JSON bundle ----

    public async Task<IActionResult> BundleJson(CancellationToken ct)
    {
        var subjects = await _db.Subjects
            .OrderBy(s => s.Code)
            .Select(s => new
            {
                s.Code,
                DisplayName = s.Anonymized ? "" : s.DisplayName,
                s.Age,
                Sex = s.Sex != null ? s.Sex.Name : null,
                s.Bmi,
                s.ConsentGiven,
                s.Anonymized,
                s.CreatedAtUtc
            })
            .ToListAsync(ct);

        var assessments = await _db.Assessments
            .Where(a => a.ScoreResult != null)
            .OrderBy(a => a.Subject!.Code).ThenBy(a => a.Id)
            .Select(a => new
            {
                SubjectCode = a.Subject!.Code,
                AssessmentId = a.Id,
                TimePoint = a.TimePoint.ToString(),
                a.AssessmentDate,
                a.SelectedPanelId,
                a.PrimaryPhenotypeId,
                Scores = new
                {
                    a.ScoreResult!.IntegratedSkinQualityScore,
                    a.ScoreResult!.CollagenRemodelingNeedScore,
                    a.ScoreResult!.BarrierHydrationNeedScore,
                    a.ScoreResult!.DeepEcmLaxityNeedScore,
                    a.ScoreResult!.CrossModalConcordance,
                    a.ScoreResult!.BiologicalSkinAge,
                    a.ScoreResult!.OverallConfidence,
                    a.ScoreResult!.BenchmarkPercentile
                }
            })
            .ToListAsync(ct);

        var measurements = await _db.ProteinMeasurements
            .OrderBy(m => m.Assessment!.Subject!.Code).ThenBy(m => m.AssessmentId).ThenBy(m => m.BiomarkerId)
            .Select(m => new
            {
                SubjectCode = m.Assessment!.Subject!.Code,
                m.AssessmentId,
                BiomarkerCode = m.Biomarker!.Code,
                BiomarkerName = m.Biomarker!.Name,
                m.Value,
                m.ZScore,
                m.PercentChange,
                m.IsOutlier
            })
            .ToListAsync(ct);

        var bundle = new
        {
            exportedAtUtc = DateTime.UtcNow,
            disclaimer = "Research use only. Not for clinical or diagnostic use. Reference statistics may be synthetic.",
            subjects,
            assessments,
            measurements
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };
        var json = JsonSerializer.Serialize(bundle, options);
        return JsonFile(json, "mindaeon-export.json");
    }

    // ---- Helpers ----

    /// <summary>Builds a single CSV line (fields escaped) terminated with CRLF.</summary>
    private static string CsvRow(params string[] fields)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < fields.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(CsvEscape(fields[i]));
        }
        sb.Append("\r\n");
        return sb.ToString();
    }

    /// <summary>Quotes a field when it contains a comma, quote, CR or LF; doubles embedded quotes.</summary>
    private static string CsvEscape(string? value)
    {
        var v = value ?? "";
        if (v.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            return v;
        return "\"" + v.Replace("\"", "\"\"") + "\"";
    }

    private static string Num(double? value) =>
        value.HasValue ? value.Value.ToString("R", CultureInfo.InvariantCulture) : "";

    private static string Num(int? value) =>
        value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "";

    private static string Iso(DateTime value) =>
        value.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

    // Emit a UTF-8 BOM so Excel opens the file with correct encoding.
    private static readonly byte[] Bom = { 0xEF, 0xBB, 0xBF };

    private FileContentResult CsvFile(StringBuilder sb, string name) =>
        WithBom(sb.ToString(), "text/csv", name);

    private FileContentResult JsonFile(string json, string name) =>
        WithBom(json, "application/json", name);

    private FileContentResult WithBom(string body, string contentType, string name)
    {
        var payload = Encoding.UTF8.GetBytes(body);
        var bytes = new byte[Bom.Length + payload.Length];
        Buffer.BlockCopy(Bom, 0, bytes, 0, Bom.Length);
        Buffer.BlockCopy(payload, 0, bytes, Bom.Length, payload.Length);
        return File(bytes, contentType, name);
    }
}
