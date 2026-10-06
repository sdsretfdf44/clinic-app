using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Domain.Identity;

/// <summary>
/// The primary identity aggregate. Every user belongs to a tenant (SuperAdmin belongs to the platform tenant).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>, IAuditable, ISoftDelete, IHasConcurrencyToken
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => string.Join(' ', new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    public Guid TenantId { get; set; }

    public UserRole PrimaryRole { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public DateTimeOffset CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
