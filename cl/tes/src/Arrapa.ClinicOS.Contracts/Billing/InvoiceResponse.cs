namespace Arrapa.ClinicOS.Contracts.Billing;

public sealed record InvoiceSummaryResponse(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    Guid? AppointmentId,
    string? AppointmentNumber,
    string Status,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? DueAt,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    decimal AmountPaid,
    decimal AmountDue,
    string Currency);

public sealed record InvoiceLineItemResponse(
    Guid Id,
    string Description,
    string? ServiceCode,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record InvoicePaymentResponse(
    Guid Id,
    decimal Amount,
    string Method,
    string? Reference,
    DateTimeOffset PaidAt,
    Guid RecordedByUserId,
    string? Notes);

public sealed record InvoiceDetailResponse(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    string? PatientPhone,
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
    IReadOnlyList<InvoiceLineItemResponse> LineItems,
    IReadOnlyList<InvoicePaymentResponse> Payments,
    DateTimeOffset CreatedOn,
    DateTimeOffset? UpdatedOn,
    bool CanEdit,
    bool CanIssue,
    bool CanPay,
    bool CanCancel);

public sealed record CreateInvoiceLineItemRequest(
    string Description,
    string? ServiceCode,
    decimal Quantity,
    decimal UnitPrice);

public sealed record CreateInvoiceRequest(
    Guid PatientId,
    Guid? AppointmentId,
    DateTimeOffset? DueAt,
    decimal DiscountAmount,
    decimal TaxAmount,
    string? Currency,
    string? Notes,
    IReadOnlyList<CreateInvoiceLineItemRequest> LineItems);

public sealed record UpdateInvoiceRequest(
    DateTimeOffset? DueAt,
    decimal DiscountAmount,
    decimal TaxAmount,
    string? Notes,
    IReadOnlyList<CreateInvoiceLineItemRequest> LineItems);

public sealed record CancelInvoiceRequest(string Reason);

public sealed record RecordPaymentRequest(
    decimal Amount,
    string Method,
    string? Reference,
    DateTimeOffset? PaidAt,
    string? Notes);
