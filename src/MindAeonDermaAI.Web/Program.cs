using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Analysis;
using MindAeonDermaAI.Infrastructure;
using MindAeonDermaAI.Infrastructure.Data;
using MindAeonDermaAI.Infrastructure.Identity;
using MindAeonDermaAI.Infrastructure.Seed;
using MindAeonDermaAI.Web.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Persist Data Protection keys so encrypted API keys survive app-pool recycles on IIS.
var keysDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keysDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("MindAeonDermaAI");

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAnalysis();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.LogoutPath = "/Account/Logout";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
    o.Cookie.HttpOnly = true;
    // Secure over HTTPS (production/IIS) while still working over plain HTTP in local dev.
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddLocalization();
string[] supportedCultures = { "en", "tr", "de", "fr", "es", "it", "pt", "ru", "ar", "zh", "ja", "ko", "hi", "nl", "pl", "sv", "uk", "fa", "id", "az" };
builder.Services.Configure<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>(o =>
{
    o.SetDefaultCulture("en");
    o.AddSupportedCultures(supportedCultures);
    o.AddSupportedUICultures(supportedCultures);
});

builder.Services.AddControllersWithViews(options =>
{
    // Require an authenticated user everywhere unless [AllowAnonymous] is applied.
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
}).AddViewLocalization();
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

// Application services (report generation, assessment orchestration).
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ExampleReportBuilder>();
builder.Services.AddScoped<DemoDataSeeder>();
builder.Services.AddScoped<IReferenceCalibrationService, ReferenceCalibrationService>();
builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IBrandingService, BrandingService>();
builder.Services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
builder.Services.AddHttpClient("webhook", c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// Public REST API docs (Swagger / OpenAPI). Authenticate calls with the X-Api-Key header (config Api:Key).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
{
    Title = "MindAeon Dermalytics API",
    Version = "v1",
    Description = "Public REST API — send your key in the X-Api-Key header."
}));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "MindAeon Dermalytics API v1"); c.DocumentTitle = "MindAeon Dermalytics API"; });

app.MapControllers(); // attribute-routed API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

// Apply migrations and seed reference + identity data at startup (DB-verified pipeline).
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await sp.GetRequiredService<DbSeeder>().SeedAsync();
    await IdentitySeeder.SeedAsync(sp);
    await sp.GetRequiredService<DemoDataSeeder>().SeedAsync();
}

// `dotnet run -- --seed` migrates + seeds and exits (useful for deployment / verification).
if (args.Contains("--seed"))
{
    Console.WriteLine("Seed completed.");
    return;
}

app.Run();
