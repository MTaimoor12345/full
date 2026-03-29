using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InventoryProxyController> _logger;

    public InventoryProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<InventoryProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{inventoryServiceUrl}/api/inventory");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy inventory request");
            return StatusCode(500, "Failed to load inventory");
        }
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult> GetById(long productId)
    {
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{inventoryServiceUrl}/api/inventory/{productId}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy inventory item request");
            return StatusCode(500, "Failed to load inventory item");
        }
    }

    [HttpGet("reservations")]
    public async Task<ActionResult> GetReservations([FromQuery] int? orderId)
    {
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var client = _httpClientFactory.CreateClient();
            var url = $"{inventoryServiceUrl}/api/inventory/reservations";
            if (orderId.HasValue) url += $"?orderId={orderId}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy reservations request");
            return StatusCode(500, "Failed to load reservations");
        }
    }

    [HttpPost("check-availability")]
    public async Task<ActionResult> CheckAvailability([FromBody] object request)
    {
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{inventoryServiceUrl}/api/inventory/check-availability", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy inventory check request");
            return StatusCode(500, "Failed to check inventory");
        }
    }

    [HttpPut("{productId}/stock")]
    public async Task<ActionResult> UpdateStock(long productId, [FromBody] object request)
    {
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{inventoryServiceUrl}/api/inventory/{productId}/stock", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy update stock request");
            return StatusCode(500, "Failed to update stock");
        }
    }
}
