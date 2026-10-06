namespace Arrapa.ClinicOS.Application.Abstractions.Email;

/// <summary>
/// A single outbound email. Providers may render <see cref="HtmlBody"/> only, <see cref="TextBody"/> only, or both.
/// </summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? ToDisplayName = null);
