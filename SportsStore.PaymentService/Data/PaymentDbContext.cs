using Microsoft.EntityFrameworkCore;
using SportsStore.PaymentService.Models;

namespace SportsStore.PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<TestCard> TestCards => Set<TestCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.Property(p => p.Amount)
                .HasPrecision(10, 2);

            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.CorrelationId);
            entity.HasIndex(p => p.Status);
        });

        modelBuilder.Entity<TestCard>(entity =>
        {
            entity.HasIndex(t => t.CardNumber)
                .IsUnique();
        });
    }
}
