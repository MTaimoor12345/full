using SportsStore.Blazor.DTOs;

namespace SportsStore.Blazor.Services;

public interface IProductService
{
    Task<PagedProductsDto> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null);
    Task<ProductDto?> GetProductAsync(long productId);
    Task<IEnumerable<string>> GetCategoriesAsync();
}

public interface ICartService
{
    Task<CartDto> GetCartAsync();
    Task<(bool Success, string? ErrorMessage)> AddToCartAsync(ProductDto product, int quantity = 1);
    Task RemoveFromCartAsync(long productId);
    Task<(bool Success, string? ErrorMessage)> UpdateQuantityAsync(long productId, int quantity);
    Task ClearCartAsync();
    event Action? OnCartChanged;
}

public interface IOrderService
{
    Task<CheckoutResultDto> CheckoutAsync(CheckoutDto checkout, string baseUrl);
    Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(string email);
    Task<OrderDto?> GetOrderAsync(int orderId);
}
