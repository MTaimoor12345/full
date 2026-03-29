namespace SportsStore.Shared.DTOs;

public class ShipmentDto
{
    public int ShipmentId { get; set; }
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime EstimatedDispatchDate { get; set; }
    public DateTime? ActualDispatchDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentRecordDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class InventoryRecordDto
{
    public int RecordId { get; set; }
    public int OrderId { get; set; }
    public long ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}
