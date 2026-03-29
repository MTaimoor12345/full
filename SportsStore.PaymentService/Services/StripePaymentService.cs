using Stripe;
using Stripe.Checkout;

namespace SportsStore.PaymentService.Services;

/// <summary>
/// Stripe payment service using official SDK. Uses test keys only (from configuration).
/// </summary>
public class StripePaymentService : IStripePaymentService
{
    private readonly string? _secretKey;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _secretKey = configuration["Stripe:SecretKey"];
        _logger = logger;
    }

    private bool EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogWarning("Stripe SecretKey is not configured. Payment operations will not work.");
            return false;
        }

        StripeConfiguration.ApiKey = _secretKey;
        return true;
    }

    public async Task<string?> CreateCheckoutSessionAsync(
        long amountCents,
        string successUrl,
        string cancelUrl,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureApiKey())
        {
            return null;
        }

        try
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = amountCents,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Sports Store Order"
                            }
                        },
                        Quantity = 1
                    }
                }
            };

            if (metadata != null && metadata.Count > 0)
            {
                options.Metadata = new Dictionary<string, string>(metadata);
            }

            var service = new SessionService();
            Session session = await service.CreateAsync(options, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Created Stripe checkout session. SessionId: {SessionId}, Amount: {Amount}", 
                session.Id, amountCents);
            
            return session.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session. Amount: {Amount}", amountCents);
            return null;
        }
    }

    public async Task<bool> IsSessionPaidAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureApiKey())
        {
            return false;
        }

        try
        {
            var service = new SessionService();
            Session session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
            return session.PaymentStatus == "paid";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Stripe session payment status. SessionId: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<Session?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureApiKey())
        {
            return null;
        }

        try
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Stripe session. SessionId: {SessionId}", sessionId);
            return null;
        }
    }
}
