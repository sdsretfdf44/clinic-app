using Arrapa.ClinicOS.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(64);
        builder.Property(rt => rt.IpAddress).HasMaxLength(45);
        builder.Property(rt => rt.UserAgent).HasMaxLength(500);

        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });

        builder.Property(rt => rt.RowVersion).IsRowVersion();

        builder.HasQueryFilter(rt => !rt.IsDeleted);

        builder.Ignore(rt => rt.DomainEvents);
    }
}
