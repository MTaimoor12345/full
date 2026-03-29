using Microsoft.EntityFrameworkCore;
using SportsStore.PaymentService.Data;
using SportsStore.PaymentService.Models;
using Xunit;

namespace SportsStore.PaymentService.Tests;

public class PaymentServiceTests
{
    private PaymentDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new PaymentDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void CanCreatePaymentTransaction()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var transaction = new PaymentTransaction
        {
            OrderId = 1,
            CustomerId = 1,
            Amount = 99.99m,
            Currency = "USD",
            Status = "Pending",
            CorrelationId = Guid.NewGuid()
        };

        // Act
        context.PaymentTransactions.Add(transaction);
        context.SaveChanges();

        // Assert
        Assert.Single(context.PaymentTransactions);
        Assert.Equal("Pending", context.PaymentTransactions.First().Status);
    }

    [Fact]
    public void CanApprovePayment()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var transaction = new PaymentTransaction
        {
            OrderId = 1,
            CustomerId = 1,
            Amount = 150m,
            Currency = "USD",
            Status = "Pending",
            CorrelationId = Guid.NewGuid()
        };
        context.PaymentTransactions.Add(transaction);
        context.SaveChanges();

        // Act
        transaction.Status = "Approved";
        transaction.TransactionReference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}";
        transaction.ProcessedAt = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        var saved = context.PaymentTransactions.First();
        Assert.Equal("Approved", saved.Status);
        Assert.NotNull(saved.TransactionReference);
        Assert.NotNull(saved.ProcessedAt);
    }

    [Fact]
    public void CanRejectPayment()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var transaction = new PaymentTransaction
        {
            OrderId = 1,
            CustomerId = 1,
            Amount = 200m,
            Currency = "USD",
            Status = "Pending",
            CorrelationId = Guid.NewGuid()
        };
        context.PaymentTransactions.Add(transaction);
        context.SaveChanges();

        // Act
        transaction.Status = "Rejected";
        transaction.RejectionReason = "Insufficient funds";
        transaction.ErrorCode = "DECLINED";
        transaction.ProcessedAt = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        var saved = context.PaymentTransactions.First();
        Assert.Equal("Rejected", saved.Status);
        Assert.Equal("Insufficient funds", saved.RejectionReason);
    }

    [Fact]
    public void CanCreateTestCard()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var card = new TestCard
        {
            CardNumber = "4111111111111111",
            CardType = "Success",
            Description = "Visa - Always approves"
        };

        // Act
        context.TestCards.Add(card);
        context.SaveChanges();

        // Assert
        Assert.Single(context.TestCards);
        Assert.Equal("Success", context.TestCards.First().CardType);
    }
}
