using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentProxyController> _logger;

    public PaymentProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PaymentProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions([FromQuery] int? orderId, [FromQuery] string? status)
    {
        try
        {
            var paymentServiceUrl = _configuration["PaymentService:Url"] ?? "http://localhost:5140";
            var client = _httpClientFactory.CreateClient();
            
            var query = new List<string>();
            if (orderId.HasValue) query.Add($"orderId={orderId}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            
            var url = $"{paymentServiceUrl}/api/payment/transactions";
            if (query.Any()) url += "?" + string.Join("&", query);
            
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy payment transactions request");
            return StatusCode(500, "Failed to load payment transactions");
        }
    }

    [HttpGet("transactions/{id}")]
    public async Task<ActionResult> GetTransaction(int id)
    {
        try
        {
            var paymentServiceUrl = _configuration["PaymentService:Url"] ?? "http://localhost:5140";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{paymentServiceUrl}/api/payment/transactions/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy payment transaction request");
            return StatusCode(500, "Failed to load payment transaction");
        }
    }

    [HttpGet("test-cards")]
    public async Task<ActionResult> GetTestCards()
    {
        try
        {
            var paymentServiceUrl = _configuration["PaymentService:Url"] ?? "http://localhost:5140";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{paymentServiceUrl}/api/payment/test-cards");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy test cards request");
            return StatusCode(500, "Failed to load test cards");
        }
    }

    [HttpPost("transactions")]
    public async Task<ActionResult> CreateTransaction([FromBody] object request)
    {
        try
        {
            var paymentServiceUrl = _configuration["PaymentService:Url"] ?? "http://localhost:5140";
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{paymentServiceUrl}/api/payment/transactions", request);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to proxy create payment transaction request");
            return StatusCode(500, "Failed to create payment transaction");
        }
    }
}
