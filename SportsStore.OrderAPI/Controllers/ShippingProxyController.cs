using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/shipping")]
public class ShippingProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShippingProxyController> _logger;

    public ShippingProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ShippingProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("shipments")]
    public async Task<ActionResult> GetShipments([FromQuery] int? orderId, [FromQuery] string? status)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            
            var query = new List<string>();
            if (orderId.HasValue) query.Add($"orderId={orderId}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            
            var url = $"{shippingServiceUrl}/api/shipping/shipments";
            if (query.Any()) url += "?" + string.Join("&", query);
            
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy shipments request");
            return StatusCode(500, "Failed to load shipments");
        }
    }

    [HttpGet("shipments/{id}")]
    public async Task<ActionResult> GetShipment(int id)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{shippingServiceUrl}/api/shipping/shipments/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy shipment request");
            return StatusCode(500, "Failed to load shipment");
        }
    }

    [HttpGet("carriers")]
    public async Task<ActionResult> GetCarriers()
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{shippingServiceUrl}/api/shipping/carriers");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy carriers request");
            return StatusCode(500, "Failed to load carriers");
        }
    }

    [HttpGet("track/{trackingNumber}")]
    public async Task<ActionResult> Track(string trackingNumber)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{shippingServiceUrl}/api/shipping/track/{trackingNumber}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy track request");
            return StatusCode(500, "Failed to track shipment");
        }
    }

    [HttpPost("shipments/{shipmentId}/dispatch")]
    public async Task<ActionResult> Dispatch(int shipmentId)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{shippingServiceUrl}/api/shipping/shipments/{shipmentId}/dispatch", null);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy dispatch request");
            return StatusCode(500, "Failed to dispatch shipment");
        }
    }

    [HttpPost("shipments/{shipmentId}/deliver")]
    public async Task<ActionResult> Deliver(int shipmentId)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{shippingServiceUrl}/api/shipping/shipments/{shipmentId}/deliver", null);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy deliver request");
            return StatusCode(500, "Failed to deliver shipment");
        }
    }

    [HttpPost("shipments")]
    public async Task<ActionResult> CreateShipment([FromBody] object request)
    {
        try
        {
            var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{shippingServiceUrl}/api/shipping/shipments", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy create shipment request");
            return StatusCode(500, "Failed to create shipment");
        }
    }
}
