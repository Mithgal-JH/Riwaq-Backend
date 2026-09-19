namespace Team3.Backend.Features.Points.Dtos;

public class PointsBalanceResponse
{
    public int Points { get; set; }
}

public class PointsTransactionResponse
{
    public Guid Id { get; set; }

    public int Amount { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public Guid? RelatedUserId { get; set; }

    public Guid? MentoringSessionId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class PointsPackageResponse
{
    public string Id { get; set; } = string.Empty;

    public int Points { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";
}

public class PurchasePointsRequest
{
    public string PackageId { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;
}

public class PointsPurchaseResponse
{
    public Guid Id { get; set; }

    public string PackageId { get; set; } = string.Empty;

    public int Points { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public string PaymentMethod { get; set; } = "Simulated";

    public string Status { get; set; } = "Completed";

    public DateTime CreatedAt { get; set; }

    public int Balance { get; set; }
}
