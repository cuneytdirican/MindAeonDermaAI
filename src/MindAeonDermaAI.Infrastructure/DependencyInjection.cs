using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MindAeonDermaAI.Core.Ai;
using MindAeonDermaAI.Core.Lookups;
using MindAeonDermaAI.Infrastructure.Ai;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Infrastructure.Lookups;
using MindAeonDermaAI.Infrastructure.Seed;

namespace MindAeonDermaAI.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers EF Core, AI provider clients/orchestrator, lookup service and the seeder.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException(
                "ConnectionStrings:Default is not configured. Set it via user-secrets (dev) or the " +
                "ConnectionStrings__Default environment variable / appsettings.Production.json (IIS).");

        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<ISecretProtector, SecretProtector>();
        services.AddScoped<ILookupService, LookupService>();

        // AI clients (one per protocol family) + orchestrator.
        services.AddScoped<IAiClient, OpenAiCompatibleClient>();
        services.AddScoped<IAiClient, AnthropicClient>();
        services.AddScoped<IAiOrchestrator, AiOrchestrator>();

        services.AddScoped<DbSeeder>();

        services.AddHttpClient("ai", c => c.Timeout = TimeSpan.FromSeconds(120));

        return services;
    }
}
