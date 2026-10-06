namespace Arrapa.ClinicOS.Contracts.Clinic.WorkingHours;

public sealed record WorkingHoursSlot(int DayOfWeek, string StartTime, string EndTime);

public sealed record WorkingHoursResponse(
    Guid? DoctorId,
    IReadOnlyList<WorkingHoursSlot> Slots);

public sealed record SetWorkingHoursRequest(IReadOnlyList<WorkingHoursSlot> Slots);
