using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportsStore.InventoryService.Data;
using SportsStore.InventoryService.Models;
using SportsStore.Shared.Messages;

namespace SportsStore.InventoryService.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(
        InventoryDbContext context,
        ILogger<OrderSubmittedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Inventory Service: Processing OrderSubmittedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        try
        {
            // Check inventory for all items
            var failedItems = new List<InventoryFailureItem>();
            var reservedItems = new List<InventoryReservationItem>();
            var reservations = new List<InventoryReservation>();

            foreach (var item in message.Items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventoryItem == null)
                {
                    _logger.LogWarning(
                        "Product not found in inventory - ProductId: {ProductId}, ProductName: {ProductName}",
                        item.ProductId, item.ProductName);

                    failedItems.Add(new InventoryFailureItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = 0
                    });
                    continue;
                }

                if (inventoryItem.AvailableQuantity < item.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock - ProductId: {ProductId}, ProductName: {ProductName}, Requested: {Requested}, Available: {Available}",
                        item.ProductId, item.ProductName, item.Quantity, inventoryItem.AvailableQuantity);

                    failedItems.Add(new InventoryFailureItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = inventoryItem.AvailableQuantity
                    });
                    continue;
                }

                // Reserve the stock
                inventoryItem.ReservedQuantity += item.Quantity;
                inventoryItem.LastUpdated = DateTime.UtcNow;

                reservedItems.Add(new InventoryReservationItem
                {
                    ProductId = item.ProductId,
                    ReservedQuantity = item.Quantity
                });

                // Create reservation record
                reservations.Add(new InventoryReservation
                {
                    OrderId = message.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Status = "Reserved",
                    CorrelationId = message.CorrelationId,
                    CreatedAt = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "Stock reserved - ProductId: {ProductId}, Quantity: {Quantity}, NewReserved: {NewReserved}",
                    item.ProductId, item.Quantity, inventoryItem.ReservedQuantity);
            }

            await _context.SaveChangesAsync();

            if (failedItems.Any())
            {
                // Some items failed - release any reservations made and publish failure
                foreach (var reservation in reservations)
                {
                    var invItem = await _context.InventoryItems
                        .FirstOrDefaultAsync(i => i.ProductId == reservation.ProductId);
                    if (invItem != null)
                    {
                        invItem.ReservedQuantity -= reservation.Quantity;
                        invItem.LastUpdated = DateTime.UtcNow;
                    }
                    reservation.Status = "Released";
                    reservation.ReleasedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "Inventory check failed for OrderId: {OrderId}, FailedItems: {FailedCount}",
                    message.OrderId, failedItems.Count);

                var failedEvent = new InventoryFailedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    CustomerId = message.CustomerId,
                    FailedItems = failedItems,
                    FailureReason = "Insufficient stock for one or more items"
                };

                await context.Publish(failedEvent);
            }
            else
            {
                // All items available - confirm reservations
                foreach (var reservation in reservations)
                {
                    reservation.Status = "Confirmed";
                    reservation.ConfirmedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inventory confirmed for OrderId: {OrderId}, ItemsReserved: {ItemsCount}",
                    message.OrderId, reservedItems.Count);

                var confirmedEvent = new InventoryConfirmedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    CustomerId = message.CustomerId,
                    ReservedItems = reservedItems
                };

                await context.Publish(confirmedEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing inventory for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                message.OrderId, message.CorrelationId);

            var failedEvent = new InventoryFailedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                FailedItems = message.Items.Select(i => new InventoryFailureItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    RequestedQuantity = i.Quantity,
                    AvailableQuantity = 0
                }).ToList(),
                FailureReason = $"Inventory service error: {ex.Message}"
            };

            await context.Publish(failedEvent);
        }
    }
}
