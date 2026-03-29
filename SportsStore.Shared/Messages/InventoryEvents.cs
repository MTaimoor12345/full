namespace SportsStore.Shared.Messages;

/// <summary>
/// Published to request inventory validation for an order
/// </summary>
public class InventoryCheckRequestedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public List<InventoryCheckItem> Items { get; set; } = new();
}

public class InventoryCheckItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
}

/// <summary>
/// Published when inventory check passes for all items
/// </summary>
public class InventoryConfirmedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public List<InventoryReservationItem> ReservedItems { get; set; } = new();
}

public class InventoryReservationItem
{
    public long ProductId { get; set; }
    public int ReservedQuantity { get; set; }
}

/// <summary>
/// Published when inventory check fails (insufficient stock)
/// </summary>
public class InventoryFailedEvent : BaseMessage
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public List<InventoryFailureItem> FailedItems { get; set; } = new();
    public string FailureReason { get; set; } = string.Empty;
}

public class InventoryFailureItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
