using MassTransit;
using MediatR;
using SportsStore.OrderAPI.Commands;
using SportsStore.Shared.Messages;

namespace SportsStore.OrderAPI.Consumers;

public class PaymentResultConsumer :
    IConsumer<PaymentApprovedEvent>,
    IConsumer<PaymentRejectedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentResultConsumer> _logger;

    public PaymentResultConsumer(IMediator mediator, ILogger<PaymentResultConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "OrderAPI received PaymentApprovedEvent - OrderId: {OrderId}, TransactionRef: {TransactionRef}, CorrelationId: {CorrelationId}",
            message.OrderId, message.TransactionReference, message.CorrelationId);

        var command = new ProcessPaymentResultCommand(
            message.OrderId,
            true,
            message.TransactionReference,
            null);

        await _mediator.Send(command);
    }

    public async Task Consume(ConsumeContext<PaymentRejectedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogWarning(
            "OrderAPI received PaymentRejectedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
            message.OrderId, message.RejectionReason, message.CorrelationId);

        var command = new ProcessPaymentResultCommand(
            message.OrderId,
            false,
            string.Empty,
            message.RejectionReason);

        await _mediator.Send(command);
    }
}
