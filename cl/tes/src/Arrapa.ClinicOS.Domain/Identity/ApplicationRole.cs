using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Domain.Identity;

/// <summary>
/// Role scoped to a tenant (or global for platform roles when <see cref="TenantId"/> is null).
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsSystemRole { get; set; }

    public ApplicationRole()
    {
    }

    public ApplicationRole(string name) : base(name)
    {
        NormalizedName = name.ToUpperInvariant();
    }
}
