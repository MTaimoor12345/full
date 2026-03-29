using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Commands;

public record ProcessPaymentResultCommand(int OrderId, bool Success, string TransactionReference, string? RejectionReason) : IRequest<bool>;

public class ProcessPaymentResultCommandHandler : IRequestHandler<ProcessPaymentResultCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<ProcessPaymentResultCommandHandler> _logger;

    public ProcessPaymentResultCommandHandler(
        OrderDbContext context,
        ILogger<ProcessPaymentResultCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessPaymentResultCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("ProcessPaymentResult: Order not found - OrderId: {OrderId}", request.OrderId);
            return false;
        }

        // Create payment record
        var paymentRecord = new PaymentRecord
        {
            OrderId = order.OrderId,
            Amount = order.TotalAmount,
            Currency = "USD",
            Status = request.Success ? "Approved" : "Rejected",
            TransactionReference = request.TransactionReference,
            RejectionReason = request.RejectionReason,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        _context.PaymentRecords.Add(paymentRecord);

        if (request.Success)
        {
            // Payment approved - update status to ShippingPending
            order.Status = OrderStatus.ShippingPending;
            order.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Payment approved for order - OrderId: {OrderId}, TransactionRef: {TransactionRef}",
                request.OrderId, request.TransactionReference);
        }
        else
        {
            // Payment failed
            order.Status = OrderStatus.PaymentFailed;
            order.UpdatedAt = DateTime.UtcNow;

            _logger.LogWarning(
                "Payment failed for order - OrderId: {OrderId}, Reason: {Reason}",
                request.OrderId, request.RejectionReason);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
