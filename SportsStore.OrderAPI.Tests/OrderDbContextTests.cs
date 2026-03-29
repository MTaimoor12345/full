using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.Enums;
using Xunit;

namespace SportsStore.OrderAPI.Tests;

public class OrderDbContextTests
{
    private OrderDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new OrderDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void CanCreateOrder()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        // Act
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country"
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Assert
        Assert.Single(context.Orders);
        Assert.Equal(OrderStatus.Submitted, context.Orders.First().Status);
    }

    [Fact]
    public void CanAddOrderItems()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        
        var product = new Product
        {
            ProductId = 1,
            Name = "Test Product",
            Description = "Test Description",
            Category = "Test",
            Price = 99.99m
        };
        context.Products.Add(product);
        context.SaveChanges();

        // Act
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country",
            Items = new List<OrderItem>
            {
                new() { ProductId = product.ProductId, ProductName = product.Name, Quantity = 2 }
            }
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Assert
        var savedOrder = context.Orders.Include(o => o.Items).First();
        Assert.Single(savedOrder.Items);
    }

    [Fact]
    public void CanUpdateOrderStatus()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country"
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Act
        order.Status = OrderStatus.InventoryPending;
        context.SaveChanges();

        // Assert
        Assert.Equal(OrderStatus.InventoryPending, context.Orders.First().Status);
    }

    [Fact]
    public void CanCreateCustomer()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john@example.com",
            Line1 = "123 Main St",
            Line2 = "Apt 4B",
            City = "New York",
            State = "NY",
            Zip = "10001",
            Country = "USA"
        };

        // Act
        context.Customers.Add(customer);
        context.SaveChanges();

        // Assert
        var saved = context.Customers.First();
        Assert.Equal("John Doe", saved.Name);
        Assert.Equal("john@example.com", saved.Email);
        Assert.Equal("New York", saved.City);
    }

    [Fact]
    public void CanCreateProduct()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var product = new Product
        {
            ProductId = 100,
            Name = "Soccer Ball",
            Description = "Professional soccer ball",
            Category = "Soccer",
            Price = 29.99m
        };

        // Act
        context.Products.Add(product);
        context.SaveChanges();

        // Assert
        var saved = context.Products.First();
        Assert.Equal("Soccer Ball", saved.Name);
        Assert.Equal(29.99m, saved.Price);
    }

    [Fact]
    public void OrderCalculatesTotalCorrectly()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        // Act
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country",
            Items = new List<OrderItem>
            {
                new() { ProductId = 1, ProductName = "Item 1", Quantity = 2, ProductPrice = 50m },
                new() { ProductId = 2, ProductName = "Item 2", Quantity = 1, ProductPrice = 100m }
            }
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Assert
        var savedOrder = context.Orders.Include(o => o.Items).First();
        Assert.Equal(200m, savedOrder.Items.Sum(i => i.Quantity * i.ProductPrice));
    }

    [Fact]
    public void CanFindCustomerByEmail()
    {
        // Arrange
        using var context = GetInMemoryContext();
        context.Customers.Add(new Customer
        {
            Name = "Test User",
            Email = "unique@example.com",
            Line1 = "123 Test St"
        });
        context.SaveChanges();

        // Act
        var found = context.Customers.FirstOrDefault(c => c.Email == "unique@example.com");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Test User", found.Name);
    }

    [Fact]
    public void CanGetOrdersByStatus()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        context.Orders.Add(new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country"
        });
        context.Orders.Add(new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country"
        });
        context.Orders.Add(new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country"
        });
        context.SaveChanges();

        // Act
        var submittedOrders = context.Orders.Where(o => o.Status == OrderStatus.Submitted).ToList();

        // Assert
        Assert.Equal(2, submittedOrders.Count);
    }

    [Fact]
    public void CanCreateOrderWithGiftWrap()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var customer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            Line1 = "123 Test St"
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        // Act
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            Line1 = "123 Test St",
            City = "Test City",
            State = "TS",
            Country = "Test Country",
            GiftWrap = true
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Assert
        var saved = context.Orders.First();
        Assert.True(saved.GiftWrap);
    }
}
