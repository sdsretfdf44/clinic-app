namespace Arrapa.ClinicOS.Shared.Time;

/// <summary>
/// Abstraction over the system clock for testability. Always returns UTC.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly TodayUtc { get; }
}
