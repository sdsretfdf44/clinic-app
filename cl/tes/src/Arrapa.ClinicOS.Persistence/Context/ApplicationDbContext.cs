using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Domain.Audit;
using Arrapa.ClinicOS.Domain.Billing;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.FeatureFlags;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Domain.Notifications;
using Arrapa.ClinicOS.Domain.Settings;
using Arrapa.ClinicOS.Domain.Subscriptions;
using Arrapa.ClinicOS.Domain.Tenants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Persistence.Context;

/// <summary>
/// Root EF Core context. Combines ASP.NET Identity, the application domain, and all cross-cutting interceptors
/// (audit, soft-delete, tenant, concurrency). Clinic-scoped entities receive an automatic tenant query filter via
/// <see cref="OnModelCreating"/>; cross-tenant handlers opt out with <c>IgnoreQueryFilters</c>.
/// </summary>
public sealed class ApplicationDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>,
        Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>,
        Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>,
        Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>,
        Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>,
      IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<TenantFeatureFlag> TenantFeatureFlags => Set<TenantFeatureFlag>();
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<MedicalHistoryEntry> MedicalHistoryEntries => Set<MedicalHistoryEntry>();
    public DbSet<PatientMedication> PatientMedications => Set<PatientMedication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<VisitVitals> VisitVitals => Set<VisitVitals>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Doctor>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Receptionist>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<WorkingHours>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<TenantSetting>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Patient>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<PatientAllergy>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<MedicalHistoryEntry>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<PatientMedication>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Visit>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<VisitVitals>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Prescription>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<PrescriptionItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<InvoiceLineItem>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<InvoicePayment>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
        builder.Entity<NotificationLog>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext.TenantId);
    }
}
