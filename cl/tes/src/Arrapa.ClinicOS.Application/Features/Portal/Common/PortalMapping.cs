using Arrapa.ClinicOS.Contracts.Portal;
using Arrapa.ClinicOS.Domain.Billing;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Application.Features.Portal.Common;

internal static class PortalMapping
{
    public static int Age(DateOnly dob, DateOnly today)
    {
        var years = today.Year - dob.Year;
        if (dob > today.AddYears(-years)) years--;
        return Math.Max(0, years);
    }

    public static PortalProfileResponse ToProfile(Patient p, DateOnly today) => new(
        p.Id, p.MRN, p.FirstName, p.MiddleName, p.LastName, p.FullName,
        p.DateOfBirth, Age(p.DateOfBirth, today),
        p.Gender.ToString(), p.BloodGroup.ToString(), p.MaritalStatus.ToString(),
        p.Phone, p.Email, p.PhotoUrl,
        p.AddressLine1, p.AddressLine2, p.City, p.State, p.Country, p.PostalCode,
        p.Occupation,
        p.EmergencyContactName, p.EmergencyContactPhone, p.EmergencyContactRelationship,
        p.InsuranceProvider, p.InsurancePolicyNumber, p.InsuranceGroupNumber, p.InsuranceValidUntil,
        p.LastVisitAt);

    public static PortalDoctorResponse ToDoctor(Domain.Clinical.Doctor d, string tenantCurrency) => new(
        d.Id,
        d.User?.FullName ?? string.Empty,
        d.User?.AvatarUrl,
        d.Specialty,
        d.DepartmentId,
        d.Department?.Name,
        d.ConsultationFee ?? d.Department?.DefaultConsultationFee,
        tenantCurrency,
        d.Bio);

    public static PortalAppointmentSummaryResponse ToAppointmentSummary(Appointment a) => new(
        a.Id, a.AppointmentNumber,
        a.DoctorId,
        a.Doctor?.User?.FullName ?? string.Empty,
        a.Doctor?.Specialty,
        a.DepartmentId,
        a.Department?.Name,
        a.ScheduledStart, a.ScheduledEnd,
        a.Status.ToString(),
        a.Reason,
        a.ConsultationFee, a.Currency);

    public static PortalAppointmentDetailResponse ToAppointmentDetail(Appointment a) => new(
        a.Id, a.AppointmentNumber,
        a.DoctorId,
        a.Doctor?.User?.FullName ?? string.Empty,
        a.Doctor?.Specialty,
        a.DepartmentId,
        a.Department?.Name,
        a.ScheduledStart, a.ScheduledEnd,
        a.Status.ToString(),
        a.Reason, a.Notes,
        a.ConsultationFee, a.Currency,
        a.CancelledAt, a.CancellationReason,
        a.CreatedOn,
        CanPatientCancel(a.Status));

    private static bool CanPatientCancel(AppointmentStatus status) =>
        status is AppointmentStatus.Requested
            or AppointmentStatus.Booked
            or AppointmentStatus.Confirmed;

    public static PortalPrescriptionResponse ToPrescription(Prescription p) => new(
        p.Id, p.VisitId, p.IssuedOn,
        p.Visit?.Doctor?.User?.FullName ?? string.Empty,
        p.Visit?.Doctor?.Specialty,
        p.GeneralInstructions,
        p.Items.OrderBy(i => i.SortOrder).ThenBy(i => i.Medicine).Select(ToPrescriptionItem).ToList());

    public static PortalPrescriptionItemResponse ToPrescriptionItem(PrescriptionItem i) => new(
        i.Id, i.Medicine, i.Strength, i.Dosage, i.Frequency, i.Duration, i.Instructions);

    public static PortalInvoiceSummaryResponse ToInvoiceSummary(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.AppointmentId, i.Appointment?.AppointmentNumber,
        i.Status.ToString(),
        i.IssuedAt, i.DueAt,
        i.Total, i.AmountPaid, i.AmountDue,
        i.Currency);

    public static PortalInvoiceDetailResponse ToInvoiceDetail(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.AppointmentId, i.Appointment?.AppointmentNumber,
        i.Status.ToString(),
        i.IssuedAt, i.DueAt, i.CancelledAt, i.CancellationReason,
        i.Subtotal, i.DiscountAmount, i.TaxAmount, i.Total, i.AmountPaid, i.AmountDue,
        i.Currency, i.Notes,
        i.LineItems.OrderBy(l => l.CreatedOn).Select(ToInvoiceLineItem).ToList(),
        i.Payments.Where(p => !p.IsDeleted).OrderByDescending(p => p.PaidAt).Select(ToInvoicePayment).ToList());

    public static PortalInvoiceLineItemResponse ToInvoiceLineItem(InvoiceLineItem l) => new(
        l.Id, l.Description, l.ServiceCode, l.Quantity, l.UnitPrice, l.LineTotal);

    public static PortalInvoicePaymentResponse ToInvoicePayment(InvoicePayment p) => new(
        p.Id, p.Amount, p.Method.ToString(), p.Reference, p.PaidAt);
}
