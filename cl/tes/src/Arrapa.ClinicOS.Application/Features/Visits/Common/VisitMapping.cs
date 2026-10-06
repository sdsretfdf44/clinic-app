using Arrapa.ClinicOS.Contracts.Visits;
using Arrapa.ClinicOS.Domain.Clinical;

namespace Arrapa.ClinicOS.Application.Features.Visits.Common;

internal static class VisitMapping
{
    public static VisitDetailResponse ToDetail(Visit v, int patientAge) => new(
        v.Id,
        v.AppointmentId,
        v.Appointment?.AppointmentNumber,
        v.PatientId,
        v.Patient?.FullName ?? string.Empty,
        v.Patient?.MRN ?? string.Empty,
        patientAge,
        v.DoctorId,
        v.Doctor?.User?.FullName ?? string.Empty,
        v.Doctor?.Specialty,
        v.VisitDate,
        v.Symptoms,
        v.Diagnosis,
        v.DoctorNotes,
        v.IsLocked,
        v.Vitals is null ? null : ToVitals(v.Vitals),
        v.Prescriptions.Select(ToPrescription).ToList(),
        v.CreatedOn,
        v.UpdatedOn);

    public static VitalsResponse ToVitals(VisitVitals x) => new(
        x.Id,
        x.BloodPressureSystolic, x.BloodPressureDiastolic,
        x.PulseBpm, x.TemperatureCelsius,
        x.HeightCm, x.WeightKg, x.Bmi,
        x.SpO2Percent, x.RespirationRate, x.Notes);

    public static PrescriptionResponse ToPrescription(Prescription p) => new(
        p.Id, p.IssuedOn, p.GeneralInstructions,
        p.Items.OrderBy(i => i.SortOrder)
            .Select(i => new PrescriptionItemResponse(
                i.Id, i.Medicine, i.Strength, i.Dosage, i.Frequency, i.Duration, i.Instructions, i.SortOrder))
            .ToList());

    /// <summary>Recomputes BMI when both height and weight are present.</summary>
    public static decimal? ComputeBmi(decimal? heightCm, decimal? weightKg)
    {
        if (heightCm is null or <= 0 || weightKg is null or <= 0)
        {
            return null;
        }
        var meters = heightCm.Value / 100m;
        return Math.Round(weightKg.Value / (meters * meters), 2);
    }
}
