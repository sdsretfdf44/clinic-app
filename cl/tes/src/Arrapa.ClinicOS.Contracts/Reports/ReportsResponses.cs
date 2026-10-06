namespace Arrapa.ClinicOS.Contracts.Reports;

// -------------------- Revenue --------------------
public sealed record RevenueSummaryResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    string Currency,
    decimal IssuedTotal,
    decimal CollectedTotal,
    decimal OutstandingTotal,
    int InvoiceCount,
    int PaidInvoiceCount,
    IReadOnlyList<RevenueSeriesPoint> Series);

public sealed record RevenueSeriesPoint(DateOnly Date, decimal Issued, decimal Collected);

// -------------------- Appointments funnel --------------------
public sealed record AppointmentsFunnelResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    int Booked,
    int Confirmed,
    int CheckedIn,
    int Completed,
    int Cancelled,
    int NoShow,
    decimal CompletionRatePct,
    decimal NoShowRatePct,
    decimal CancellationRatePct);

// -------------------- Top doctors --------------------
public sealed record TopDoctorsReportResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    string Currency,
    IReadOnlyList<TopDoctorReportRow> Rows);

public sealed record TopDoctorReportRow(
    Guid DoctorId,
    string DoctorName,
    string? Specialty,
    int Appointments,
    int Completed,
    decimal Revenue);

// -------------------- Top services --------------------
public sealed record TopServicesReportResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    string Currency,
    IReadOnlyList<TopServiceReportRow> Rows);

public sealed record TopServiceReportRow(
    string Description,
    string? ServiceCode,
    int LineCount,
    decimal Quantity,
    decimal Revenue);

// -------------------- Patient demographics --------------------
public sealed record PatientDemographicsResponse(
    int TotalPatients,
    IReadOnlyList<GenderBreakdownRow> Gender,
    IReadOnlyList<AgeBandRow> AgeBands,
    IReadOnlyList<BloodGroupRow> BloodGroups);

public sealed record GenderBreakdownRow(string Gender, int Count);

public sealed record AgeBandRow(string Band, int Count);

public sealed record BloodGroupRow(string BloodGroup, int Count);
