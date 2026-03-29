using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.PaymentService.Data;
using SportsStore.PaymentService.Models;
using SportsStore.PaymentService.Services;

namespace SportsStore.PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly IStripePaymentService _stripeService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        PaymentDbContext context,
        IStripePaymentService stripeService,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _stripeService = stripeService;
        _logger = logger;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] int? orderId = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("GetTransactions endpoint called - OrderId: {OrderId}, Status: {Status}", orderId, status);

        var query = _context.PaymentTransactions.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(t => t.OrderId == orderId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("transactions/{transactionId}")]
    public async Task<ActionResult> GetTransaction(int transactionId)
    {
        _logger.LogInformation("GetTransaction endpoint called - TransactionId: {TransactionId}", transactionId);

        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpGet("test-cards")]
    public async Task<ActionResult> GetTestCards()
    {
        _logger.LogInformation("GetTestCards endpoint called");

        var cards = await _context.TestCards.ToListAsync();
        return Ok(cards);
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "PaymentService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }

    // Stripe Checkout Endpoints

    [HttpPost("create-checkout-session")]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        _logger.LogInformation("CreateCheckoutSession endpoint called - OrderId: {OrderId}, Amount: {Amount}", 
            request.OrderId, request.Amount);

        long amountCents = (long)(request.Amount * 100);
        
        var metadata = new Dictionary<string, string>
        {
            { "orderId", request.OrderId.ToString() },
            { "customerEmail", request.CustomerEmail ?? "" }
        };

        string? checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(
            amountCents,
            request.SuccessUrl,
            request.CancelUrl,
            metadata);

        if (string.IsNullOrEmpty(checkoutUrl))
        {
            _logger.LogError("Failed to create Stripe checkout session. Check Stripe:SecretKey configuration.");
            return StatusCode(500, "Payment service is not configured. Please set Stripe keys.");
        }

        // Create a payment transaction record
        var transaction = new PaymentTransaction
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = "USD",
            Status = "Pending",
            PaymentMethod = "Stripe",
            TransactionReference = $"pending_{Guid.NewGuid()}",
            CreatedAt = DateTime.UtcNow
        };

        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created checkout session for OrderId: {OrderId}, TransactionId: {TransactionId}", 
            request.OrderId, transaction.TransactionId);

        return Ok(new CheckoutSessionResponse
        {
            CheckoutUrl = checkoutUrl,
            TransactionId = transaction.TransactionId
        });
    }

    [HttpGet("verify-session")]
    public async Task<ActionResult<PaymentVerificationResponse>> VerifySession([FromQuery] string sessionId)
    {
        _logger.LogInformation("VerifySession endpoint called - SessionId: {SessionId}", sessionId);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("Session ID is required");
        }

        bool isPaid = await _stripeService.IsSessionPaidAsync(sessionId);
        
        var session = await _stripeService.GetSessionAsync(sessionId);
        
        if (session == null)
        {
            return NotFound("Session not found");
        }

        // Update transaction status if paid
        if (isPaid && session.Metadata != null && session.Metadata.TryGetValue("orderId", out var orderIdStr))
        {
            if (int.TryParse(orderIdStr, out var orderId))
            {
                var transaction = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(t => t.OrderId == orderId && t.Status == "Pending");
                
                if (transaction != null)
                {
                    transaction.Status = "Completed";
                    transaction.TransactionReference = sessionId;
                    transaction.ProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Payment completed for OrderId: {OrderId}, SessionId: {SessionId}", 
                        orderId, sessionId);
                }
            }
        }

        return Ok(new PaymentVerificationResponse
        {
            IsPaid = isPaid,
            SessionId = sessionId,
            PaymentStatus = session.PaymentStatus,
            Amount = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : 0
        });
    }

    [HttpPost("cancel")]
    public async Task<ActionResult> CancelPayment([FromBody] CancelPaymentRequest request)
    {
        _logger.LogInformation("CancelPayment endpoint called - OrderId: {OrderId}", request.OrderId);

        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.OrderId == request.OrderId && t.Status == "Pending");

        if (transaction != null)
        {
            transaction.Status = "Cancelled";
            transaction.RejectionReason = request.Reason ?? "Cancelled by user";
            await _context.SaveChangesAsync();
        }

        return Ok(new { Success = true, Message = "Payment cancelled" });
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<PaymentTransaction>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        _logger.LogInformation("CreateTransaction endpoint called - OrderId: {OrderId}", request.OrderId);

        var transaction = new PaymentTransaction
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency ?? "USD",
            Status = request.Status ?? "Completed",
            PaymentMethod = request.PaymentMethod ?? "Stripe",
            TransactionReference = request.TransactionReference ?? $"txn_{Guid.NewGuid()}",
            ErrorCode = request.ErrorCode,
            RejectionReason = request.RejectionReason,
            CorrelationId = request.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = request.Status == "Completed" ? DateTime.UtcNow : null
        };

        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created payment transaction - TransactionId: {TransactionId}, OrderId: {OrderId}", 
            transaction.TransactionId, transaction.OrderId);

        return Ok(transaction);
    }
}

// Request/Response DTOs
public class CreateCheckoutSessionRequest
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class CheckoutSessionResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public int TransactionId { get; set; }
}

public class PaymentVerificationResponse
{
    public bool IsPaid { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CancelPaymentRequest
{
    public int OrderId { get; set; }
    public string? Reason { get; set; }
}

public class CreateTransactionRequest
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? ErrorCode { get; set; }
    public string? RejectionReason { get; set; }
    public Guid CorrelationId { get; set; }
}
