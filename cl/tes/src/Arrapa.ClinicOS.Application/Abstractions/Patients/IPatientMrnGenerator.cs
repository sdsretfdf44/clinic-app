namespace Arrapa.ClinicOS.Application.Abstractions.Patients;

/// <summary>
/// Produces a unique Medical Record Number (MRN) for a new patient inside the given tenant. Handlers must
/// persist the returned value; the generator itself performs no writes.
/// </summary>
public interface IPatientMrnGenerator
{
    Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
