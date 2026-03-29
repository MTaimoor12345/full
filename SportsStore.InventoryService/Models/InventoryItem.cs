using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsStore.InventoryService.Models;

/// <summary>
/// Represents inventory stock for a product
/// </summary>
public class InventoryItem
{
    [Key]
    public long ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(8, 2)")]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    public int ReservedQuantity { get; set; }

    [NotMapped]
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tracks inventory reservations for orders
/// </summary>
public class InventoryReservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public long ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public string Status { get; set; } = "Reserved"; // Reserved, Confirmed, Released

    public Guid CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }
}
