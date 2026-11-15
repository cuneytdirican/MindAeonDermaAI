using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Enums;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Services;

public interface IAssistantService
{
    Task<bool> AiReadyAsync(CancellationToken ct = default);
    /// <summary>Answer a question grounded in a read-only snapshot of the platform data.
    /// <paramref name="pageContext"/> tells the assistant which screen the user is viewing;
    /// <paramref name="mode"/> = "patient" switches to a plain-language, patient-facing persona.</summary>
    Task<string> AskAsync(IReadOnlyList<AiMessage> history, int? assessmentId, string? pageContext = null, string? mode = null, CancellationToken ct = default);
}

/// <summary>
/// Data-grounded conversational assistant. It never lets the model touch the database directly; instead it
/// builds a compact, bounded, read-only snapshot (population stats, per-assessment findings, biomarker
/// reference, glossary) and asks the configured LLM to answer/analyze strictly from that snapshot.
/// Non-diagnostic by design.
/// </summary>
public class AssistantService : IAssistantService
{
    private readonly AppDbContext _db;
    private readonly IAiOrchestrator _ai;

    public AssistantService(AppDbContext db, IAiOrchestrator ai)
    {
        _db = db;
        _ai = ai;
    }

    private const string Persona = """
        You are the MindAeon Dermalytics AI assistant, embedded in a research-grade, NON-DIAGNOSTIC skin-biology
        platform. You help staff explore and interpret the platform's own data and explain the science.

        ANSWERING MODE — decide per question:
        1) PLATFORM-DATA questions (about specific subjects, assessments, values, scores, counts, rankings,
           comparisons, "who/which/how many/what is subject X's ...") → answer STRICTLY from the DATA SNAPSHOT
           below. NEVER invent a subject, measurement, score or result. If it is not in the snapshot, say you do
           not have that data. You may compute rankings, comparisons, averages, counts and summaries FROM the snapshot.
        2) GENERAL SCIENCE questions (skin biology, dermatology concepts, what a biomarker/pathway/phenotype means,
           general mechanisms) → you MAY answer from established, well-known scientific literature and your own
           knowledge. When you do this, make clear it is GENERAL scientific background (not this platform's data),
           and describe evidence at a general level (e.g. "studies generally report..."). Do NOT fabricate specific
           citations, DOIs, author names, journals, years or exact statistics; if you are unsure, say so.
        • NEVER blend the two: do not present general knowledge as if it were this platform's measured data, and do
          not present platform data as clinical proof.
        • Stay non-diagnostic and cautious — research / cosmetic skin-biology interpretation, not medical advice.
          Use "suggests", "is compatible with", "may indicate". Do not give a personal diagnosis or treatment plan.
        • Be concise and clear for a non-specialist. Short paragraphs or bullets. Answer in the user's language.
        • When asked "what is X" about a platform metric, use the GLOSSARY.

        GLOSSARY:
        • Integrated skin-quality score (0–100, higher better): weighted blend of panel, image and lifestyle scores.
        • Panels/domains: Collagen Remodeling; Barrier/Hydration; Deep ECM/Laxity. The engine picks the highest "need".
        • Phenotypes: barrier-deficient, collagen-depleted, inflammaging, deep-ECM/laxity, fibrosis-prone,
          mechanically-stressed, young-like balanced, mixed.
        • Cross-modal concordance (0–100): agreement between image-derived and protein-derived signals.
        • Subclinical flag: molecular need high while the skin still looks well (molecular-ahead-of-visible).
        • Aging velocity: change in quality per 30 days vs a prior baseline (positive = worsening).
        • Mechanistic indices (0–100, higher healthier): Remodeling balance (MMP-1/TIMP-1), Hyaluronan axis (HAS2×CD44),
          Barrier resilience (FLG·LOR vs IL-1α).
        • Response likelihood: biological fit of each intervention category (not a treatment recommendation).
        • Exposome burden: environmental load from UV, PM2.5 and low humidity.
        """;

