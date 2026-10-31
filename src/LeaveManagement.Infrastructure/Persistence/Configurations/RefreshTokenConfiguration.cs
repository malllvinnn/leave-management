using LeaveManagement.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.Property((refreshToken) => refreshToken.Jti).HasMaxLength(100);
        builder.Property((refreshToken) => refreshToken.TokenHash).HasMaxLength(64);

        builder.HasIndex((refreshToken) => refreshToken.Jti)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_jti");

        builder.HasIndex((refreshToken) => refreshToken.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_refresh_tokens_token_hash");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey((refreshToken) => refreshToken.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}