using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportsStore.OrderAPI.Commands;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Queries;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;
using Stripe.Checkout;
using Stripe;
using System.Net.Http.Json;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly OrderDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrdersController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrdersController(
        IMediator mediator,
        OrderDbContext context,
        IConfiguration configuration,
        ILogger<OrdersController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResultDto>> Checkout([FromBody] CheckoutRequest request)
    {
        _logger.LogInformation("Checkout endpoint called - Customer: {CustomerName}", request.Checkout.Name);

        try
        {
            // Use Blazor frontend URL for Stripe redirects
            var blazorBaseUrl = request.BlazorBaseUrl ?? $"{Request.Scheme}://{Request.Host}";

            var command = new CheckoutOrderCommand(request.Checkout, blazorBaseUrl);
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checkout failed");
            return StatusCode(500, "An error occurred during checkout");
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("GetAll orders endpoint called - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetOrdersQuery(page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        _logger.LogInformation("GetById endpoint called - OrderId: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<OrderStatusDto>> GetStatus(int id)
    {
        _logger.LogInformation("GetStatus endpoint called - OrderId: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);

        if (order == null)
            return NotFound();

        var statusDto = new OrderStatusDto
        {
            OrderId = order.OrderId,
            Status = order.Status,
            StatusMessage = GetStatusMessage(order.Status),
            LastUpdated = order.UpdatedAt ?? order.CreatedAt,
            Events = new List<OrderEventDto>()
        };

        return Ok(statusDto);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<OrderDto>>> GetByStatus(OrderStatus status)
    {
        _logger.LogInformation("GetByStatus endpoint called - Status: {Status}", status);

        var query = new GetOrdersByStatusQuery(status);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<bool>> Cancel(int id, [FromBody] string reason)
    {
        _logger.LogInformation("Cancel endpoint called - OrderId: {OrderId}", id);

        var command = new CancelOrderCommand(id, reason ?? "Cancelled by user");
        var result = await _mediator.Send(command);

        if (!result)
            return BadRequest("Unable to cancel order");

        return Ok(result);
    }

    [HttpGet("customers/{customerId}/orders")]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(int customerId)
    {
        _logger.LogInformation("GetCustomerOrders endpoint called - CustomerId: {CustomerId}", customerId);

        var query = new GetCustomerOrdersQuery(customerId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("customer")]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersByEmail([FromQuery] string email)
    {
        _logger.LogInformation("GetOrdersByEmail endpoint called - Email: {Email}", email);

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        var query = new GetOrdersByEmailQuery(email);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        _logger.LogInformation("GetDashboardSummary endpoint called");

        var query = new GetDashboardSummaryQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // Stripe Payment Endpoints

    [HttpPost("{id}/create-payment-session")]
    public async Task<ActionResult<PaymentSessionResponse>> CreatePaymentSession(int id, [FromBody] PaymentSessionRequest request)
    {
        _logger.LogInformation("CreatePaymentSession endpoint called - OrderId: {OrderId}", id);

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound("Order not found");

        var secretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogError("Stripe SecretKey is not configured");
            return StatusCode(500, "Payment service is not configured");
        }

        StripeConfiguration.ApiKey = secretKey;

        try
        {
            long amountCents = (long)(order.TotalAmount * 100);
            
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = amountCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{order.OrderId}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.OrderId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // Update order with Stripe session ID
            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created Stripe checkout session for OrderId: {OrderId}, SessionId: {SessionId}", 
                order.OrderId, session.Id);

            return Ok(new PaymentSessionResponse
            {
                CheckoutUrl = session.Url,
                SessionId = session.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session for OrderId: {OrderId}", id);
            return StatusCode(500, "Failed to create payment session");
        }
    }

    [HttpGet("payment-success")]
    public async Task<ActionResult<PaymentVerificationResponse>> PaymentSuccess([FromQuery] string session_id, [FromQuery] int? order_id)
    {
        _logger.LogInformation("PaymentSuccess endpoint called - SessionId: {SessionId}, OrderId: {OrderId}", session_id, order_id);

        if (string.IsNullOrWhiteSpace(session_id))
            return BadRequest("Session ID is required");

        var secretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
            return StatusCode(500, "Payment service is not configured");

        StripeConfiguration.ApiKey = secretKey;

        try
        {
            var service = new SessionService();
            Session session = await service.GetAsync(session_id);

            bool isPaid = session.PaymentStatus == "paid";

            if (isPaid)
            {
                // Get order ID from metadata or query parameter
                int? orderId = order_id;
                if (!orderId.HasValue && session.Metadata != null && session.Metadata.TryGetValue("orderId", out var orderIdStr))
                {
                    int.TryParse(orderIdStr, out var parsedOrderId);
                    orderId = parsedOrderId;
                }

                if (orderId.HasValue)
                {
                    var order = await _context.Orders.FindAsync(orderId.Value);
                    if (order != null)
                    {
                        order.Status = OrderStatus.PaymentApproved;
                        order.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Payment completed for OrderId: {OrderId}, SessionId: {SessionId}", 
                            orderId, session_id);

                        // Get service URLs from configuration
                        var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
                        var paymentServiceUrl = _configuration["PaymentService:Url"] ?? "http://localhost:5140";
                        var shippingServiceUrl = _configuration["ShippingService:Url"] ?? "http://localhost:5141";

                        // Confirm inventory reservation (reduce stock)
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            await client.PostAsync($"{inventoryServiceUrl}/api/inventory/confirm/{order.OrderId}", null);
                            _logger.LogInformation("Inventory confirmed for OrderId: {OrderId}", order.OrderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to confirm inventory for OrderId: {OrderId}", order.OrderId);
                        }

                        // Record payment transaction in PaymentService
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            var paymentRequest = new
                            {
                                OrderId = order.OrderId,
                                CustomerId = order.CustomerId,
                                Amount = order.TotalAmount,
                                Currency = "USD",
                                Status = "Completed",
                                PaymentMethod = "Stripe",
                                TransactionReference = session_id,
                                CorrelationId = Guid.NewGuid()
                            };
                            
                            await client.PostAsJsonAsync($"{paymentServiceUrl}/api/payment/transactions", paymentRequest);
                            _logger.LogInformation("Recorded payment transaction for OrderId: {OrderId}", order.OrderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to record payment transaction for OrderId: {OrderId}", order.OrderId);
                        }

                        // Create shipment in ShippingService
                        try
                        {
                            var client = _httpClientFactory.CreateClient();
                            var shippingAddress = $"{order.Line1 ?? ""}, {order.Line2 ?? ""}, {order.City ?? ""}, {order.State ?? ""} {order.Zip ?? ""}, {order.Country ?? ""}";
                            var shipmentRequest = new
                            {
                                OrderId = order.OrderId,
                                CustomerId = order.CustomerId,
                                Carrier = "Standard Shipping",
                                ShippingAddress = shippingAddress,
                                CorrelationId = Guid.NewGuid()
                            };

                            await client.PostAsJsonAsync($"{shippingServiceUrl}/api/shipping/shipments", shipmentRequest);
                            _logger.LogInformation("Created shipment for OrderId: {OrderId}", order.OrderId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to create shipment for OrderId: {OrderId}", order.OrderId);
                        }

                        // Publish OrderSubmitted event after successful payment
                        var orderSubmittedEvent = new SportsStore.Shared.Messages.OrderSubmittedEvent
                        {
                            OrderId = order.OrderId,
                            CustomerId = order.CustomerId,
                            TotalAmount = order.TotalAmount,
                            Timestamp = DateTime.UtcNow,
                            CorrelationId = Guid.NewGuid()
                        };
                        // Note: In a real implementation, you'd inject IPublishEndpoint and publish this event
                    }
                }
            }

            return Ok(new PaymentVerificationResponse
            {
                IsPaid = isPaid,
                SessionId = session_id,
                PaymentStatus = session.PaymentStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify payment for SessionId: {SessionId}", session_id);
            return StatusCode(500, "Failed to verify payment");
        }
    }

    [HttpGet("payment-cancel")]
    public async Task<ActionResult> PaymentCancel([FromQuery] int orderId)
    {
        _logger.LogInformation("PaymentCancel endpoint called - OrderId: {OrderId}", orderId);

        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Release inventory reservation
            try
            {
                var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
                var client = _httpClientFactory.CreateClient();
                await client.PostAsync($"{inventoryServiceUrl}/api/inventory/release/{orderId}", null);
                _logger.LogInformation("Inventory released for cancelled OrderId: {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release inventory for cancelled OrderId: {OrderId}", orderId);
            }
        }

        return Ok(new { Success = true, Message = "Payment cancelled" });
    }

    private static string GetStatusMessage(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Cart => "Order is in cart",
            OrderStatus.Submitted => "Order has been submitted",
            OrderStatus.InventoryPending => "Checking inventory availability",
            OrderStatus.InventoryConfirmed => "Inventory confirmed",
            OrderStatus.InventoryFailed => "Inventory check failed",
            OrderStatus.PaymentPending => "Processing payment",
            OrderStatus.PaymentApproved => "Payment approved",
            OrderStatus.PaymentFailed => "Payment failed",
            OrderStatus.ShippingPending => "Preparing shipment",
            OrderStatus.ShippingCreated => "Shipment created",
            OrderStatus.Completed => "Order completed",
            OrderStatus.Failed => "Order failed",
            _ => "Unknown status"
        };
    }
}
