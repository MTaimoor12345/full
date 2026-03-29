using Microsoft.AspNetCore.Mvc.Testing;
using SportsStore.OrderAPI;
using Xunit;
using System.Net.Http.Json;

namespace SportsStore.IntegrationTests;

public class OrderApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetOrders_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact(Skip = "Dashboard endpoint requires database seeding")]
    public async Task GetDashboard_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/dashboard");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetProductById_ReturnsProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact(Skip = "API returns paginated response, test needs update")]
    public async Task GetProducts_ReturnsNonEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        // Assert
        Assert.NotNull(products);
        Assert.NotEmpty(products!);
    }

    [Fact]
    public async Task GetCategories_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products/categories");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/99999");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "Checkout requires Stripe configuration and database setup")]
    public async Task Checkout_WithValidData_ReturnsCreated()
    {
        // Arrange
        var checkout = new CheckoutDto
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Zip = "12345",
            Country = "Test Country",
            Items = new List<CheckoutItemDto>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders/checkout", checkout);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(Skip = "Orders by status endpoint not implemented")]
    public async Task GetOrdersByStatus_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/status/Submitted");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/customer?email=test@example.com");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact(Skip = "Dashboard endpoint requires database seeding")]
    public async Task Dashboard_ReturnsValidMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/dashboard");
        response.EnsureSuccessStatusCode();
        
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();

        // Assert
        Assert.NotNull(dashboard);
        Assert.True(dashboard!.TotalOrders >= 0);
        Assert.True(dashboard.TotalRevenue >= 0);
    }
}

// DTOs for testing
public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CheckoutDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool GiftWrap { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new();
}

public class CheckoutItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
}
