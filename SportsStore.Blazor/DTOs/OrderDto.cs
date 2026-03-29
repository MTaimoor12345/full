namespace SportsStore.Blazor.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();

    // Shipping Address
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
}

public class OrderItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ProductPrice { get; set; }
    public decimal LineTotal => ProductPrice * Quantity;
}

public class CheckoutDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool GiftWrap { get; set; }
    public List<CheckoutItemDto> CartItems { get; set; } = new();
}

public class CheckoutItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ProductPrice { get; set; }
}

public class CheckoutRequestDto
{
    public CheckoutDto Checkout { get; set; } = new();
    public string? BlazorBaseUrl { get; set; }
}

public class CheckoutResultDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public string? StripeSessionId { get; set; }
    public bool RequiresPayment { get; set; }
    public string? ErrorMessage { get; set; }
}
