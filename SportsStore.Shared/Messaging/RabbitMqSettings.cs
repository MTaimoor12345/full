namespace SportsStore.Shared.Messaging;

public class RabbitMqSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public static class QueueNames
{
    // Order queues
    public const string OrderSubmitted = "order-submitted";
    public const string OrderCompleted = "order-completed";
    public const string OrderFailed = "order-failed";

    // Inventory queues
    public const string InventoryCheckRequested = "inventory-check-requested";
    public const string InventoryConfirmed = "inventory-confirmed";
    public const string InventoryFailed = "inventory-failed";

    // Payment queues
    public const string PaymentProcessingRequested = "payment-processing-requested";
    public const string PaymentApproved = "payment-approved";
    public const string PaymentRejected = "payment-rejected";

    // Shipping queues
    public const string ShippingRequested = "shipping-requested";
    public const string ShippingCreated = "shipping-created";
    public const string ShippingFailed = "shipping-failed";
}
