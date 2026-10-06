namespace Arrapa.ClinicOS.Application.Abstractions.Appointments;

/// <summary>
/// Produces a unique appointment number for a new booking inside the given tenant.
/// </summary>
public interface IAppointmentNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
