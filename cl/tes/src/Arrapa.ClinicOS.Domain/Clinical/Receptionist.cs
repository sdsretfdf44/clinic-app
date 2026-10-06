using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Identity;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A receptionist / front-desk staff member. Backed by an <see cref="ApplicationUser"/>.
/// </summary>
public sealed class Receptionist : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
