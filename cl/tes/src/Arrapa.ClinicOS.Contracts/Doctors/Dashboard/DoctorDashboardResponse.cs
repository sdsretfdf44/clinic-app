namespace Arrapa.ClinicOS.Contracts.Doctors.Dashboard;

public sealed record DoctorDashboardResponse(
    DoctorProfileSummary Doctor,
    DoctorTodayKpis Today,
    DoctorRangeKpis ThisWeek,
    DoctorRangeKpis ThisMonth,
    DoctorLifetimeKpis Lifetime,
    IReadOnlyList<DoctorAppointmentPreview> NextAppointments,
    IReadOnlyList<DoctorTrendPoint> AppointmentTrend);

public sealed record DoctorProfileSummary(
    Guid Id,
    string FullName,
    string? Specialty,
    Guid? DepartmentId,
    string? DepartmentName,
    string Currency);

public sealed record DoctorTodayKpis(
    int Total,
    int Remaining,
    int InConsultation,
    int Completed,
    int NoShow);

public sealed record DoctorRangeKpis(
    int Booked,
    int Completed,
    int Cancelled,
    int NoShow,
    decimal ConsultationRevenue);

public sealed record DoctorLifetimeKpis(
    int UniquePatients,
    int TotalCompleted);

public sealed record DoctorAppointmentPreview(
    Guid Id,
    string AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    string? Reason);

public sealed record DoctorTrendPoint(DateOnly Date, int Total, int Completed);
