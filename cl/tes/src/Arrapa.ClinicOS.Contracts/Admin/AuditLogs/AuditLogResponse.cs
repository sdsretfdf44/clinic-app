namespace Arrapa.ClinicOS.Contracts.Admin.AuditLogs;

public sealed record AuditLogResponse(
    Guid Id,
    Guid TenantId,
    string? TenantName,
    string EntityType,
    string EntityId,
    string Operation,
    Guid? PerformedByUserId,
    string? PerformedByEmail,
    string? IpAddress,
    string? CorrelationId,
    DateTimeOffset OccurredOn);

public sealed record AuditLogDetailResponse(
    Guid Id,
    Guid TenantId,
    string? TenantName,
    string EntityType,
    string EntityId,
    string Operation,
    Guid? PerformedByUserId,
    string? PerformedByEmail,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? ChangesJson,
    DateTimeOffset OccurredOn);
