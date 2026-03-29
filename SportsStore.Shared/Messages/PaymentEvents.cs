namespace SportsStore.Shared.Messages;

/// <summary>
/// Published to request payment processing for an order
/// </summary>
public class PaymentProcessingRequestedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Published when payment is approved
/// </summary>
public class PaymentApprovedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Published when payment is rejected
/// </summary>
public class PaymentRejectedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
