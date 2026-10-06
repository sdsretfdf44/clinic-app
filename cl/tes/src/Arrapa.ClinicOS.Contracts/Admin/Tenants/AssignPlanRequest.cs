namespace Arrapa.ClinicOS.Contracts.Admin.Tenants;

public sealed record AssignPlanRequest(Guid SubscriptionPlanId, DateTimeOffset? ExpiresAt);
