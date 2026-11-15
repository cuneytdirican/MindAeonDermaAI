using MindAeonDermaAI.Web.Models;

namespace MindAeonDermaAI.Web.Services;

/// <summary>Renders report charts to PNG (via ScottPlot) for embedding in the PDF — no headless browser.</summary>
public static class ReportCharts
{
    private static readonly string[] Palette = { "#0b7285", "#12a594", "#f59e0b", "#6366f1" };

    public static byte[]? ProjectionPng(List<ReportViewModel.ProjectionSeries> series, int width = 680, int height = 300)
    {
        if (series.Count == 0) return null;
        try
        {
            var plot = new ScottPlot.Plot();
            double[] xs = Enumerable.Range(0, series[0].Weekly.Length).Select(i => (double)i).ToArray();
            double yMin = 100;
            for (int i = 0; i < series.Count; i++)
            {
                var line = plot.Add.Scatter(xs, series[i].Weekly);
                line.LegendText = series[i].Category;
                line.Color = ScottPlot.Color.FromHex(Palette[i % Palette.Length]);
                line.LineWidth = 2;
                line.MarkerSize = 4;
                yMin = Math.Min(yMin, series[i].Weekly.Min());
            }
            plot.Axes.SetLimits(0, series[0].Weekly.Length - 1, Math.Max(0, yMin - 6), 100);
            plot.XLabel("Weeks");
            plot.YLabel("Integrated score");
            plot.ShowLegend(ScottPlot.Alignment.LowerRight);
            return plot.GetImageBytes(width, height);
        }
        catch { return null; } // never break the PDF over a chart
    }
}
