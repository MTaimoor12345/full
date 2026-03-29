using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.ShippingService.Data;

namespace SportsStore.ShippingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly ShippingDbContext _context;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(
        ShippingDbContext context,
        ILogger<ShippingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("shipments")]
    public async Task<ActionResult> GetShipments(
        [FromQuery] int? orderId = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("GetShipments endpoint called - OrderId: {OrderId}, Status: {Status}", orderId, status);

        var query = _context.Shipments.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(s => s.OrderId == orderId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var shipments = await query
            .OrderByDescending(s => s.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(shipments);
    }

    [HttpGet("shipments/{shipmentId}")]
    public async Task<ActionResult> GetShipment(int shipmentId)
    {
        _logger.LogInformation("GetShipment endpoint called - ShipmentId: {ShipmentId}", shipmentId);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }

    [HttpGet("carriers")]
    public async Task<ActionResult> GetCarriers()
    {
        _logger.LogInformation("GetCarriers endpoint called");

        var carriers = await _context.ShippingCarriers
            .Where(c => c.IsActive)
            .ToListAsync();

        return Ok(carriers);
    }

    [HttpGet("track/{trackingNumber}")]
    public async Task<ActionResult> TrackShipment(string trackingNumber)
    {
        _logger.LogInformation("TrackShipment endpoint called - TrackingNumber: {TrackingNumber}", trackingNumber);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

        if (shipment == null)
            return NotFound("Shipment not found");

        return Ok(new
        {
            shipment.TrackingNumber,
            shipment.Carrier,
            shipment.Status,
            shipment.EstimatedDispatchDate,
            shipment.EstimatedDeliveryDate,
            shipment.ActualDispatchDate,
            shipment.ActualDeliveryDate
        });
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "ShippingService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("shipments")]
    public async Task<ActionResult> CreateShipment([FromBody] CreateShipmentRequest request)
    {
        _logger.LogInformation("CreateShipment endpoint called - OrderId: {OrderId}", request.OrderId);

        // Check if shipment already exists for this order
        var existingShipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId);

        if (existingShipment != null)
        {
            return Ok(existingShipment);
        }

        var trackingNumber = $"TRK{DateTime.UtcNow:yyyyMMddHHmmss}{request.OrderId:D4}";

        var shipment = new Models.Shipment
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            TrackingNumber = trackingNumber,
            Carrier = request.Carrier ?? "Standard Shipping",
            Status = "Created",
            EstimatedDispatchDate = DateTime.UtcNow.AddDays(1),
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
            ShippingAddress = request.ShippingAddress,
            CorrelationId = request.CorrelationId
        };

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment created - ShipmentId: {ShipmentId}, OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
            shipment.ShipmentId, shipment.OrderId, shipment.TrackingNumber);

        return Ok(shipment);
    }

    [HttpPost("shipments/{shipmentId}/dispatch")]
    public async Task<ActionResult> DispatchShipment(int shipmentId)
    {
        _logger.LogInformation("DispatchShipment endpoint called - ShipmentId: {ShipmentId}", shipmentId);

        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null)
            return NotFound("Shipment not found");

        if (shipment.Status != "Created")
            return BadRequest($"Cannot dispatch shipment with status: {shipment.Status}");

        shipment.Status = "Dispatched";
        shipment.ActualDispatchDate = DateTime.UtcNow;
        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment dispatched - ShipmentId: {ShipmentId}", shipmentId);

        return Ok(shipment);
    }

    [HttpPost("shipments/{shipmentId}/deliver")]
    public async Task<ActionResult> DeliverShipment(int shipmentId)
    {
        _logger.LogInformation("DeliverShipment endpoint called - ShipmentId: {ShipmentId}", shipmentId);

        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null)
            return NotFound("Shipment not found");

        if (shipment.Status != "Dispatched")
            return BadRequest($"Cannot deliver shipment with status: {shipment.Status}");

        shipment.Status = "Delivered";
        shipment.ActualDeliveryDate = DateTime.UtcNow;
        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Shipment delivered - ShipmentId: {ShipmentId}", shipmentId);

        return Ok(shipment);
    }
}

// Request DTOs
public class CreateShipmentRequest
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string? Carrier { get; set; }
    public string? ShippingAddress { get; set; }
    public Guid CorrelationId { get; set; }
}
