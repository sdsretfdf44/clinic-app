using Arrapa.ClinicOS.Shared.Time;

namespace Arrapa.ClinicOS.Infrastructure.Services;

/// <summary>
/// Production clock implementation. Always returns UTC.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
}
