namespace Arrapa.ClinicOS.Contracts.Appointments;

public sealed record AppointmentSummaryResponse(
    Guid Id,
    string AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid DoctorId,
    string DoctorName,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    string? Reason,
    decimal? ConsultationFee,
    string? Currency);

public sealed record AppointmentDetailResponse(
    Guid Id,
    string AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    string? PatientPhone,
    Guid DoctorId,
    string DoctorName,
    string? DoctorSpecialty,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    string? Reason,
    string? Notes,
    decimal? ConsultationFee,
    string? Currency,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CheckedInAt,
    DateTimeOffset? ConsultationStartedAt,
    DateTimeOffset? ConsultationEndedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    DateTimeOffset CreatedOn,
    DateTimeOffset? UpdatedOn);

public sealed record BookAppointmentRequest(
    Guid PatientId,
    Guid DoctorId,
    Guid? DepartmentId,
    DateTimeOffset ScheduledStart,
    int DurationMinutes,
    string? Reason,
    string? Notes);

public sealed record RescheduleAppointmentRequest(
    Guid DoctorId,
    Guid? DepartmentId,
    DateTimeOffset ScheduledStart,
    int DurationMinutes,
    string? Reason,
    string? Notes);

public sealed record CancelAppointmentRequest(string Reason);

public sealed record AppointmentSlot(DateTimeOffset Start, DateTimeOffset End);

public sealed record AvailableSlotsResponse(
    Guid DoctorId,
    DateOnly Date,
    int SlotMinutes,
    IReadOnlyList<AppointmentSlot> Slots);
