using System.Security.Cryptography;
using Arrapa.ClinicOS.Application.Abstractions.Appointments;
using Arrapa.ClinicOS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Persistence.Services;

/// <summary>
/// Emits appointment numbers in the format <c>APT-XXXXXXXX</c> from a confusing-char-safe alphabet.
/// </summary>
public sealed class AppointmentNumberGenerator : IAppointmentNumberGenerator
{
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int SuffixLength = 8;
    private const int MaxAttempts = 10;

    private readonly ApplicationDbContext _db;

    public AppointmentNumberGenerator(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = "APT-" + GenerateSuffix();
            var exists = await _db.Appointments.IgnoreQueryFilters()
                .AnyAsync(a => a.TenantId == tenantId && a.AppointmentNumber == candidate && !a.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (!exists)
            {
                return candidate;
            }
        }

        return "APT-" + Guid.NewGuid().ToString("N")[..SuffixLength].ToUpperInvariant();
    }

    private static string GenerateSuffix()
    {
        Span<char> buffer = stackalloc char[SuffixLength];
        for (var i = 0; i < SuffixLength; i++)
        {
            buffer[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }
        return new string(buffer);
    }
}
