namespace SportsStore.Shared.Messages;

/// <summary>
/// Published to request shipment creation for an order
/// </summary>
public class ShippingRequestedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
    public int TotalItems { get; set; }
}

/// <summary>
/// Published when shipment is created successfully
/// </summary>
public class ShippingCreatedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
}

/// <summary>
/// Published when shipment creation fails
/// </summary>
public class ShippingFailedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
}
