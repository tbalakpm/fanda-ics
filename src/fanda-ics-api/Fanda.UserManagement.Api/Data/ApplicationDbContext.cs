using Fanda.UserManagement.Api.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fanda.UserManagement.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser extensions
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users"); // Rename table if needed
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.UserName).IsRequired().HasMaxLength(256);
            entity.Property(u => u.NormalizedUserName).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.PasswordHash).HasMaxLength(256);
            entity.Property(u => u.SecurityStamp).HasMaxLength(256);
            entity.Property(u => u.ConcurrencyStamp).HasMaxLength(256);
            entity.Property(u => u.EmailConfirmed).HasDefaultValue(false);
            entity.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false);
            entity.Property(u => u.TwoFactorEnabled).HasDefaultValue(false);
            entity.Property(u => u.LockoutEnabled).HasDefaultValue(false);
            entity.Property(u => u.AccessFailedCount).HasDefaultValue(0);

            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(u => u.NormalizedUserName).IsUnique();
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
        });

        // Configure ApplicationRole extensions
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("roles"); // Rename table if needed

            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.NormalizedName).IsRequired().HasMaxLength(50);
            entity.Property(r => r.ConcurrencyStamp).HasMaxLength(256);
            entity.Property(r => r.Description).HasMaxLength(256);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("user_roles"); // Renames AspNetUserRoles to user_roles
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("user_claims"); // Renames AspNetUserClaims to user_claims
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("user_logins"); // Renames AspNetUserLogins to user_logins
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("role_claims"); // Renames AspNetRoleClaims to role_claims
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("user_tokens"); // Renames AspNetUserTokens to user_tokens
        });

        // Configure RefreshToken entity
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.UserId).IsRequired();
            entity.Property(rt => rt.ExpiresAt).IsRequired();
            entity.Property(rt => rt.IsActive).HasDefaultValue(true);
            entity.Property(rt => rt.IsRevoked).HasDefaultValue(false);
            entity.Property(rt => rt.ReplacedByToken).HasMaxLength(500);
            entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(rt => rt.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
        });
    }
}
