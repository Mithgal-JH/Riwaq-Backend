namespace Team3.Backend.Models;

public class PointsTransaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int Amount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public PointsTransactionType TransactionType { get; set; }

    public Guid? RelatedUserId { get; set; }

    public Guid? MentoringSessionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
