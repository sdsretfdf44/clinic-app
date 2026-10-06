namespace Arrapa.ClinicOS.Contracts.Reception;

/// <summary>
/// Reception's today-at-a-glance queue. Every non-cancelled appointment scheduled for today grouped by the
/// bucket the front desk cares about.
/// </summary>
public sealed record ReceptionQueueResponse(
    DateTimeOffset AsOf,
    ReceptionQueueBucket ExpectedToArrive,
    ReceptionQueueBucket WaitingForConsultation,
    ReceptionQueueBucket InConsultation,
    ReceptionQueueBucket DoneToday,
    ReceptionQueueBucket NoShow);

public sealed record ReceptionQueueBucket(int Count, IReadOnlyList<ReceptionQueueEntry> Items);

public sealed record ReceptionQueueEntry(
    Guid AppointmentId,
    string AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    string? PatientPhone,
    Guid DoctorId,
    string DoctorName,
    string? DepartmentName,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    string? Reason,
    DateTimeOffset? CheckedInAt);
