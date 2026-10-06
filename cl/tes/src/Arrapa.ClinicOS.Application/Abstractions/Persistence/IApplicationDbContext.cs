using Arrapa.ClinicOS.Domain.Audit;
using Arrapa.ClinicOS.Domain.Billing;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.FeatureFlags;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Domain.Notifications;
using Arrapa.ClinicOS.Domain.Settings;
using Arrapa.ClinicOS.Domain.Subscriptions;
using Arrapa.ClinicOS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Abstractions.Persistence;

/// <summary>
/// Application-level facade over the EF Core <c>DbContext</c>. Handlers depend on this contract, never on the concrete context.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<FeatureFlag> FeatureFlags { get; }
    DbSet<TenantFeatureFlag> TenantFeatureFlags { get; }
    DbSet<GlobalSetting> GlobalSettings { get; }
    DbSet<TenantSetting> TenantSettings { get; }
    DbSet<AuditLogEntry> AuditLogs { get; }
    DbSet<Department> Departments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Receptionist> Receptionists { get; }
    DbSet<WorkingHours> WorkingHours { get; }
    DbSet<Patient> Patients { get; }
    DbSet<PatientAllergy> PatientAllergies { get; }
    DbSet<MedicalHistoryEntry> MedicalHistoryEntries { get; }
    DbSet<PatientMedication> PatientMedications { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Visit> Visits { get; }
    DbSet<VisitVitals> VisitVitals { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLineItem> InvoiceLineItems { get; }
    DbSet<InvoicePayment> InvoicePayments { get; }
    DbSet<NotificationLog> NotificationLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
