using SportsStore.PaymentService.Data;
using SportsStore.PaymentService.Models;

namespace SportsStore.PaymentService.Data;

public static class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        if (!context.TestCards.Any())
        {
            context.TestCards.AddRange(
                // Success cards
                new TestCard { CardNumber = "4111111111111111", CardType = "Success", Description = "Visa - Always approves" },
                new TestCard { CardNumber = "5555555555554444", CardType = "Success", Description = "MasterCard - Always approves" },
                new TestCard { CardNumber = "378282246310005", CardType = "Success", Description = "Amex - Always approves" },

                // Decline cards
                new TestCard { CardNumber = "4000000000000002", CardType = "Decline", Description = "Visa - Always declines" },
                new TestCard { CardNumber = "5105105105105100", CardType = "Decline", Description = "MasterCard - Always declines" },

                // Error cards
                new TestCard { CardNumber = "4000000000000119", CardType = "Error", Description = "Visa - Processing error" },
                new TestCard { CardNumber = "4000000000003220", CardType = "Error", Description = "Visa - 3D Secure required" }
            );
            context.SaveChanges();
        }

        // Add sample payment transactions for demo
        if (!context.PaymentTransactions.Any())
        {
            context.PaymentTransactions.AddRange(
                new PaymentTransaction
                {
                    OrderId = 1,
                    CustomerId = 1,
                    Amount = 275.00m,
                    Currency = "USD",
                    Status = "Completed",
                    PaymentMethod = "Stripe",
                    TransactionReference = "pi_3OkTest123456",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ProcessedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2)
                },
                new PaymentTransaction
                {
                    OrderId = 2,
                    CustomerId = 2,
                    Amount = 530.00m,
                    Currency = "USD",
                    Status = "Completed",
                    PaymentMethod = "Stripe",
                    TransactionReference = "pi_3OkTest234567",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    ProcessedAt = DateTime.UtcNow.AddDays(-4).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    OrderId = 3,
                    CustomerId = 3,
                    Amount = 1375.00m,
                    Currency = "USD",
                    Status = "Pending",
                    PaymentMethod = "Stripe",
                    TransactionReference = "pending_abc123",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new PaymentTransaction
                {
                    OrderId = 4,
                    CustomerId = 1,
                    Amount = 150.00m,
                    Currency = "USD",
                    Status = "Failed",
                    PaymentMethod = "Stripe",
                    TransactionReference = "pi_declined_001",
                    ErrorCode = "card_declined",
                    RejectionReason = "Your card was declined.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ProcessedAt = DateTime.UtcNow.AddDays(-1).AddSeconds(30)
                },
                new PaymentTransaction
                {
                    OrderId = 5,
                    CustomerId = 2,
                    Amount = 89.95m,
                    Currency = "USD",
                    Status = "Completed",
                    PaymentMethod = "Stripe",
                    TransactionReference = "pi_3OkTest345678",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ProcessedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1)
                }
            );
            context.SaveChanges();
        }
    }
}
