using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Commands;

public record CreateShipmentCommand(
    int OrderId,
    int ShipmentId,
    string TrackingNumber,
    string Carrier,
    DateTime EstimatedDispatchDate) : IRequest<bool>;

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, bool>
{
    private readonly OrderDbContext _context;
    private readonly ILogger<CreateShipmentCommandHandler> _logger;

    public CreateShipmentCommandHandler(
        OrderDbContext context,
        ILogger<CreateShipmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("CreateShipment: Order not found - OrderId: {OrderId}", request.OrderId);
            return false;
        }

        // Create shipment record
        var shipmentRecord = new ShipmentRecord
        {
            OrderId = order.OrderId,
            TrackingNumber = request.TrackingNumber,
            Carrier = request.Carrier,
            EstimatedDispatchDate = request.EstimatedDispatchDate,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };

        _context.ShipmentRecords.Add(shipmentRecord);

        // Update order status to Completed
        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Shipment created and order completed - OrderId: {OrderId}, ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}",
            request.OrderId, shipmentRecord.ShipmentId, request.TrackingNumber);

        return true;
    }
}
