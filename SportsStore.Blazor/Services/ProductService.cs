using System.Net.Http.Json;
using SportsStore.Blazor.DTOs;

namespace SportsStore.Blazor.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedProductsDto> GetProductsAsync(int page = 1, int pageSize = 10, string? category = null)
    {
        var url = $"api/products?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(category))
        {
            url += $"&category={Uri.EscapeDataString(category)}";
        }

        var result = await _httpClient.GetFromJsonAsync<PagedProductsDto>(url);
        return result ?? new PagedProductsDto();
    }

    public async Task<ProductDto?> GetProductAsync(long productId)
    {
        return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{productId}");
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("api/products/categories");
        return result ?? Enumerable.Empty<string>();
    }
}
