namespace MindAeonDermaAI.Web.Models;

public class DashboardViewModel
{
    public int SubjectCount { get; set; }
    public int AssessmentCount { get; set; }
    public int ReportCount { get; set; }
    public int ConfiguredProviders { get; set; }
    public bool AiReady { get; set; }

    public List<RecentRow> Recent { get; set; } = new();
    public List<NameCount> PanelDistribution { get; set; } = new();

    public record RecentRow(int Id, string Code, string? Name, string Status, string? Panel, double? Score, DateTime Created);
    public record NameCount(string Name, int Count);
}
