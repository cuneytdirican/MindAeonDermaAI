using Microsoft.Extensions.DependencyInjection;
using MindAeonDermaAI.Analysis.Engine;
using MindAeonDermaAI.Analysis.Imaging;

namespace MindAeonDermaAI.Analysis;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalysis(this IServiceCollection services)
    {
        services.AddScoped<IAnalysisEngine, AnalysisEngine>();
        services.AddScoped<IImageAnalyzer, ImageAnalyzer>();
        return services;
    }
}
