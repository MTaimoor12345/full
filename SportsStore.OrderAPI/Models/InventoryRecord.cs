using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsStore.OrderAPI.Models;

public class InventoryRecord
{
    [Key]
    public int RecordId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public int RequestedQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
