using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;

namespace SportsStore.OrderAPI.Data;

public static class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product { Name = "Kayak", Description = "A boat for one person", Category = "Watersports", Price = 275.00m },
                new Product { Name = "Lifejacket", Description = "Protective and fashionable", Category = "Watersports", Price = 48.95m },
                new Product { Name = "Soccer Ball", Description = "FIFA-approved size and weight", Category = "Soccer", Price = 19.50m },
                new Product { Name = "Corner Flags", Description = "Give your playing field a professional touch", Category = "Soccer", Price = 34.95m },
                new Product { Name = "Stadium", Description = "Flat-packed 35,000-seat stadium", Category = "Soccer", Price = 79500.00m },
                new Product { Name = "Thinking Cap", Description = "Improve brain efficiency by 75%", Category = "Chess", Price = 16.00m },
                new Product { Name = "Unsteady Chair", Description = "Secretly give your opponent a disadvantage", Category = "Chess", Price = 29.95m },
                new Product { Name = "Human Chess Board", Description = "A fun game for the family", Category = "Chess", Price = 75.00m },
                new Product { Name = "Bling-Bling King", Description = "Gold-plated, diamond-studded King", Category = "Chess", Price = 1200.00m }
            );
            context.SaveChanges();
        }

        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer
                {
                    Name = "John Doe",
                    Email = "john@example.com",
                    Line1 = "123 Main St",
                    City = "New York",
                    State = "NY",
                    Zip = "10001",
                    Country = "USA"
                },
                new Customer
                {
                    Name = "Jane Smith",
                    Email = "jane@example.com",
                    Line1 = "456 Oak Ave",
                    City = "Los Angeles",
                    State = "CA",
                    Zip = "90001",
                    Country = "USA"
                }
            );
            context.SaveChanges();
        }
    }
}
