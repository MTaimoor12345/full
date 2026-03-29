using System.ComponentModel.DataAnnotations;

namespace SportsStore.OrderAPI.Models;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Please enter a name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    // Address
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
