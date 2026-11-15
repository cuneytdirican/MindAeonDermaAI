using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MindAeonDermaAI.Core.Entities;
using MindAeonDermaAI.Infrastructure.Identity;

namespace MindAeonDermaAI.Infrastructure.Data;

/// <summary>
/// EF Core context for MindAeon DermaAI. Extends the Identity context so users/roles and domain data
/// share one MS SQL database.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Lookups
    public DbSet<Sex> Sexes => Set<Sex>();
    public DbSet<SkinType> SkinTypes => Set<SkinType>();
    public DbSet<FitzpatrickType> FitzpatrickTypes => Set<FitzpatrickType>();
    public DbSet<SmokingStatus> SmokingStatuses => Set<SmokingStatus>();
    public DbSet<AlcoholUse> AlcoholUses => Set<AlcoholUse>();
    public DbSet<SleepQuality> SleepQualities => Set<SleepQuality>();
    public DbSet<StressLevel> StressLevels => Set<StressLevel>();
    public DbSet<UVExposureLevel> UVExposureLevels => Set<UVExposureLevel>();
    public DbSet<MenopauseStatus> MenopauseStatuses => Set<MenopauseStatus>();
    public DbSet<AestheticProcedure> AestheticProcedures => Set<AestheticProcedure>();
    public DbSet<InterventionCategory> InterventionCategories => Set<InterventionCategory>();
    public DbSet<MeasurementUnit> MeasurementUnits => Set<MeasurementUnit>();
    public DbSet<PhenotypeType> PhenotypeTypes => Set<PhenotypeType>();
    public DbSet<FacialZone> FacialZones => Set<FacialZone>();
    public DbSet<InstrumentType> InstrumentTypes => Set<InstrumentType>();
    public DbSet<WearableSource> WearableSources => Set<WearableSource>();
    public DbSet<WearableMetricType> WearableMetricTypes => Set<WearableMetricType>();
    public DbSet<OmicsType> OmicsTypes => Set<OmicsType>();

    // Biological reference
    public DbSet<BiologicalDomain> BiologicalDomains => Set<BiologicalDomain>();
    public DbSet<Panel> Panels => Set<Panel>();
    public DbSet<PanelBiomarker> PanelBiomarkers => Set<PanelBiomarker>();
    public DbSet<ProteinBiomarker> ProteinBiomarkers => Set<ProteinBiomarker>();

    // Subject & assessment
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SubjectAestheticProcedure> SubjectAestheticProcedures => Set<SubjectAestheticProcedure>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<SkinImage> SkinImages => Set<SkinImage>();
    public DbSet<ProteinMeasurement> ProteinMeasurements => Set<ProteinMeasurement>();
    public DbSet<ScoreResult> ScoreResults => Set<ScoreResult>();
    public DbSet<ReportDocument> ReportDocuments => Set<ReportDocument>();
    public DbSet<ZoneScore> ZoneScores => Set<ZoneScore>();
    public DbSet<InstrumentalReading> InstrumentalReadings => Set<InstrumentalReading>();
    public DbSet<FacialGeometry> FacialGeometries => Set<FacialGeometry>();
    public DbSet<BrandingSettings> BrandingSettings => Set<BrandingSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WearableReading> WearableReadings => Set<WearableReading>();
    public DbSet<OmicsMarker> OmicsMarkers => Set<OmicsMarker>();

    // Cohort
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<CohortSubject> CohortSubjects => Set<CohortSubject>();
    public DbSet<CohortMeasurement> CohortMeasurements => Set<CohortMeasurement>();

    // AI configuration
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AiRequestLog> AiRequestLogs => Set<AiRequestLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Store floating-point measures with a fixed precision for MSSQL determinism.
        foreach (var prop in b.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(double) || p.ClrType == typeof(double?)))
        {
            prop.SetColumnType("decimal(18,4)");
        }

        b.Entity<PanelBiomarker>()
            .HasIndex(pb => new { pb.PanelId, pb.BiomarkerId }).IsUnique();

        b.Entity<ProteinBiomarker>()
            .HasOne(p => p.Domain).WithMany(d => d.Biomarkers)
            .HasForeignKey(p => p.DomainId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Panel>()
            .HasOne(p => p.Domain).WithOne(d => d.Panel)
            .HasForeignKey<Panel>(p => p.DomainId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Assessment>()
            .HasOne(a => a.ScoreResult).WithOne(s => s.Assessment)
            .HasForeignKey<ScoreResult>(s => s.AssessmentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Assessment>()
            .HasOne(a => a.Report).WithOne(r => r.Assessment)
            .HasForeignKey<ReportDocument>(r => r.AssessmentId).OnDelete(DeleteBehavior.Cascade);

        // Zone / instrumental readings: cascade from the assessment, but restrict from the lookup side
        // to avoid multiple cascade paths on SQL Server.
        b.Entity<ZoneScore>().HasOne(z => z.Assessment).WithMany(a => a.ZoneScores)
            .HasForeignKey(z => z.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ZoneScore>().HasOne(z => z.FacialZone).WithMany()
            .HasForeignKey(z => z.FacialZoneId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<InstrumentalReading>().HasOne(r => r.Assessment).WithMany(a => a.InstrumentalReadings)
            .HasForeignKey(r => r.AssessmentId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<InstrumentalReading>().HasOne(r => r.InstrumentType).WithMany()
            .HasForeignKey(r => r.InstrumentTypeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Assessment>().HasOne(a => a.FacialGeometry).WithOne(g => g.Assessment)
            .HasForeignKey<FacialGeometry>(g => g.AssessmentId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<WearableReading>().HasOne(w => w.Subject).WithMany(s => s.WearableReadings)
            .HasForeignKey(w => w.SubjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<WearableReading>().HasOne(w => w.WearableSource).WithMany().HasForeignKey(w => w.WearableSourceId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<WearableReading>().HasOne(w => w.WearableMetricType).WithMany().HasForeignKey(w => w.WearableMetricTypeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<OmicsMarker>().HasOne(o => o.Subject).WithMany(s => s.OmicsMarkers)
            .HasForeignKey(o => o.SubjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<OmicsMarker>().HasOne(o => o.OmicsType).WithMany().HasForeignKey(o => o.OmicsTypeId).OnDelete(DeleteBehavior.Restrict);

        // Prevent multiple cascade paths on MSSQL for optional lookups on Subject/Assessment.
        foreach (var fk in b.Model.GetEntityTypes()
                     .SelectMany(t => t.GetForeignKeys())
                     .Where(fk => !fk.IsRequired))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        b.Entity<Subject>().HasIndex(s => s.Code).IsUnique();
        b.Entity<AiProvider>().HasIndex(p => p.Name).IsUnique();
    }
}
