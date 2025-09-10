using Fanda.ICS.Api.Enums;
using Fanda.ICS.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Fanda.ICS.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Unit> Units { get; set; } = default!;
    public DbSet<ItemCategory> ItemCategories { get; set; } = default!;
    public DbSet<Item> Items { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure PostgreSQL enums
        modelBuilder.HasPostgresEnum<GTNGenerations>();
        modelBuilder.HasPostgresEnum<ItemOfferings>();
        modelBuilder.HasPostgresEnum<TaxTreatments>();

        //  Configure entities

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShortName).HasMaxLength(10);
            entity.Property(e => e.UnitName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShortName).HasMaxLength(10);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ItemName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.UnitId).IsRequired();
            entity.Property(e => e.GTNGeneration).IsRequired();
            entity.Property(e => e.Offering).IsRequired();
            entity.Property(e => e.TaxTreatment).IsRequired();
            entity.Property(e => e.IsBarcoded).HasDefaultValue(false);
            entity.Property(e => e.IsExpiryDated).HasDefaultValue(false);
            entity.Property(e => e.IsBatchTracked).HasDefaultValue(false);
            entity.Property(e => e.IsSKUGenerated).HasDefaultValue(false);
            entity.Property(e => e.IsReturnable).HasDefaultValue(false);
            entity.Property(e => e.IsDiscontinued).HasDefaultValue(false);
            entity.Property(e => e.Warranty).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Unit)
                .WithMany(u => u.Items)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Attributes)
                .WithOne(p => p.Item)
                .HasForeignKey<ItemAttributes>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.EOQ)
                .WithOne(p => p.Item)
                .HasForeignKey<ItemEOQ>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Pricing)
                .WithOne(p => p.Item)
                .HasForeignKey<ItemPricing>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Taxation)
                .WithOne(p => p.Item)
                .HasForeignKey<ItemTaxation>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
