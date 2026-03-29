using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportsStore.ShippingService.Data;
using SportsStore.ShippingService.Models;
using SportsStore.Shared.Messages;

namespace SportsStore.ShippingService.Consumers;

public class PaymentApprovedConsumer : IConsumer<PaymentApprovedEvent>
{
    private readonly ShippingDbContext _context;
    private readonly ILogger<PaymentApprovedConsumer> _logger;

    public PaymentApprovedConsumer(
        ShippingDbContext context,
        ILogger<PaymentApprovedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Shipping Service: Processing PaymentApprovedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        try
        {
            // Get active carriers
            var carriers = await _context.ShippingCarriers
                .Where(c => c.IsActive)
                .ToListAsync();

            if (!carriers.Any())
            {
                _logger.LogError("No active shipping carriers available");
                throw new InvalidOperationException("No active shipping carriers available");
            }

            // Select a random carrier
            var random = new Random();
            var carrier = carriers[random.Next(carriers.Count)];

            // Generate tracking number
            var trackingNumber = $"TRK{DateTime.UtcNow:yyyyMMdd}{random.Next(1000000, 9999999)}";

            // Calculate dates
            var estimatedDispatchDate = DateTime.UtcNow.AddDays(random.Next(1, 3));
            var estimatedDeliveryDate = estimatedDispatchDate.AddDays(carrier.EstimatedDays);

            // Create shipment record
            var shipment = new Shipment
            {
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                TrackingNumber = trackingNumber,
                Carrier = carrier.Name,
                Status = "Created",
                EstimatedDispatchDate = estimatedDispatchDate,
                EstimatedDeliveryDate = estimatedDeliveryDate,
                CorrelationId = message.CorrelationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Shipment created - OrderId: {OrderId}, ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}, Carrier: {Carrier}",
                message.OrderId, shipment.ShipmentId, trackingNumber, carrier.Name);

            // Simulate processing delay
            await Task.Delay(100);

            // Publish shipping created event
            var shippingCreatedEvent = new ShippingCreatedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                ShipmentId = shipment.ShipmentId,
                TrackingNumber = trackingNumber,
                Carrier = carrier.Name,
                EstimatedDispatchDate = estimatedDispatchDate
            };

            await context.Publish(shippingCreatedEvent);

            _logger.LogInformation(
                "ShippingCreatedEvent published - OrderId: {OrderId}, ShipmentId: {ShipmentId}",
                message.OrderId, shipment.ShipmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing shipping for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
                message.OrderId, message.CorrelationId);

            // Note: In a production system, you might want to publish a ShippingFailedEvent
            // For now, we'll just log the error and let the system retry or handle it manually
            throw;
        }
    }
}
