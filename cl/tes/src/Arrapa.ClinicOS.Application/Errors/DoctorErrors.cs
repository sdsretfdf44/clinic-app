using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

public static class DoctorErrors
{
    public static readonly Error NoDoctorRecord = Error.NotFound(
        "doctor.no_record",
        "No doctor record is linked to this account. Please contact your clinic administrator.");
}
