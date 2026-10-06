using System.Security.Cryptography;
using Arrapa.ClinicOS.Application.Abstractions.Patients;
using Arrapa.ClinicOS.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Persistence.Services;

/// <summary>
/// Produces a 12-char MRN in the format <c>PAT-XXXXXXXX</c>, drawing from a confusing-char-safe alphabet
/// (no 0/O, no 1/I/L). Retries with a fresh value on unique-index collision.
/// </summary>
public sealed class PatientMrnGenerator : IPatientMrnGenerator
{
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int SuffixLength = 8;
    private const int MaxAttempts = 10;

    private readonly ApplicationDbContext _db;

    public PatientMrnGenerator(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = "PAT-" + GenerateSuffix();
            var exists = await _db.Patients.IgnoreQueryFilters()
                .AnyAsync(p => p.TenantId == tenantId && p.MRN == candidate && !p.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (!exists)
            {
                return candidate;
            }
        }

        // Astronomically unlikely — 31^8 candidates in the pool — but fall back to a Guid-derived suffix.
        return "PAT-" + Guid.NewGuid().ToString("N")[..SuffixLength].ToUpperInvariant();
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
