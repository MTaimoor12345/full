namespace SportsStore.Blazor.DTOs;

public class CartItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.LineTotal);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
