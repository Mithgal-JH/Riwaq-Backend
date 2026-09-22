namespace Team3.Backend.Models;

public class PointsPurchase
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string PackageId { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public int Points { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public string PaymentMethod { get; set; } = "Simulated";

    public string Status { get; set; } = "Completed";

    public string Reference { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}