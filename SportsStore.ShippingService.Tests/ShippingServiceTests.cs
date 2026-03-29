using Microsoft.EntityFrameworkCore;
using SportsStore.ShippingService.Data;
using SportsStore.ShippingService.Models;
using Xunit;

namespace SportsStore.ShippingService.Tests;

public class ShippingServiceTests
{
    private ShippingDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ShippingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new ShippingDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void CanCreateShipment()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var shipment = new Shipment
        {
            OrderId = 1,
            CustomerId = 1,
            TrackingNumber = "TRK20260326123456",
            Carrier = "FedEx",
            Status = "Created",
            EstimatedDispatchDate = DateTime.UtcNow.AddDays(1),
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = Guid.NewGuid()
        };

        // Act
        context.Shipments.Add(shipment);
        context.SaveChanges();

        // Assert
        Assert.Single(context.Shipments);
        Assert.Equal("Created", context.Shipments.First().Status);
    }

    [Fact]
    public void CanUpdateShipmentStatus()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var shipment = new Shipment
        {
            OrderId = 1,
            CustomerId = 1,
            TrackingNumber = "TRK20260326123457",
            Carrier = "UPS",
            Status = "Created",
            EstimatedDispatchDate = DateTime.UtcNow.AddDays(1),
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = Guid.NewGuid()
        };
        context.Shipments.Add(shipment);
        context.SaveChanges();

        // Act
        shipment.Status = "Dispatched";
        shipment.ActualDispatchDate = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        var saved = context.Shipments.First();
        Assert.Equal("Dispatched", saved.Status);
        Assert.NotNull(saved.ActualDispatchDate);
    }

    [Fact]
    public void CanCreateCarrier()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var carrier = new ShippingCarrier
        {
            Name = "FedEx",
            Description = "Federal Express",
            BaseCost = 15.99m,
            EstimatedDays = 2,
            IsActive = true
        };

        // Act
        context.ShippingCarriers.Add(carrier);
        context.SaveChanges();

        // Assert
        Assert.Single(context.ShippingCarriers);
        Assert.Equal("FedEx", context.ShippingCarriers.First().Name);
    }

    [Fact]
    public void CanMarkAsDelivered()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var shipment = new Shipment
        {
            OrderId = 1,
            CustomerId = 1,
            TrackingNumber = "TRK20260326123458",
            Carrier = "DHL",
            Status = "InTransit",
            EstimatedDispatchDate = DateTime.UtcNow.AddDays(-2),
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1),
            CorrelationId = Guid.NewGuid()
        };
        context.Shipments.Add(shipment);
        context.SaveChanges();

        // Act
        shipment.Status = "Delivered";
        shipment.ActualDeliveryDate = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        var saved = context.Shipments.First();
        Assert.Equal("Delivered", saved.Status);
        Assert.NotNull(saved.ActualDeliveryDate);
    }
}
