namespace SportsStore.Shared.DTOs;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => ProductPrice * Quantity;
}

public class CreateOrderItemDto
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
}
