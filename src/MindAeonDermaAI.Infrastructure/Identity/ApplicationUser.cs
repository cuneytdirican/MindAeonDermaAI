using Microsoft.AspNetCore.Identity;

namespace MindAeonDermaAI.Infrastructure.Identity;

/// <summary>Application user with a display name for the modern UI.</summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Central definition of application roles.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Clinician = "Clinician";
    public const string Reviewer = "Reviewer";

    public static readonly string[] All = { Admin, Clinician, Reviewer };
}
