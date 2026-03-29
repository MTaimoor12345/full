using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportsStore.PaymentService.Data;
using SportsStore.PaymentService.Models;
using SportsStore.Shared.Messages;

namespace SportsStore.PaymentService.Consumers;

public class InventoryConfirmedConsumer : IConsumer<InventoryConfirmedEvent>
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<InventoryConfirmedConsumer> _logger;

    public InventoryConfirmedConsumer(
        PaymentDbContext context,
        ILogger<InventoryConfirmedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryConfirmedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Payment Service: Processing InventoryConfirmedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        try
        {
            // Create pending transaction record
            var transaction = new PaymentTransaction
            {
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = 0, // Will be updated when we get amount from order
                Currency = "USD",
                Status = "Pending",
                CorrelationId = message.CorrelationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Simulate payment processing delay
            await Task.Delay(150);

            // Determine payment outcome
            var random = new Random();
            int outcome = random.Next(100);

            bool approved;
            string? rejectionReason = null;
            string? errorCode = null;

            // 85% success rate, 10% decline, 5% error
            if (outcome < 85)
            {
                approved = true;
                _logger.LogInformation(
                    "Payment approved for OrderId: {OrderId}",
                    message.OrderId);
            }
            else if (outcome < 95)
            {
                approved = false;
                rejectionReason = "Card declined - Insufficient funds";
                errorCode = "DECLINED_INSUFFICIENT_FUNDS";
                _logger.LogWarning(
                    "Payment declined for OrderId: {OrderId} - {Reason}",
                    message.OrderId, rejectionReason);
            }
            else
            {
                approved = false;
                rejectionReason = "Payment processing error";
                errorCode = "PROCESSING_ERROR";
                _logger.LogWarning(
                    "Payment error for OrderId: {OrderId} - {Reason}",
                    message.OrderId, rejectionReason);
            }

            // Update transaction record
            transaction.Status = approved ? "Approved" : "Rejected";
            transaction.TransactionReference = approved
                ? $"PAY-{DateTime.UtcNow:yyyyMMdd}-{random.Next(100000, 999999)}"
                : null;
            transaction.RejectionReason = rejectionReason;
            transaction.ErrorCode = errorCode;
            transaction.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (approved)
            {
                var approvedEvent = new PaymentApprovedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    CustomerId = message.CustomerId,
                    Amount = transaction.Amount,
                    TransactionReference = transaction.TransactionReference!,
                    ProcessedAt = transaction.ProcessedAt.Value
                };

                await context.Publish(approvedEvent);

                _logger.LogInformation(
                    "PaymentApprovedEvent published - OrderId: {OrderId}, TransactionRef: {TransactionRef}",
                    message.OrderId, transaction.TransactionReference);
            }
            else
            {
                var rejectedEvent = new PaymentRejectedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    CustomerId = message.CustomerId,
                    Amount = transaction.Amount,
                    RejectionReason = rejectionReason!,
                    ErrorCode = errorCode
                };

                await context.Publish(rejectedEvent);

                _logger.LogWarning(
                    "PaymentRejectedEvent published - OrderId: {OrderId}, Reason: {Reason}",
                    message.OrderId, rejectionReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing payment for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                message.OrderId, message.CorrelationId);

            var rejectedEvent = new PaymentRejectedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = 0,
                RejectionReason = $"Payment service error: {ex.Message}",
                ErrorCode = "SERVICE_ERROR"
            };

            await context.Publish(rejectedEvent);
        }
    }
}
