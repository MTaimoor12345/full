using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace SportsStore.PaymentService.Controllers;

/// <summary>
/// Receives Stripe webhooks (e.g. card declined) and logs them so they appear in Seq.
/// </summary>
[Route("webhooks")]
[ApiController]
[IgnoreAntiforgeryToken]
public class StripeWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IConfiguration configuration, ILogger<StripeWebhookController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stripe webhook POST received at /webhooks/stripe.");

        bool skipVerification = _configuration.GetValue<bool>("Stripe:SkipWebhookSignatureVerification");
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (!skipVerification && string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogWarning("Stripe webhook received but Stripe:WebhookSecret is not configured. Skipping processing.");
            return BadRequest();
        }

        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        if (skipVerification)
        {
            HandleUnverifiedEvent(body);
            return Ok();
        }

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without Stripe-Signature header.");
            return BadRequest();
        }

        Event? stripeEvent = null;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(body, signature, webhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook event could not be parsed.");
            return Ok();
        }

        if (stripeEvent == null)
        {
            _logger.LogWarning("Stripe webhook event was null after parsing; nothing to log.");
            return Ok();
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.payment_failed":
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    var errorMessage = paymentIntent.LastPaymentError?.Message ?? "Unknown error";
                    var errorCode = paymentIntent.LastPaymentError?.Code ?? "unknown";
                    _logger.LogWarning(
                        "Stripe card declined / payment failed. PaymentIntentId: {PaymentIntentId}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, StripeEventId: {StripeEventId}",
                        paymentIntent.Id,
                        errorCode,
                        errorMessage,
                        stripeEvent.Id);
                }
                break;

            case "charge.failed":
                var charge = stripeEvent.Data.Object as Charge;
                if (charge != null)
                {
                    var failMsg = charge.FailureMessage ?? charge.FailureCode ?? "Card declined or charge failed";
                    _logger.LogWarning(
                        "Stripe charge failed (e.g. card declined). ChargeId: {ChargeId}, FailureCode: {FailureCode}, FailureMessage: {FailureMessage}, StripeEventId: {StripeEventId}",
                        charge.Id,
                        charge.FailureCode ?? "unknown",
                        failMsg,
                        stripeEvent.Id);
                }
                break;

            case "checkout.session.completed":
                var sessionCompleted = stripeEvent.Data.Object as Session;
                if (sessionCompleted != null)
                {
                    _logger.LogInformation(
                        "Stripe checkout session completed. SessionId: {StripeSessionId}, PaymentStatus: {PaymentStatus}, StripeEventId: {StripeEventId}",
                        sessionCompleted.Id,
                        sessionCompleted.PaymentStatus,
                        stripeEvent.Id);
                }
                break;

            case "checkout.session.expired":
                var sessionExpired = stripeEvent.Data.Object as Session;
                if (sessionExpired != null)
                {
                    _logger.LogInformation(
                        "Stripe checkout session expired. SessionId: {StripeSessionId}, StripeEventId: {StripeEventId}",
                        sessionExpired.Id,
                        stripeEvent.Id);
                }
                break;

            default:
                _logger.LogInformation("Stripe webhook event received (not payment_failed). Type: {EventType}, Id: {StripeEventId}", stripeEvent.Type, stripeEvent.Id);
                break;
        }

        return Ok();
    }

    private void HandleUnverifiedEvent(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp))
            {
                _logger.LogWarning("Stripe webhook (dev) payload missing 'type'.");
                return;
            }

            var eventType = typeProp.GetString() ?? string.Empty;
            if (!root.TryGetProperty("data", out var dataProp) ||
                !dataProp.TryGetProperty("object", out var objProp))
            {
                _logger.LogWarning("Stripe webhook (dev) payload missing 'data.object'. Type: {EventType}", eventType);
                return;
            }

            switch (eventType)
            {
                case "payment_intent.payment_failed":
                    var piId = objProp.TryGetProperty("id", out var piIdProp) ? piIdProp.GetString() : "unknown";
                    string errorCode = "unknown";
                    string errorMessage = "Unknown error";
                    if (objProp.TryGetProperty("last_payment_error", out var lastError))
                    {
                        if (lastError.TryGetProperty("code", out var codeProp))
                        {
                            errorCode = codeProp.GetString() ?? "unknown";
                        }
                        if (lastError.TryGetProperty("message", out var msgProp))
                        {
                            errorMessage = msgProp.GetString() ?? "Unknown error";
                        }
                    }

                    _logger.LogWarning(
                        "Stripe card declined / payment failed (dev). PaymentIntentId: {PaymentIntentId}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        piId,
                        errorCode,
                        errorMessage);
                    break;

                case "charge.failed":
                    var chargeId = objProp.TryGetProperty("id", out var chIdProp) ? chIdProp.GetString() : "unknown";
                    string failureCode = "unknown";
                    string failureMessage = "Card declined or charge failed";

                    if (objProp.TryGetProperty("failure_code", out var failCodeProp))
                    {
                        failureCode = failCodeProp.GetString() ?? "unknown";
                    }
                    if (objProp.TryGetProperty("failure_message", out var failMsgProp))
                    {
                        failureMessage = failMsgProp.GetString() ?? "Card declined or charge failed";
                    }

                    _logger.LogWarning(
                        "Stripe charge failed (dev, e.g. card declined). ChargeId: {ChargeId}, FailureCode: {FailureCode}, FailureMessage: {FailureMessage}",
                        chargeId,
                        failureCode,
                        failureMessage);
                    break;

                case "checkout.session.completed":
                    var sessionId = objProp.TryGetProperty("id", out var sessIdProp) ? sessIdProp.GetString() : "unknown";
                    var paymentStatus = objProp.TryGetProperty("payment_status", out var statusProp) ? statusProp.GetString() : "unknown";
                    _logger.LogInformation(
                        "Stripe checkout session completed (dev). SessionId: {StripeSessionId}, PaymentStatus: {PaymentStatus}",
                        sessionId,
                        paymentStatus);
                    break;

                case "checkout.session.expired":
                    var expiredSessionId = objProp.TryGetProperty("id", out var expSessIdProp) ? expSessIdProp.GetString() : "unknown";
                    _logger.LogInformation(
                        "Stripe checkout session expired (dev). SessionId: {StripeSessionId}",
                        expiredSessionId);
                    break;

                default:
                    _logger.LogInformation(
                        "Stripe webhook event (dev) received. Type: {EventType}",
                        eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook (dev) raw payload could not be parsed.");
        }
    }
}
