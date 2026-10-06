using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

public static class VisitErrors
{
    public static readonly Error NotFound = Error.NotFound("visit.not_found", "Visit not found.");
    public static readonly Error Locked = Error.Conflict("visit.locked", "This visit is locked and cannot be edited.");
    public static readonly Error PrescriptionNotFound = Error.NotFound("visit.prescription.not_found", "Prescription not found.");
    public static readonly Error PrescriptionEmpty = Error.Validation("visit.prescription.empty", "A prescription must have at least one line item.");
}
