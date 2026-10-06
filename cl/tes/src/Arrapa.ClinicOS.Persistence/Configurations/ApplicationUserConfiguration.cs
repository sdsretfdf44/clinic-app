using Arrapa.ClinicOS.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.PrimaryRole).HasConversion<int>();

        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(u => u.RowVersion).IsRowVersion();

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
