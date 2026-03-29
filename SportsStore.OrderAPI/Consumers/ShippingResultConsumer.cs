using MassTransit;
using MediatR;
using SportsStore.OrderAPI.Commands;
using SportsStore.Shared.Messages;

namespace SportsStore.OrderAPI.Consumers;

public class ShippingResultConsumer : IConsumer<ShippingCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ShippingResultConsumer> _logger;

    public ShippingResultConsumer(IMediator mediator, ILogger<ShippingResultConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShippingCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "OrderAPI received ShippingCreatedEvent - OrderId: {OrderId}, ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}, CorrelationId: {CorrelationId}",
            message.OrderId, message.ShipmentId, message.TrackingNumber, message.CorrelationId);

        var command = new CreateShipmentCommand(
            message.OrderId,
            message.ShipmentId,
            message.TrackingNumber,
            message.Carrier,
            message.EstimatedDispatchDate);

        await _mediator.Send(command);
    }
}
