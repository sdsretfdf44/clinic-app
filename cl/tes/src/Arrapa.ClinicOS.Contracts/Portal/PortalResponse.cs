namespace Arrapa.ClinicOS.Contracts.Portal;

// ==================== Profile ====================
public sealed record PortalProfileResponse(
    Guid Id,
    string MRN,
    string FirstName,
    string? MiddleName,
    string LastName,
    string FullName,
    DateOnly DateOfBirth,
    int Age,
    string Gender,
    string BloodGroup,
    string MaritalStatus,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Occupation,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship,
    string? InsuranceProvider,
    string? InsurancePolicyNumber,
    string? InsuranceGroupNumber,
    DateOnly? InsuranceValidUntil,
    DateTimeOffset? LastVisitAt);

public sealed record UpdatePortalProfileRequest(
    string? Phone,
    string? Email,
    string? PhotoUrl,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Occupation,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship);

// ==================== Doctor directory ====================
public sealed record PortalDoctorResponse(
    Guid Id,
    string FullName,
    string? AvatarUrl,
    string? Specialty,
    Guid? DepartmentId,
    string? DepartmentName,
    decimal? ConsultationFee,
    string Currency,
    string? Bio);

// ==================== Appointments ====================
public sealed record PortalAppointmentSummaryResponse(
    Guid Id,
    string AppointmentNumber,
    Guid DoctorId,
    string DoctorName,
    string? DoctorSpecialty,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    string Status,
    string? Reason,
    decimal? ConsultationFee,
    string? Currency);

public sealed record PortalAppointmentDetailResponse(
    Guid Id,
    string AppointmentNumber,
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
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    DateTimeOffset CreatedOn,
    bool CanCancel);

public sealed record RequestAppointmentRequest(
    Guid DoctorId,
    Guid? DepartmentId,
    DateTimeOffset ScheduledStart,
    int DurationMinutes,
    string? Reason);

public sealed record CancelPortalAppointmentRequest(string Reason);

// ==================== Prescriptions ====================
public sealed record PortalPrescriptionItemResponse(
    Guid Id,
    string Medicine,
    string? Strength,
    string? Dosage,
    string? Frequency,
    string? Duration,
    string? Instructions);

public sealed record PortalPrescriptionResponse(
    Guid Id,
    Guid VisitId,
    DateTimeOffset IssuedOn,
    string DoctorName,
    string? DoctorSpecialty,
    string? GeneralInstructions,
    IReadOnlyList<PortalPrescriptionItemResponse> Items);

// ==================== Invoices ====================
public sealed record PortalInvoiceLineItemResponse(
    Guid Id,
    string Description,
    string? ServiceCode,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record PortalInvoicePaymentResponse(
    Guid Id,
    decimal Amount,
    string Method,
    string? Reference,
    DateTimeOffset PaidAt);

public sealed record PortalInvoiceSummaryResponse(
    Guid Id,
    string InvoiceNumber,
    Guid? AppointmentId,
    string? AppointmentNumber,
    string Status,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? DueAt,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    string Currency);

public sealed record PortalInvoiceDetailResponse(
    Guid Id,
    string InvoiceNumber,
    Guid? AppointmentId,
    string? AppointmentNumber,
    string Status,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    string Currency,
    string? Notes,
    IReadOnlyList<PortalInvoiceLineItemResponse> LineItems,
    IReadOnlyList<PortalInvoicePaymentResponse> Payments);

// ==================== Dashboard ====================
public sealed record PortalDashboardResponse(
    PortalAppointmentSummaryResponse? NextAppointment,
    int UpcomingAppointmentCount,
    int PendingRequestCount,
    int PrescriptionCount,
    int OutstandingInvoiceCount,
    decimal OutstandingAmount,
    string Currency);
