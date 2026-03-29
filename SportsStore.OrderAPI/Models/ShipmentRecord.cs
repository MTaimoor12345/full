using System.ComponentModel.DataAnnotations;

namespace SportsStore.OrderAPI.Models;

public class ShipmentRecord
{
    [Key]
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Required]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string Carrier { get; set; } = string.Empty;

    public DateTime EstimatedDispatchDate { get; set; }
    public DateTime? ActualDispatchDate { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
