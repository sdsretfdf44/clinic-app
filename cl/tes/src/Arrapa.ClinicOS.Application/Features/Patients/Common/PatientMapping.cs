using Arrapa.ClinicOS.Contracts.Patients;
using Arrapa.ClinicOS.Domain.Clinical;

namespace Arrapa.ClinicOS.Application.Features.Patients.Common;

/// <summary>
/// Central mapping helpers that keep patient DTO shape in sync between handlers.
/// </summary>
internal static class PatientMapping
{
    public static int ComputeAge(DateOnly dateOfBirth, DateOnly today)
    {
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }
        return age < 0 ? 0 : age;
    }

    public static PatientSummaryResponse ToSummary(Patient p, DateOnly today) => new(
        p.Id, p.MRN, p.FirstName, p.LastName, p.FullName,
        p.DateOfBirth, ComputeAge(p.DateOfBirth, today),
        p.Gender.ToString(), p.BloodGroup.ToString(),
        p.Phone, p.Email, p.PhotoUrl, p.IsActive, p.LastVisitAt, p.CreatedOn);

    public static PatientDetailResponse ToDetail(
        Patient p,
        DateOnly today,
        int allergyCount,
        int historyCount,
        int activeMedicationCount) => new(
        p.Id, p.MRN,
        p.FirstName, p.MiddleName, p.LastName, p.FullName,
        p.DateOfBirth, ComputeAge(p.DateOfBirth, today),
        p.Gender.ToString(), p.BloodGroup.ToString(), p.MaritalStatus.ToString(),
        p.Phone, p.Email, p.PhotoUrl,
        p.AddressLine1, p.AddressLine2, p.City, p.State, p.Country, p.PostalCode,
        p.Occupation,
        p.EmergencyContactName, p.EmergencyContactPhone, p.EmergencyContactRelationship,
        p.GuardianName, p.GuardianPhone, p.GuardianRelationship,
        p.InsuranceProvider, p.InsurancePolicyNumber, p.InsuranceGroupNumber, p.InsuranceValidUntil,
        p.Notes,
        p.IsActive, p.LastVisitAt,
        p.CreatedOn, p.UpdatedOn,
        allergyCount, historyCount, activeMedicationCount);
}
