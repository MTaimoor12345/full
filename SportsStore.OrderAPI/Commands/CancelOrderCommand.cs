using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Commands;

public record CancelOrderCommand(int OrderId, string Reason) : IRequest<bool>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        OrderDbContext context,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("CancelOrder: Order not found - OrderId: {OrderId}", request.OrderId);
            return false;
        }

        // Can only cancel orders that are not completed
        if (order.Status == OrderStatus.Completed)
        {
            _logger.LogWarning("CancelOrder: Cannot cancel completed order - OrderId: {OrderId}", request.OrderId);
            return false;
        }

        order.Status = OrderStatus.Failed;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order cancelled - OrderId: {OrderId}, Reason: {Reason}",
            request.OrderId, request.Reason);

        return true;
    }
}
