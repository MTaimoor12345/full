using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsStore.PaymentService.Models;

/// <summary>
/// Represents a payment transaction
/// </summary>
public class PaymentTransaction
{
    [Key]
    public int TransactionId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "USD";

    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public string? TransactionReference { get; set; }

    public string? RejectionReason { get; set; }

    public string? ErrorCode { get; set; }

    public string? PaymentMethod { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Test card numbers for simulating payment scenarios
/// </summary>
public class TestCard
{
    [Key]
    public int CardId { get; set; }

    [Required]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    public string CardType { get; set; } = string.Empty; // Success, Decline, Error

    public string? Description { get; set; }
}
