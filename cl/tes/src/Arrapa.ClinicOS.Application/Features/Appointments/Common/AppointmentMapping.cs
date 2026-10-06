using Arrapa.ClinicOS.Contracts.Appointments;
using Arrapa.ClinicOS.Domain.Clinical;

namespace Arrapa.ClinicOS.Application.Features.Appointments.Common;

internal static class AppointmentMapping
{
    public static AppointmentDetailResponse ToDetail(Appointment a) => new(
        a.Id, a.AppointmentNumber,
        a.PatientId, a.Patient?.FullName ?? string.Empty, a.Patient?.MRN ?? string.Empty, a.Patient?.Phone,
        a.DoctorId, a.Doctor?.User?.FullName ?? string.Empty, a.Doctor?.Specialty,
        a.DepartmentId, a.Department?.Name,
        a.ScheduledStart, a.ScheduledEnd,
        a.Status.ToString(), a.Reason, a.Notes,
        a.ConsultationFee, a.Currency,
        a.ConfirmedAt, a.CheckedInAt, a.ConsultationStartedAt, a.ConsultationEndedAt,
        a.CancelledAt, a.CancellationReason,
        a.CreatedOn, a.UpdatedOn);

    public static AppointmentSummaryResponse ToSummary(Appointment a) => new(
        a.Id, a.AppointmentNumber,
        a.PatientId, a.Patient?.FullName ?? string.Empty, a.Patient?.MRN ?? string.Empty,
        a.DoctorId, a.Doctor?.User?.FullName ?? string.Empty,
        a.DepartmentId, a.Department?.Name,
        a.ScheduledStart, a.ScheduledEnd, a.Status.ToString(),
        a.Reason, a.ConsultationFee, a.Currency);
}
