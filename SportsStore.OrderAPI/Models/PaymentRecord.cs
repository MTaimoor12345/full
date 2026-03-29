using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsStore.OrderAPI.Models;

public class PaymentRecord
{
    [Key]
    public int PaymentId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    [Required]
    public string Status { get; set; } = "Pending";

    public string? TransactionReference { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
