using Microsoft.EntityFrameworkCore;
using SportsStore.InventoryService.Models;

namespace SportsStore.InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(i => i.Price)
                .HasPrecision(8, 2);

            entity.HasIndex(i => i.ProductId)
                .IsUnique();
        });

        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.Property(r => r.Status)
                .HasConversion<string>();

            entity.HasIndex(r => r.OrderId);
            entity.HasIndex(r => r.CorrelationId);
        });
    }
}