    private const string PatientPersona = """
        You are a warm, plain-language skin-health explainer speaking directly to the person whose results these are
        ("your skin", "you"). Explain the platform's findings simply and kindly, at a general-reader level.
        RULES: use ONLY the DATA SNAPSHOT below — never invent values. Be encouraging but honest and NON-DIAGNOSTIC:
        this is research/cosmetic skin-biology insight, not medical advice, not a diagnosis, and not a treatment plan;
        gently suggest discussing specifics with a professional. Avoid jargon (briefly define any term you must use).
        Keep answers short, friendly and concrete. Answer in the user's language.
        """;

    public Task<bool> AiReadyAsync(CancellationToken ct = default) => _ai.HasUsableProviderAsync(AiTaskKind.Chat, ct);

    public async Task<string> AskAsync(IReadOnlyList<AiMessage> history, int? assessmentId, string? pageContext = null, string? mode = null, CancellationToken ct = default)
    {
        if (!await _ai.HasUsableProviderAsync(AiTaskKind.Chat, ct))
            return "No AI provider is configured yet. An administrator can add a provider key under Admin → AI Providers, then I can answer questions about your data.";

        bool patient = string.Equals(mode, "patient", StringComparison.OrdinalIgnoreCase);
        var snapshot = await BuildSnapshotAsync(assessmentId, ct);

        var messages = new List<AiMessage>
        {
            new("system", patient ? PatientPersona : Persona),
            new("system", "DATA SNAPSHOT (current platform state, read-only):\n" + snapshot)
        };
        if (!string.IsNullOrWhiteSpace(pageContext))
            messages.Add(new("system", "CURRENT PAGE: " + pageContext +
                " — if the user says 'this', 'this page', 'here', 'this record/subject/report', they mean the record above; analyze it from the snapshot."));
        // Keep the last ~12 turns of conversation.
        foreach (var m in history.TakeLast(12)) messages.Add(m);

        var result = await _ai.ChatAsync(new ChatRequest
        {
            Messages = messages,
            Temperature = 0.3,
            MaxTokens = 1200
        }, ct: ct);

        return result.Success
            ? result.Content.Trim()
            : $"I couldn't reach the AI provider ({result.Error}). Please check Admin → AI Providers.";
    }

