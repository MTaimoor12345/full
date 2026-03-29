using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    // Shipping Address
    [Required(ErrorMessage = "Please enter the first address line")]
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }

    [Required(ErrorMessage = "Please enter a city name")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Please enter a state name")]
    public string? State { get; set; }

    public string? Zip { get; set; }

    [Required(ErrorMessage = "Please enter a country name")]
    public string? Country { get; set; }

    public bool GiftWrap { get; set; }

    public string? StripeSessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public PaymentRecord? Payment { get; set; }
    public ShipmentRecord? Shipment { get; set; }
    public ICollection<InventoryRecord> InventoryRecords { get; set; } = new List<InventoryRecord>();
}
