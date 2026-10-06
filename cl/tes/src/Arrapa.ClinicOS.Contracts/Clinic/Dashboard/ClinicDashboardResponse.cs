namespace Arrapa.ClinicOS.Contracts.Clinic.Dashboard;

public sealed record ClinicDashboardResponse(
    ClinicStaffKpis Staff,
    ClinicDepartmentKpis Departments,
    ClinicAppointmentKpis Appointments,
    ClinicRevenueKpis Revenue,
    ClinicPatientKpis Patients,
    string Currency,
    decimal? DefaultConsultationFee,
    IReadOnlyList<DepartmentSummary> DepartmentBreakdown,
    IReadOnlyList<AppointmentTrendPoint> AppointmentTrend,
    IReadOnlyList<TopDoctorSummary> TopDoctors);

public sealed record ClinicStaffKpis(
    int TotalUsers,
    int ActiveUsers,
    int Doctors,
    int Receptionists,
    int ClinicAdmins);

public sealed record ClinicDepartmentKpis(int Total, int Active);

public sealed record ClinicAppointmentKpis(
    int Today,
    int Tomorrow,
    int ThisWeek,
    int ThisMonth,
    int PendingRequests,
    decimal NoShowRatePct);

public sealed record ClinicRevenueKpis(
    decimal IssuedThisMonth,
    decimal CollectedThisMonth,
    decimal OutstandingTotal,
    int OutstandingInvoiceCount);

public sealed record ClinicPatientKpis(
    int TotalActive,
    int NewThisMonth);

public sealed record DepartmentSummary(Guid Id, string Name, int DoctorCount, decimal? DefaultConsultationFee);

/// <summary>
/// A single day in a trend series. Counts are computed from local tenant time — the day boundary is midnight UTC.
/// </summary>
public sealed record AppointmentTrendPoint(DateOnly Date, int Total, int Completed);

public sealed record TopDoctorSummary(
    Guid DoctorId,
    string DoctorName,
    string? Specialty,
    int CompletedThisMonth,
    decimal RevenueThisMonth);
