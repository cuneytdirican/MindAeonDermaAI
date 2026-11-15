using MindAeonDermaAI.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MindAeonDermaAI.Web.Services;

/// <summary>Renders the assessment report to a branded PDF using QuestPDF.</summary>
public static class ReportPdf
{
    private static readonly string Navy = "#0B2340";
    private static readonly string Teal = "#12A594";
    private static readonly string Ink = "#1F2937";
    private static readonly string Muted = "#6B7280";
    private static readonly string Line = "#E5E7EB";

    public static byte[] Build(ReportViewModel vm)
    {
        var sections = ParseSections(vm);

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Ink).FontFamily("Segoe UI"));

                page.Header().Element(h => Header(h, vm));
                page.Content().PaddingVertical(12).Element(c => Content(c, vm, sections));
                page.Footer().Element(c => Footer(c, vm.BrandName));
            });
        }).GeneratePdf();
    }

    private static void Header(IContainer c, ReportViewModel vm)
    {
        c.BorderBottom(2).BorderColor(Teal).PaddingBottom(8).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(vm.BrandName).FontSize(18).Bold().FontColor(vm.BrandColor);
                col.Item().Text("Personalized Skin-Biology Report · Research Use").FontSize(9).FontColor(Muted);
            });
            row.ConstantItem(150).AlignRight().Column(col =>
            {
                col.Item().Text($"Subject {vm.Subject.Code}").FontSize(9).FontColor(Muted);
                col.Item().Text($"Assessment #{vm.Assessment.Id}").FontSize(9).FontColor(Muted);
                col.Item().Text(vm.Assessment.AssessmentDate.ToString("yyyy-MM-dd")).FontSize(9).FontColor(Muted);
            });
        });
    }

    private static void Content(IContainer c, ReportViewModel vm, ReportSections s)
    {
        c.Column(col =>
        {
            col.Spacing(10);

            // Integrated score banner
            col.Item().Background(Navy).Padding(12).Row(r =>
            {
                r.RelativeItem().Column(x =>
                {
                    x.Item().Text("Integrated Skin-Quality Score").FontColor("#CBD5E1").FontSize(10);
                    x.Item().Text($"{vm.Score.IntegratedSkinQualityScore:0}/100").FontColor(Colors.White).FontSize(24).Bold();
                });
                r.RelativeItem().AlignRight().Column(x =>
                {
                    x.Item().AlignRight().Text("Primary phenotype").FontColor("#CBD5E1").FontSize(10);
                    x.Item().AlignRight().Text(vm.PrimaryPhenotypeName).FontColor(Colors.White).FontSize(12).Bold();
                    x.Item().AlignRight().Text($"Selected panel: {vm.SelectedPanel?.Name}").FontColor("#CBD5E1").FontSize(9);
                });
            });

            Section(col, "Executive Summary", s.ExecutiveSummary);
            Section(col, "Personalized Interpretation", s.PersonalizedInterpretation);

            // Subject profile table
            col.Item().Text("Subject Profile").FontSize(12).Bold().FontColor(Navy);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                var subj = vm.Subject;
                KeyVal(t, "Age", subj.Age?.ToString() ?? "—"); KeyVal(t, "Sex", subj.Sex?.Name ?? "—");
                KeyVal(t, "BMI", subj.Bmi?.ToString("0.0") ?? "—"); KeyVal(t, "Skin type", subj.SkinType?.Name ?? "—");
                KeyVal(t, "Fitzpatrick", subj.FitzpatrickType?.Name ?? "—"); KeyVal(t, "Smoking", subj.SmokingStatus?.Name ?? "—");
                KeyVal(t, "UV exposure", subj.UVExposureLevel?.Name ?? "—"); KeyVal(t, "Sleep", subj.SleepQuality?.Name ?? "—");
                KeyVal(t, "Stress", subj.StressLevel?.Name ?? "—"); KeyVal(t, "Alcohol", subj.AlcoholUse?.Name ?? "—");
            });

            if (vm.PhenotypeNames.Count > 0)
                col.Item().PaddingTop(4).Text(txt =>
                {
                    txt.Span("Biological phenotype: ").SemiBold().FontColor(Navy).FontSize(9);
                    txt.Span(string.Join(" · ", vm.PhenotypeNames)).FontSize(9);
                });
            if (vm.Score.BiologicalSkinAge.HasValue)
                col.Item().PaddingTop(2).Text(txt =>
                {
                    txt.Span("Biological skin age: ").SemiBold().FontColor(Navy).FontSize(9);
                    txt.Span($"{vm.Score.BiologicalSkinAge:0} (chronological {(vm.Subject.Age?.ToString() ?? "—")}) · Face {Fmt(vm.Score.FaceAge)} · Protein {Fmt(vm.Score.ProteinAge)} · Barrier {Fmt(vm.Score.BarrierAge)} · Collagen {Fmt(vm.Score.CollagenAge)}").FontSize(9);
                });

            // Panel selection
            col.Item().PaddingTop(4).Text("Panel Selection (biological need)").FontSize(12).Bold().FontColor(Navy);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.RelativeColumn(); cd.RelativeColumn(); });
                HeadCell(t, "Panel"); HeadCell(t, "Need score"); HeadCell(t, "Selected");
                foreach (var p in vm.PanelNeeds)
                {
                    Cell(t, p.Panel); Cell(t, $"{p.NeedScore:0.0}"); Cell(t, p.Selected ? "✓" : "");
                }
            });
            Section(col, null, s.PanelSelectionNote);

            // Biomarkers
            if (vm.Biomarkers.Count > 0)
            {
                col.Item().PaddingTop(4).Text("Protein Biomarker Analysis").FontSize(12).Bold().FontColor(Navy);
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.RelativeColumn(2); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(3); });
                    HeadCell(t, "Biomarker"); HeadCell(t, "Domain"); HeadCell(t, "Value"); HeadCell(t, "z"); HeadCell(t, "Interpretation");
                    foreach (var b in vm.Biomarkers)
                    {
                        Cell(t, b.Name); Cell(t, b.Domain); Cell(t, b.Value?.ToString("0.#") ?? "—");
                        Cell(t, b.ZScore?.ToString("0.0") ?? "—"); Cell(t, b.Interpretation);
                    }
                });
                Section(col, null, s.ProteinAnalysisNote);
            }

            // Image scores
            if (vm.ImageScores.Count > 0)
            {
                col.Item().PaddingTop(4).Text("Image-Based Skin Assessment").FontSize(12).Bold().FontColor(Navy);
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.RelativeColumn(); });
                    HeadCell(t, "Metric"); HeadCell(t, "Score /100");
                    foreach (var im in vm.ImageScores) { Cell(t, im.Metric); Cell(t, im.Score?.ToString("0") ?? "—"); }
                });
                Section(col, null, s.ImageAssessmentNote);
            }

            // Advanced multimodal intelligence
            col.Item().PaddingTop(6).Text("Advanced Multimodal Intelligence").FontSize(12).Bold().FontColor(Navy);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                HeadCell(t, "Cross-modal concord."); HeadCell(t, "Molecular status"); HeadCell(t, "Aging velocity"); HeadCell(t, "Exposome burden");
                Cell(t, $"{vm.Score.CrossModalConcordance:0}/100");
                Cell(t, vm.Score.IsSubclinical ? "Subclinical signal" : "Aligned");
                Cell(t, vm.Score.AgingVelocity.HasValue ? $"{vm.Score.AgingVelocity:+0.0;-0.0}/30d" : "Baseline");
                Cell(t, vm.Score.ExposomeBurden.HasValue ? $"{vm.Score.ExposomeBurden:0}/100" : "—");
            });
            col.Item().PaddingTop(4).Text(txt =>
            {
                txt.Span("Mechanistic indices — ").SemiBold().FontColor(Navy).FontSize(9);
                txt.Span($"Remodeling {Fmt(vm.Score.RemodelingBalanceIndex)} · Hyaluronan {Fmt(vm.Score.HyaluronanAxisIndex)} · Barrier resilience {Fmt(vm.Score.BarrierResilienceIndex)} (0–100, higher = healthier).").FontSize(9);
            });
            col.Item().PaddingTop(2).Text(txt =>
            {
                txt.Span("Confidence — ").SemiBold().FontColor(Navy).FontSize(9);
                txt.Span($"protein {Fmt(vm.Score.ProteinConfidence)}% · image {Fmt(vm.Score.ImageConfidence)}% · overall {Fmt(vm.Score.OverallConfidence)}%. ").FontSize(9);
                if (vm.Score.BenchmarkPercentile.HasValue)
                    txt.Span($"Peer benchmark: {vm.Score.BenchmarkPercentile:0}th among {vm.Score.BenchmarkGroupLabel}.").FontSize(9);
            });
            if (vm.ResponseLikelihood.Count > 0)
                col.Item().PaddingTop(2).Text(txt =>
                {
                    txt.Span("Intervention-response fit — ").SemiBold().FontColor(Navy).FontSize(9);
                    txt.Span(string.Join(" · ", vm.ResponseLikelihood.Take(3).Select(r => $"{r.Category} {r.Likelihood:0}%"))).FontSize(9);
                });
            if (!string.IsNullOrWhiteSpace(vm.Score.CounterfactualNote))
                col.Item().PaddingTop(2).Text(vm.Score.CounterfactualNote).FontSize(9).Italic();
            if (vm.Zones.Count > 0)
            {
                var best = vm.Zones.Where(z => z.Overall.HasValue).OrderByDescending(z => z.Overall).FirstOrDefault();
                var worst = vm.Zones.Where(z => z.Overall.HasValue).OrderBy(z => z.Overall).FirstOrDefault();
                if (best is not null && worst is not null)
                    col.Item().PaddingTop(2).Text($"Regional heatmap — strongest zone: {best.Zone} ({best.Overall:0}); zone needing most attention: {worst.Zone} ({worst.Overall:0}).").FontSize(9);
            }
            if (vm.Instruments.Count > 0)
                col.Item().PaddingTop(2).Text($"Instrumental / multi-omics — {string.Join(" · ", vm.Instruments.Select(i => $"{i.Name} {i.Value:0.##}"))}").FontSize(9);

            // Digital-twin projection — real chart image (ScottPlot), with a table fallback.
            if (vm.Projections.Count > 0)
            {
                col.Item().PaddingTop(6).Text("Digital-Twin — 12-week What-if Projection").FontSize(12).Bold().FontColor(Navy);
                var png = ReportCharts.ProjectionPng(vm.Projections);
                if (png is not null)
                {
                    col.Item().PaddingTop(2).Height(180).Image(png).FitArea();
                    col.Item().Text(txt =>
                    {
                        txt.Span("Under: ").FontSize(8).FontColor(Muted);
                        txt.Span(string.Join(" · ", vm.Projections.Select(p => p.Category))).FontSize(8).FontColor(Ink);
                    });
                }
                else
                {
                    int[] wk = { 0, 2, 4, 6, 8, 10, 12 };
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(3); for (int k = 0; k < wk.Length; k++) cd.RelativeColumn(); });
                        HeadCell(t, "Intervention");
                        foreach (var wi in wk) HeadCell(t, "W" + wi);
                        foreach (var p in vm.Projections)
                        {
                            Cell(t, p.Category);
                            foreach (var wi in wk) Cell(t, wi < p.Weekly.Length ? p.Weekly[wi].ToString("0") : "—");
                        }
                    });
                }
                col.Item().Text("Projected integrated skin-quality score (0–100) over 12 weeks, scaled by biological response likelihood. Illustrative and non-diagnostic — not a guarantee of outcome.")
                    .FontSize(7.5f).Italic().FontColor(Muted);
            }

            Section(col, "Mathematical Model", s.ModelExplanation);
            Section(col, "Pathway-Level Interpretation", s.PathwayInterpretation);
            if (vm.Score.CohortPercentileScore.HasValue)
                col.Item().PaddingTop(4).Text($"Reference percentile (synthetic cohort, n={vm.Score.CohortSize}): {vm.Score.CohortPercentileScore.Value:0}th")
                    .FontSize(10).Bold().FontColor(Navy);
            Section(col, "Cohort Comparison", s.CohortNote);
            Section(col, "Reasoning Summary", s.ReasoningSummary);
            Section(col, "Limitations", s.Limitations);

            col.Item().PaddingTop(6).BorderTop(1).BorderColor(Line).PaddingTop(6)
                .Text(ReportViewModel.Disclaimer).FontSize(8).Italic().FontColor(Muted);
        });
    }

    private static void Section(ColumnDescriptor col, string? title, string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return;
        if (!string.IsNullOrWhiteSpace(title))
            col.Item().PaddingTop(4).Text(title).FontSize(12).Bold().FontColor(Navy);
        col.Item().Text(body).FontSize(10).LineHeight(1.35f);
    }

    private static void KeyVal(TableDescriptor t, string k, string v)
    {
        t.Cell().Element(e => e.Background("#F8FAFC").Padding(4)).Text(k).FontSize(9).FontColor(Muted);
        t.Cell().Element(e => e.Padding(4)).Text(v).FontSize(9).Bold();
    }

    private static void HeadCell(TableDescriptor t, string text) =>
        t.Cell().Background(Navy).Padding(4).Text(text).FontColor(Colors.White).FontSize(9).Bold();

    private static void Cell(TableDescriptor t, string text) =>
        t.Cell().BorderBottom(1).BorderColor(Line).Padding(4).Text(text).FontSize(9);

    private static string Fmt(double? v) => v?.ToString("0") ?? "—";

    private static void Footer(IContainer c, string brandName) =>
        c.BorderTop(1).BorderColor(Line).PaddingTop(6).Row(r =>
        {
            r.RelativeItem().Text($"{brandName} · Research use only — not a medical diagnosis").FontSize(8).FontColor(Muted);
            r.ConstantItem(80).AlignRight().Text(x =>
            {
                x.CurrentPageNumber().FontSize(8).FontColor(Muted);
                x.Span(" / ").FontSize(8).FontColor(Muted);
                x.TotalPages().FontSize(8).FontColor(Muted);
            });
        });

    private static ReportSections ParseSections(ReportViewModel vm)
    {
        if (vm.Document?.SectionsJson is { Length: > 2 } json)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ReportSections>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch { /* fall through */ }
        }
        return new ReportSections { ExecutiveSummary = vm.Document?.ExecutiveSummary };
    }
}
