using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MindAeonDermaAI.Infrastructure.Data;

/// <summary>
/// Design-time factory so `dotnet ef` can build the context without spinning up the web host.
/// Resolves the connection string from (in order): env var MINDAEON_CONNECTION, user-secrets/appsettings
/// of the Web project, or a local fallback. Keeps the SQL password out of source where possible.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "MindAeonDermaAI.Web"));
        if (!Directory.Exists(basePath)) basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("05cbb96d-72c2-40fc-a39a-a7538edcc10a")
            .AddEnvironmentVariables()
            .Build();

        var cs = Environment.GetEnvironmentVariable("MINDAEON_CONNECTION")
                 ?? config.GetConnectionString("Default")
                 ?? throw new InvalidOperationException("No connection string configured for design-time.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new AppDbContext(options);
    }
}
