using MassTransit;
using MediatR;
using SportsStore.OrderAPI.Commands;
using SportsStore.Shared.Messages;

namespace SportsStore.OrderAPI.Consumers;

public class InventoryResultConsumer : 
    IConsumer<InventoryConfirmedEvent>,
    IConsumer<InventoryFailedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<InventoryResultConsumer> _logger;

    public InventoryResultConsumer(IMediator mediator, ILogger<InventoryResultConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryConfirmedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "OrderAPI received InventoryConfirmedEvent - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CorrelationId);

        var command = new ProcessInventoryResultCommand(
            InventoryResult.Confirmed,
            message.OrderId,
            true,
            null);

        await _mediator.Send(command);
    }

    public async Task Consume(ConsumeContext<InventoryFailedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogWarning(
            "OrderAPI received InventoryFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
            message.OrderId, message.FailureReason, message.CorrelationId);

        var command = new ProcessInventoryResultCommand(
            InventoryResult.Failed,
            message.OrderId,
            false,
            message.FailureReason);

        await _mediator.Send(command);
    }
}
