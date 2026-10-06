using System.Runtime.CompilerServices;

namespace Arrapa.ClinicOS.Shared.Guards;

/// <summary>
/// Minimal guard clauses used throughout the domain and application layers.
/// </summary>
public static class Guard
{
    public static T AgainstNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    public static string AgainstNullOrEmpty(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
        return value!;
    }

    public static string AgainstNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value!;
    }

    public static Guid AgainstEmpty(Guid value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Guid cannot be empty.", paramName);
        }

        return value;
    }
}
