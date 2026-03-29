using SportsStore.Shared.Enums;

namespace SportsStore.Shared.Messages;

/// <summary>
/// Base message with correlation support for distributed tracing
/// </summary>
public abstract class BaseMessage
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

/// <summary>
/// Published when a customer submits an order for processing
/// </summary>
public class OrderSubmittedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
}

public class OrderItemMessage
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Published when order processing fails at any stage
/// </summary>
public class OrderFailedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public string FailedAtStage { get; set; } = string.Empty;
    public OrderStatus PreviousStatus { get; set; }
}

/// <summary>
/// Published when order processing completes successfully
/// </summary>
public class OrderCompletedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
}
