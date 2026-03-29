using SportsStore.ShippingService.Data;
using SportsStore.ShippingService.Models;

namespace SportsStore.ShippingService.Data;

public static class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();

        if (!context.ShippingCarriers.Any())
        {
            context.ShippingCarriers.AddRange(
                new ShippingCarrier { Name = "FedEx", Description = "Federal Express", BaseCost = 15.99m, EstimatedDays = 2, IsActive = true },
                new ShippingCarrier { Name = "UPS", Description = "United Parcel Service", BaseCost = 14.99m, EstimatedDays = 3, IsActive = true },
                new ShippingCarrier { Name = "DHL", Description = "DHL Express", BaseCost = 19.99m, EstimatedDays = 2, IsActive = true },
                new ShippingCarrier { Name = "USPS", Description = "United States Postal Service", BaseCost = 9.99m, EstimatedDays = 5, IsActive = true }
            );
            context.SaveChanges();
        }
    }
}
