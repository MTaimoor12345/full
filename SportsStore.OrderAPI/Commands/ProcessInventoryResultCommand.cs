using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.Enums;
using SportsStore.Shared.Messages;

namespace SportsStore.OrderAPI.Commands;

public record ProcessInventoryResultCommand(InventoryResult Result, int OrderId, bool Success, string? Message) : IRequest<bool>;

public enum InventoryResult { Confirmed, Failed }

public class ProcessInventoryResultCommandHandler : IRequestHandler<ProcessInventoryResultCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<ProcessInventoryResultCommandHandler> _logger;

    public ProcessInventoryResultCommandHandler(
        OrderDbContext context,
        ILogger<ProcessInventoryResultCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessInventoryResultCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("ProcessInventoryResult: Order not found - OrderId: {OrderId}", request.OrderId);
            return false;
        }

        if (request.Success)
        {
            // Inventory confirmed - update status to PaymentPending
            order.Status = OrderStatus.PaymentPending;
            order.UpdatedAt = DateTime.UtcNow;

            // Create inventory records
            foreach (var item in order.Items)
            {
                _context.InventoryRecords.Add(new InventoryRecord
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    RequestedQuantity = item.Quantity,
                    ReservedQuantity = item.Quantity,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "Inventory confirmed for order - OrderId: {OrderId}",
                request.OrderId);
        }
        else
        {
            // Inventory failed
            order.Status = OrderStatus.InventoryFailed;
            order.UpdatedAt = DateTime.UtcNow;

            _logger.LogWarning(
                "Inventory failed for order - OrderId: {OrderId}, Reason: {Reason}",
                request.OrderId, request.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
