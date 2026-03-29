using AutoMapper;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;
using SportsStore.Shared.Messages;
using Stripe;
using Stripe.Checkout;
using System.Net.Http.Json;

namespace SportsStore.OrderAPI.Commands;

public record CheckoutOrderCommand(CheckoutDto Checkout, string BlazorBaseUrl) : IRequest<CheckoutResultDto>;

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, CheckoutResultDto>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckoutOrderCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpClientFactory _httpClientFactory;

    public CheckoutOrderCommandHandler(
        OrderDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<CheckoutOrderCommandHandler> logger,
        IPublishEndpoint publishEndpoint,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CheckoutResultDto> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var checkout = request.Checkout;
        var correlationId = Guid.NewGuid();

        _logger.LogInformation(
            "Processing checkout - Customer: {CustomerName}, Email: {CustomerEmail}, CorrelationId: {CorrelationId}",
            checkout.Name, checkout.Email, correlationId);

        // Check inventory availability first
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var httpClient = _httpClientFactory.CreateClient();
            var inventoryCheckRequest = checkout.CartItems.Select(item => new
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity
            }).ToList();

            var inventoryResponse = await httpClient.PostAsJsonAsync(
                $"{inventoryServiceUrl}/api/inventory/check-availability",
                inventoryCheckRequest,
                cancellationToken);

            if (inventoryResponse.IsSuccessStatusCode)
            {
                var inventoryResult = await inventoryResponse.Content.ReadFromJsonAsync<InventoryCheckResult>(cancellationToken);
                if (inventoryResult != null && !inventoryResult.AllAvailable)
                {
                    var unavailableItems = inventoryResult.Items?.Where(i => !i.IsAvailable).ToList();
                    var errorMessage = unavailableItems != null && unavailableItems.Any()
                        ? $"Insufficient stock for: {string.Join(", ", unavailableItems.Select(i => $"{i.ProductName} (Available: {i.AvailableQuantity}, Requested: {i.RequestedQuantity})"))}"
                        : "Some items are out of stock";

                    _logger.LogWarning("Checkout failed - Insufficient inventory: {ErrorMessage}", errorMessage);

                    return new CheckoutResultDto
                    {
                        OrderId = 0,
                        CustomerName = checkout.Name ?? "",
                        CustomerEmail = checkout.Email ?? "",
                        TotalAmount = 0,
                        Status = OrderStatus.Failed,
                        ErrorMessage = errorMessage,
                        RequiresPayment = false
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check inventory availability");
            // Continue with checkout - inventory service might be down
        }

        // Create or get customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == checkout.Email, cancellationToken);

        if (customer == null)
        {
            customer = new Models.Customer
            {
                Name = checkout.Name ?? "Unknown",
                Email = checkout.Email ?? "unknown@example.com",
                Phone = checkout.Phone,
                Line1 = checkout.Line1,
                Line2 = checkout.Line2,
                Line3 = checkout.Line3,
                City = checkout.City,
                State = checkout.State,
                Zip = checkout.Zip,
                Country = checkout.Country,
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new customer - CustomerId: {CustomerId}", customer.CustomerId);
        }

        // Calculate total
        decimal totalAmount = checkout.CartItems.Sum(item => item.ProductPrice * item.Quantity);

        // Create order with PaymentPending status
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.PaymentPending,
            TotalAmount = totalAmount,
            Line1 = checkout.Line1,
            Line2 = checkout.Line2,
            Line3 = checkout.Line3,
            City = checkout.City,
            State = checkout.State,
            Zip = checkout.Zip,
            Country = checkout.Country,
            GiftWrap = checkout.GiftWrap,
            CreatedAt = DateTime.UtcNow,
            Items = checkout.CartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductPrice = item.ProductPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order created with PaymentPending status - OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
            order.OrderId, order.CustomerId, order.TotalAmount);

        // Reserve inventory for this order
        try
        {
            var inventoryServiceUrl = _configuration["InventoryService:Url"] ?? "http://localhost:5139";
            var httpClient = _httpClientFactory.CreateClient();
            var reserveRequest = new
            {
                OrderId = order.OrderId,
                Items = checkout.CartItems.Select(item => new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity
                }).ToList(),
                CorrelationId = correlationId
            };

            var reserveResponse = await httpClient.PostAsJsonAsync(
                $"{inventoryServiceUrl}/api/inventory/reserve",
                reserveRequest,
                cancellationToken);

            if (reserveResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Inventory reserved for OrderId: {OrderId}", order.OrderId);
            }
            else
            {
                var errorContent = await reserveResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to reserve inventory for OrderId: {OrderId}, Error: {Error}", 
                    order.OrderId, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving inventory for OrderId: {OrderId}", order.OrderId);
            // Continue with checkout even if reservation fails
        }

        // Create Stripe checkout session
        var secretKey = _configuration["Stripe:SecretKey"];
        string? checkoutUrl = null;
        string? sessionId = null;

        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            StripeConfiguration.ApiKey = secretKey;
            
            try
            {
                long amountCents = (long)(totalAmount * 100);
                
                // Use Blazor frontend URLs for Stripe redirect
                var blazorUrl = request.BlazorBaseUrl.TrimEnd('/');
                var successUrl = $"{blazorUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}&order_id={order.OrderId}";
                var cancelUrl = $"{blazorUrl}/payment-cancel?order_id={order.OrderId}";
                
                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    LineItems = checkout.CartItems.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(item.ProductPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName
                            }
                        },
                        Quantity = item.Quantity
                    }).ToList(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", order.OrderId.ToString() },
                        { "customerEmail", customer.Email }
                    },
                    CustomerEmail = customer.Email
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);
                
                checkoutUrl = session.Url;
                sessionId = session.Id;
                order.StripeSessionId = sessionId;
                
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Created Stripe checkout session - OrderId: {OrderId}, SessionId: {SessionId}", 
                    order.OrderId, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Stripe checkout session for OrderId: {OrderId}", order.OrderId);
            }
        }
        else
        {
            _logger.LogWarning("Stripe SecretKey is not configured. Payment session not created.");
        }

        // Return result with payment URL
        return new CheckoutResultDto
        {
            OrderId = order.OrderId,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            TotalAmount = totalAmount,
            Status = order.Status,
            PaymentUrl = checkoutUrl,
            StripeSessionId = sessionId,
            RequiresPayment = !string.IsNullOrEmpty(checkoutUrl)
        };
    }
}
