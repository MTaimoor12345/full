using SportsStore.Blazor.DTOs;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace SportsStore.Blazor.Services;

public class CartService : ICartService
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private CartDto _cart = new();
    public event Action? OnCartChanged;

    public CartService(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public async Task<CartDto> GetCartAsync()
    {
        var cart = await _localStorage.GetItemAsync<CartDto>("cart");
        _cart = cart ?? new CartDto();
        return _cart;
    }

    public async Task<(bool Success, string? ErrorMessage)> AddToCartAsync(ProductDto product, int quantity = 1)
    {
        await GetCartAsync();

        // Check inventory availability
        try
        {
            var checkResponse = await _httpClient.PostAsJsonAsync("api/inventory/check-availability", new[]
            {
                new { ProductId = product.ProductId, ProductName = product.Name, Quantity = quantity }
            });

            if (checkResponse.IsSuccessStatusCode)
            {
                var result = await checkResponse.Content.ReadFromJsonAsync<InventoryCheckResult>();
                if (result != null && !result.AllAvailable)
                {
                    var unavailableItem = result.Items?.FirstOrDefault(i => !i.IsAvailable);
                    if (unavailableItem != null)
                    {
                        return (false, $"Insufficient stock for {unavailableItem.ProductName}. Available: {unavailableItem.AvailableQuantity}");
                    }
                    return (false, "Item is out of stock");
                }
            }
        }
        catch (Exception ex)
        {
            // If inventory check fails, allow adding to cart (fail open)
            Console.WriteLine($"Inventory check failed: {ex.Message}");
        }

        var existingItem = _cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _cart.Items.Add(new CartItemDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity
            });
        }

        await SaveCartAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateQuantityAsync(long productId, int quantity)
    {
        await GetCartAsync();
        var item = _cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                _cart.Items.Remove(item);
            }
            else
            {
                // Check inventory for new quantity
                try
                {
                    var checkResponse = await _httpClient.PostAsJsonAsync("api/inventory/check-availability", new[]
                    {
                        new { ProductId = item.ProductId, ProductName = item.ProductName, Quantity = quantity }
                    });

                    if (checkResponse.IsSuccessStatusCode)
                    {
                        var result = await checkResponse.Content.ReadFromJsonAsync<InventoryCheckResult>();
                        if (result != null && !result.AllAvailable)
                        {
                            var unavailableItem = result.Items?.FirstOrDefault(i => !i.IsAvailable);
                            if (unavailableItem != null)
                            {
                                return (false, $"Insufficient stock. Available: {unavailableItem.AvailableQuantity}");
                            }
                            return (false, "Item is out of stock");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Inventory check failed: {ex.Message}");
                }

                item.Quantity = quantity;
            }
        }
        await SaveCartAsync();
        return (true, null);
    }

    public async Task RemoveFromCartAsync(long productId)
    {
        await GetCartAsync();
        _cart.Items.RemoveAll(i => i.ProductId == productId);
        await SaveCartAsync();
    }

    public async Task ClearCartAsync()
    {
        _cart = new CartDto();
        await SaveCartAsync();
    }

    private async Task SaveCartAsync()
    {
        await _localStorage.SetItemAsync("cart", _cart);
        OnCartChanged?.Invoke();
    }
}

// Simple localStorage service interface
public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}

// Inventory Check Result DTOs
public class InventoryCheckResult
{
    public bool AllAvailable { get; set; }
    public List<InventoryItemCheckResult>? Items { get; set; }
}

public class InventoryItemCheckResult
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
}
