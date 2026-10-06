using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Identity;

/// <summary>
/// A refresh token used to mint new access tokens without re-authenticating the user.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    [MaxLength(64)]
    public string? ReplacedByToken { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public bool IsActive => RevokedAt is null && !IsExpired;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}
