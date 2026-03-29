using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.InventoryService.Data;
using SportsStore.InventoryService.Models;

namespace SportsStore.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        InventoryDbContext context,
        ILogger<InventoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        _logger.LogInformation("GetAll inventory items endpoint called");

        var items = await _context.InventoryItems
            .OrderBy(i => i.ProductName)
            .ToListAsync();

        return Ok(items.Select(i => new
        {
            i.ProductId,
            i.ProductName,
            i.Category,
            i.Price,
            i.StockQuantity,
            i.ReservedQuantity,
            AvailableQuantity = i.AvailableQuantity,
            i.LastUpdated
        }));
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult> GetById(long productId)
    {
        _logger.LogInformation("GetById inventory endpoint called - ProductId: {ProductId}", productId);

        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (item == null)
            return NotFound();

        return Ok(new
        {
            item.ProductId,
            item.ProductName,
            item.Category,
            item.Price,
            item.StockQuantity,
            item.ReservedQuantity,
            AvailableQuantity = item.AvailableQuantity,
            item.LastUpdated
        });
    }

    [HttpGet("reservations")]
    public async Task<ActionResult> GetReservations([FromQuery] int? orderId = null)
    {
        _logger.LogInformation("GetReservations endpoint called - OrderId: {OrderId}", orderId);

        var query = _context.InventoryReservations.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(r => r.OrderId == orderId.Value);
        }

        var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(reservations);
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "InventoryService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("check-availability")]
    public async Task<ActionResult> CheckAvailability([FromBody] List<InventoryCheckRequest> items)
    {
        _logger.LogInformation("CheckAvailability endpoint called for {Count} items", items.Count);

        var results = new List<object>();
        var allAvailable = true;

        foreach (var item in items)
        {
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

            var available = inventoryItem != null && inventoryItem.AvailableQuantity >= item.Quantity;
            if (!available) allAvailable = false;

            results.Add(new
            {
                item.ProductId,
                item.ProductName,
                RequestedQuantity = item.Quantity,
                AvailableQuantity = inventoryItem?.AvailableQuantity ?? 0,
                IsAvailable = available
            });
        }

        return Ok(new
        {
            AllAvailable = allAvailable,
            Items = results
        });
    }

    [HttpPost("reserve")]
    public async Task<ActionResult> ReserveInventory([FromBody] ReserveInventoryRequest request)
    {
        _logger.LogInformation("ReserveInventory endpoint called - OrderId: {OrderId}", request.OrderId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reservations = new List<InventoryReservation>();
            var errors = new List<string>();

            foreach (var item in request.Items)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventoryItem == null || inventoryItem.AvailableQuantity < item.Quantity)
                {
                    errors.Add($"Insufficient stock for {item.ProductName}. Available: {inventoryItem?.AvailableQuantity ?? 0}, Requested: {item.Quantity}");
                    continue;
                }

                // Reserve the quantity
                inventoryItem.ReservedQuantity += item.Quantity;
                inventoryItem.LastUpdated = DateTime.UtcNow;

                var reservation = new InventoryReservation
                {
                    OrderId = request.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Status = "Reserved",
                    CorrelationId = request.CorrelationId
                };

                _context.InventoryReservations.Add(reservation);
                reservations.Add(reservation);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (errors.Any())
            {
                return BadRequest(new { Success = false, Errors = errors });
            }

            return Ok(new
            {
                Success = true,
                OrderId = request.OrderId,
                ReservedCount = reservations.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to reserve inventory for OrderId: {OrderId}", request.OrderId);
            return StatusCode(500, "Failed to reserve inventory");
        }
    }

    [HttpPut("{productId}/stock")]
    public async Task<ActionResult> UpdateStock(long productId, [FromBody] UpdateStockRequest request)
    {
        _logger.LogInformation("UpdateStock endpoint called - ProductId: {ProductId}, Quantity: {Quantity}", 
            productId, request.StockQuantity);

        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (item == null)
            return NotFound("Product not found in inventory");

        item.StockQuantity = request.StockQuantity;
        item.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            item.ProductId,
            item.ProductName,
            item.StockQuantity,
            item.ReservedQuantity,
            AvailableQuantity = item.AvailableQuantity,
            item.LastUpdated
        });
    }

    [HttpPost("confirm/{orderId}")]
    public async Task<ActionResult> ConfirmReservation(int orderId)
    {
        _logger.LogInformation("ConfirmReservation endpoint called - OrderId: {OrderId}", orderId);

        var reservations = await _context.InventoryReservations
            .Where(r => r.OrderId == orderId && r.Status == "Reserved")
            .ToListAsync();

        if (!reservations.Any())
        {
            return Ok(new { Success = true, Message = "No reservations found to confirm" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var reservation in reservations)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == reservation.ProductId);

                if (inventoryItem != null)
                {
                    // Reduce stock and clear reservation
                    inventoryItem.StockQuantity -= reservation.Quantity;
                    inventoryItem.ReservedQuantity -= reservation.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;
                }

                reservation.Status = "Confirmed";
                reservation.ConfirmedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory confirmed for OrderId: {OrderId}, Items: {Count}", 
                orderId, reservations.Count);

            return Ok(new { Success = true, ConfirmedCount = reservations.Count });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to confirm reservation for OrderId: {OrderId}", orderId);
            return StatusCode(500, "Failed to confirm reservation");
        }
    }

    [HttpPost("release/{orderId}")]
    public async Task<ActionResult> ReleaseReservation(int orderId)
    {
        _logger.LogInformation("ReleaseReservation endpoint called - OrderId: {OrderId}", orderId);

        var reservations = await _context.InventoryReservations
            .Where(r => r.OrderId == orderId && r.Status == "Reserved")
            .ToListAsync();

        if (!reservations.Any())
        {
            return Ok(new { Success = true, Message = "No reservations found to release" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var reservation in reservations)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == reservation.ProductId);

                if (inventoryItem != null)
                {
                    // Clear reservation (stock stays the same)
                    inventoryItem.ReservedQuantity -= reservation.Quantity;
                    inventoryItem.LastUpdated = DateTime.UtcNow;
                }

                reservation.Status = "Released";
                reservation.ReleasedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Inventory released for OrderId: {OrderId}, Items: {Count}", 
                orderId, reservations.Count);

            return Ok(new { Success = true, ReleasedCount = reservations.Count });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to release reservation for OrderId: {OrderId}", orderId);
            return StatusCode(500, "Failed to release reservation");
        }
    }
}

// Request DTOs
public class InventoryCheckRequest
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ReserveInventoryRequest
{
    public int OrderId { get; set; }
    public List<InventoryCheckRequest> Items { get; set; } = new();
    public Guid CorrelationId { get; set; }
}

public class UpdateStockRequest
{
    public int StockQuantity { get; set; }
}
