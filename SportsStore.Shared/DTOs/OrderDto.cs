using SportsStore.Shared.Enums;

namespace SportsStore.Shared.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }

    // Shipping Address
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }

    public bool GiftWrap { get; set; }
    public string? StripeSessionId { get; set; }
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
}

public class CheckoutDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
}

public class CartItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderStatusDto
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<OrderEventDto> Events { get; set; } = new();
}

public class OrderEventDto
{
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
}

// Stripe Payment DTOs
public class PaymentSessionRequest
{
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class PaymentSessionResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class PaymentVerificationResponse
{
    public bool IsPaid { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

// Checkout Result DTO
public class CheckoutResultDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentUrl { get; set; }
    public string? StripeSessionId { get; set; }
    public bool RequiresPayment { get; set; }
    public string? ErrorMessage { get; set; }
}

// Checkout Request DTO
public class CheckoutRequest
{
    public CheckoutDto Checkout { get; set; } = new();
    public string? BlazorBaseUrl { get; set; }
}

// Inventory Check Result
public class InventoryCheckResult
{
    public bool AllAvailable { get; set; }
    public List<InventoryItemCheckResult>? Items { get; set; }
}

public class InventoryItemCheckResult
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