    private async Task<string> BuildSnapshotAsync(int? focusAssessmentId, CancellationToken ct)
    {
        var sb = new StringBuilder();

        int subjects = await _db.Subjects.CountAsync(ct);
        int assessments = await _db.Assessments.CountAsync(ct);
        int reports = await _db.ReportDocuments.CountAsync(ct);
        double? avg = await _db.ScoreResults.AnyAsync(ct) ? await _db.ScoreResults.AverageAsync(s => s.IntegratedSkinQualityScore, ct) : null;

        sb.AppendLine($"PLATFORM: subjects={subjects}, assessments={assessments}, reports={reports}, avgIntegratedScore={(avg.HasValue ? avg.Value.ToString("0.0") : "n/a")}.");

        var phenos = await _db.Assessments.Where(a => a.PrimaryPhenotypeId != null)
            .Select(a => a.PrimaryPhenotype!.Name).ToListAsync(ct);
        if (phenos.Count > 0)
            sb.AppendLine("PHENOTYPE DISTRIBUTION: " + string.Join(", ", phenos.GroupBy(p => p).Select(g => $"{g.Key}={g.Count()}")));
        var panels = await _db.Assessments.Where(a => a.SelectedPanel != null)
            .Select(a => a.SelectedPanel!.Name).ToListAsync(ct);
        if (panels.Count > 0)
            sb.AppendLine("PANEL DISTRIBUTION: " + string.Join(", ", panels.GroupBy(p => p).Select(g => $"{g.Key}={g.Count()}")));

        // Per-assessment findings (bounded).
        var rows = await _db.Assessments
            .Include(a => a.Subject).Include(a => a.SelectedPanel).Include(a => a.PrimaryPhenotype).Include(a => a.ScoreResult)
            .OrderBy(a => a.Subject!.Code).ThenBy(a => a.TimePoint)
            .Take(80)
            .ToListAsync(ct);

        sb.AppendLine("\nASSESSMENTS (one line each):");
        foreach (var a in rows)
        {
            var s = a.ScoreResult;
            string top = "";
            if (s?.ResponseLikelihoodJson is { } j)
            {
                try { using var d = JsonDocument.Parse(j); var f = d.RootElement.EnumerateArray().FirstOrDefault();
                    if (f.ValueKind == JsonValueKind.Object) top = $", topResponse={f.GetProperty("category").GetString()} {f.GetProperty("likelihood").GetDouble():0}%"; }
                catch { }
            }
            sb.Append($"- {a.Subject!.Code}");
            if (!string.IsNullOrEmpty(a.Subject.DisplayName)) sb.Append($" ({a.Subject.DisplayName})");
            sb.Append($" [{a.TimePoint}] age={a.Subject.Age}, ");
            if (s is not null)
                sb.Append($"panel={a.SelectedPanel?.Name}, phenotype={a.PrimaryPhenotype?.Name}, integrated={s.IntegratedSkinQualityScore:0}, " +
                          $"needs(col/bar/ecm)={s.CollagenRemodelingNeedScore:0}/{s.BarrierHydrationNeedScore:0}/{s.DeepEcmLaxityNeedScore:0}, " +
                          $"concordance={s.CrossModalConcordance:0}, subclinical={s.IsSubclinical}, " +
                          $"velocity={(s.AgingVelocity.HasValue ? s.AgingVelocity.Value.ToString("+0.0;-0.0") : "n/a")}, " +
                          $"indices(remodel/hyal/barrier)={Fmt(s.RemodelingBalanceIndex)}/{Fmt(s.HyaluronanAxisIndex)}/{Fmt(s.BarrierResilienceIndex)}, " +
                          $"cohortPct={Fmt(s.CohortPercentileScore)}{top}");
            else sb.Append("not analyzed yet");
            sb.AppendLine($" (assessmentId={a.Id})");
        }

        // Biomarker reference (compact).
        var bios = await _db.ProteinBiomarkers.Include(b => b.Domain).OrderBy(b => b.Domain!.DisplayOrder).ThenBy(b => b.Name).ToListAsync(ct);
        sb.AppendLine("\nBIOMARKERS (name | domain | youngRefMean±sd | cohortN):");
        foreach (var b in bios)
            sb.AppendLine($"- {b.Name} | {b.Domain?.Name} | {b.YoungRefMean:0.#}±{b.YoungRefSd:0.#} | n={b.CohortRefN}");

        // Curated evidence — the ONLY references you may cite for "why does X matter" (representative literature).
        sb.AppendLine("\nBIOMARKER EVIDENCE (curated; cite ONLY these representative references, do not invent others):");
        foreach (var b in bios)
            if (b.Code is { } code && BiomarkerEvidenceCatalog.Get(code) is { } ev)
                sb.AppendLine($"- {b.Name}: {ev.WhyItMatters} [Refs: {string.Join("; ", ev.References)}]");

        // Optional focus record with measurements.
        if (focusAssessmentId is int fid)
        {
            var f = await _db.Assessments.Include(a => a.Subject)
                .Include(a => a.Measurements).ThenInclude(m => m.Biomarker)
                .FirstOrDefaultAsync(a => a.Id == fid, ct);
            if (f is not null)
            {
                sb.AppendLine($"\nFOCUS ASSESSMENT {f.Id} ({f.Subject!.Code}) measurements (biomarker=value, z):");
                foreach (var m in f.Measurements.Where(m => m.Value != null))
                    sb.AppendLine($"- {m.Biomarker?.Name}={m.Value} (z={Fmt(m.ZScore)})");
            }
        }

        return sb.ToString();
    }

    private static string Fmt(double? v) => v.HasValue ? v.Value.ToString("0.##") : "n/a";
}
