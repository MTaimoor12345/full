using SportsStore.InventoryService.Data;
using SportsStore.InventoryService.Models;

namespace SportsStore.InventoryService.Data;

public static class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        if (!context.InventoryItems.Any())
        {
            context.InventoryItems.AddRange(
                new InventoryItem { ProductId = 1, ProductName = "Kayak", Category = "Watersports", Price = 275.00m, StockQuantity = 10, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 2, ProductName = "Lifejacket", Category = "Watersports", Price = 48.95m, StockQuantity = 25, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 3, ProductName = "Soccer Ball", Category = "Soccer", Price = 19.50m, StockQuantity = 50, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 4, ProductName = "Corner Flags", Category = "Soccer", Price = 34.95m, StockQuantity = 30, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 5, ProductName = "Stadium", Category = "Soccer", Price = 79500.00m, StockQuantity = 2, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 6, ProductName = "Thinking Cap", Category = "Chess", Price = 16.00m, StockQuantity = 100, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 7, ProductName = "Unsteady Chair", Category = "Chess", Price = 29.95m, StockQuantity = 40, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 8, ProductName = "Human Chess Board", Category = "Chess", Price = 75.00m, StockQuantity = 15, ReservedQuantity = 0 },
                new InventoryItem { ProductId = 9, ProductName = "Bling-Bling King", Category = "Chess", Price = 1200.00m, StockQuantity = 5, ReservedQuantity = 0 }
            );
            context.SaveChanges();
        }
    }
}
