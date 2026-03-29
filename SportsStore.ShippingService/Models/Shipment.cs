using System.ComponentModel.DataAnnotations;

namespace SportsStore.ShippingService.Models;

/// <summary>
/// Represents a shipment for an order
/// </summary>
public class Shipment
{
    [Key]
    public int ShipmentId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string Carrier { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Created"; // Created, Dispatched, InTransit, Delivered

    public DateTime EstimatedDispatchDate { get; set; }

    public DateTime? ActualDispatchDate { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string? ShippingAddress { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Available shipping carriers
/// </summary>
public class ShippingCarrier
{
    [Key]
    public int CarrierId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BaseCost { get; set; }

    public int EstimatedDays { get; set; }

    public bool IsActive { get; set; } = true;
}
