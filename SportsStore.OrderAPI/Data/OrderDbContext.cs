using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Models;

namespace SportsStore.OrderAPI.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<ShipmentRecord> ShipmentRecords => Set<ShipmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.TotalAmount)
                .HasPrecision(10, 2);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(oi => oi.ProductPrice)
                .HasPrecision(8, 2);
        });

        // PaymentRecord configuration
        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<PaymentRecord>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Amount)
                .HasPrecision(10, 2);
        });

        // ShipmentRecord configuration
        modelBuilder.Entity<ShipmentRecord>(entity =>
        {
            entity.HasOne(s => s.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<ShipmentRecord>(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // InventoryRecord configuration
        modelBuilder.Entity<InventoryRecord>(entity =>
        {
            entity.HasOne(i => i.Order)
                .WithMany(o => o.InventoryRecords)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price)
                .HasPrecision(8, 2);
        });
    }
}
