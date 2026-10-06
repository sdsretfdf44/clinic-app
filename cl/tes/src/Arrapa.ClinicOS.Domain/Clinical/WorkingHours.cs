using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A single working-hours slot. A doctor's weekly schedule is expressed as multiple rows keyed by
/// <see cref="DayOfWeek"/> — supporting several slots per day (e.g., morning + afternoon).
/// <para>
/// When <see cref="DoctorId"/> is <c>null</c>, the row represents the clinic-wide default schedule used by
/// receptionists and any doctor without an explicit schedule.
/// </para>
/// </summary>
public sealed class WorkingHours : BaseEntity
{
    public Guid? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Start of the slot, minutes since midnight.</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End of the slot, minutes since midnight (exclusive).</summary>
    public TimeOnly EndTime { get; set; }
}
