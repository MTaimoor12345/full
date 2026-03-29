using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsStore.OrderAPI.Models;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public long ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(8, 2)")]
    public decimal ProductPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [NotMapped]
    public decimal LineTotal => ProductPrice * Quantity;
}
