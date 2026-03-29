using Microsoft.EntityFrameworkCore;
using SportsStore.ShippingService.Models;

namespace SportsStore.ShippingService.Data;

public class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShippingCarrier> ShippingCarriers => Set<ShippingCarrier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasIndex(s => s.OrderId);
            entity.HasIndex(s => s.TrackingNumber)
                .IsUnique();
            entity.HasIndex(s => s.CorrelationId);
            entity.HasIndex(s => s.Status);
        });

        modelBuilder.Entity<ShippingCarrier>(entity =>
        {
            entity.HasIndex(c => c.Name)
                .IsUnique();
        });
    }
}
