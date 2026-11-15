using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Infrastructure.Data;

namespace MindAeonDermaAI.Web.Controllers;

/// <summary>
/// Builds an interactive biomarker → domain → biological-concept knowledge graph for one assessment.
/// Node colors reflect this subject's z-scores. Serialized to JSON and rendered with vis-network.
/// Non-diagnostic, research use only.
/// </summary>
[Authorize]
public class KnowledgeGraphController : Controller
{
    private readonly AppDbContext _db;

    public KnowledgeGraphController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int id, CancellationToken ct)
    {
        // Assessment + subject code (id == assessmentId).
        var assessment = await _db.Assessments
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (assessment is null) return NotFound();

        // Measured biomarkers for this assessment, with domain.
        var measurements = await _db.ProteinMeasurements
            .Include(m => m.Biomarker!).ThenInclude(b => b.Domain)
            .Where(m => m.AssessmentId == id && m.Biomarker != null)
            .ToListAsync(ct);

        // All domains present among the measured biomarkers (for hub nodes).
        var domains = measurements
            .Select(m => m.Biomarker!.Domain)
            .Where(d => d != null)
            .Select(d => d!)
            .DistinctBy(d => d.Id)
            .ToList();

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        // ---- Concept nodes (fixed hubs) ----
        // Abstract biological concepts the domains feed into.
        const string cInflammation = "concept:Inflammation";
        const string cCollagen = "concept:Collagen";
        const string cEcm = "concept:ECM";
        const string cHydration = "concept:Hydration";
        const string cBarrier = "concept:Barrier";

        (string Id, string Label)[] concepts =
        {
            (cInflammation, "Inflammation"),
            (cCollagen, "Collagen"),
            (cEcm, "ECM"),
            (cHydration, "Hydration"),
            (cBarrier, "Barrier"),
        };
        foreach (var (cid, label) in concepts)
            nodes.Add(new GraphNode(cid, label, "concept", "#0b2340", 26, "Biological concept"));

        // ---- Domain hub nodes ----
        foreach (var d in domains)
        {
            var domId = $"domain:{d.Id}";
            nodes.Add(new GraphNode(domId, d.Name, "domain", "#12a594", 34, "Biological domain"));

            // Domain → relevant concept(s), keyed by seed domain code.
            switch (d.Code)
            {
                case "DOMAIN_COLLAGEN":
                    edges.Add(new GraphEdge(domId, cCollagen));
                    edges.Add(new GraphEdge(domId, cEcm));
                    break;
                case "DOMAIN_BARRIER":
                    edges.Add(new GraphEdge(domId, cBarrier));
                    edges.Add(new GraphEdge(domId, cHydration));
                    edges.Add(new GraphEdge(domId, cInflammation));
                    break;
                case "DOMAIN_DEEPECM":
                    edges.Add(new GraphEdge(domId, cEcm));
                    break;
            }
        }

        // ---- Biomarker nodes (one per measured marker), colored by z-score ----
        foreach (var m in measurements)
        {
            var b = m.Biomarker!;
            var bioId = $"bio:{b.Id}";
            var color = ColorForZ(m.ZScore, b.NeedDirection);
            var zText = m.ZScore.HasValue ? $"z = {m.ZScore.Value:0.0}" : "not measured";
            nodes.Add(new GraphNode(bioId, b.Code ?? b.Name, "biomarker", color, 18, $"{b.Name} · {zText}"));

            if (b.Domain != null)
                edges.Add(new GraphEdge(bioId, $"domain:{b.Domain.Id}"));
        }

        var vm = new KnowledgeGraphViewModel(
            assessment.Id,
            assessment.Subject?.Code ?? "—",
            nodes,
            edges);
        return View(vm);
    }

    // Map a z-score to a color, honoring the marker's need direction.
    // Red/orange = high need (driving deviation), grey/teal = near reference, green = favorable.
    private static string ColorForZ(double? z, Core.Enums.NeedDirection dir)
    {
        if (z is null) return "#cbd5e1"; // grey — missing

        var v = z.Value;
        // Signed "need pressure": positive = worse than reference for this marker.
        double need = dir switch
        {
            Core.Enums.NeedDirection.HighIsNeed => v,             // high value = need
            Core.Enums.NeedDirection.LowIsNeed => -v,             // low value = need
            _ => Math.Abs(v) - 0.5,                               // dysregulation either way
        };

        if (need >= 1.0) return "#dc2626";  // red — high need
        if (need >= 0.4) return "#f59e0b";  // orange — moderate need
        if (need <= -0.8) return "#16a34a"; // green — favorable
        return "#5e6b7a";                    // grey/teal — near reference
    }
}

// ---- In-file graph model (plain data serialized to JSON for vis-network) ----

/// <summary>A graph node. <paramref name="Group"/> distinguishes concept/domain/biomarker.</summary>
public record GraphNode(string Id, string Label, string Group, string Color, int Size, string Title);

/// <summary>A directed edge from one node to another.</summary>
public record GraphEdge(string From, string To);

/// <summary>View model: graph data plus assessment context.</summary>
public record KnowledgeGraphViewModel(
    int AssessmentId,
    string SubjectCode,
    IReadOnlyList<GraphNode> Nodes,
    IReadOnlyList<GraphEdge> Edges);
