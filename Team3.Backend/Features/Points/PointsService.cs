using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Points.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Points;

public class PointsService
{
    public const int MentoringCost = 15;

    private static readonly IReadOnlyList<PointsPackageResponse> Packages =
    [
        new PointsPackageResponse
        {
            Id = "starter",
            Points = 100,
            Price = 5m
        },
        new PointsPackageResponse
        {
            Id = "standard",
            Points = 250,
            Price = 10m
        },
        new PointsPackageResponse
        {
            Id = "premium",
            Points = 500,
            Price = 18m
        }
    ];

    private readonly AppDbContext _context;

    public PointsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PointsBalanceResponse> GetBalanceAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        return new PointsBalanceResponse { Points = user.Points };
    }

    public async Task<IReadOnlyList<PointsTransactionResponse>>
        GetTransactionsAsync(Guid userId)
    {
        await EnsureUserExistsAsync(userId);

        return await _context.PointsTransactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Select(transaction => new PointsTransactionResponse
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType.ToString(),
                RelatedUserId = transaction.RelatedUserId,
                MentoringSessionId = transaction.MentoringSessionId,
                Reason = transaction.Reason,
                CreatedAt = transaction.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PointsPurchaseResponse>>
        GetPurchasesAsync(Guid userId)
    {
        await EnsureUserExistsAsync(userId);

        return await _context.PointsPurchases
            .AsNoTracking()
            .Where(purchase => purchase.UserId == userId)
            .OrderByDescending(purchase => purchase.CreatedAt)
            .Select(purchase => new PointsPurchaseResponse
            {
                Id = purchase.Id,
                PackageId = purchase.PackageId,
                Points = purchase.Points,
                Price = purchase.Price,
                Currency = purchase.Currency,
                PaymentMethod = purchase.PaymentMethod,
                Status = purchase.Status,
                CreatedAt = purchase.CreatedAt
            })
            .ToListAsync();
    }

    public IReadOnlyList<PointsPackageResponse> GetPackages() => Packages;

    public async Task<PointsPurchaseResponse> PurchaseAsync(
        Guid userId,
        string? packageId,
        string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.");
        }

        idempotencyKey = idempotencyKey.Trim();
        var user = await GetUserAsync(userId);
        var package = FindPackage(packageId);

        if (!_context.Database.IsRelational())
        {
            var existingPurchase = await FindPurchaseAsync(userId, idempotencyKey);
            if (existingPurchase is not null)
            {
                return ToPurchaseResponse(existingPurchase, user.Points);
            }

            user.Points += package.Points;
            return await SavePurchaseAsync(
                user,
                package,
                idempotencyKey,
                user.Points);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingPurchase = await FindPurchaseAsync(userId, idempotencyKey);
            if (existingPurchase is not null)
            {
                await transaction.CommitAsync();
                var existingBalance = await _context.Users
                    .Where(candidate => candidate.Id == userId)
                    .Select(candidate => candidate.Points)
                    .SingleAsync();
                return ToPurchaseResponse(existingPurchase, existingBalance);
            }

            var affectedRows = await _context.Users
                .Where(candidate => candidate.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.Points,
                        candidate => candidate.Points + package.Points));

            if (affectedRows != 1)
            {
                throw new KeyNotFoundException("User not found.");
            }

            _context.Entry(user).State = EntityState.Detached;
            var currentBalance = await _context.Users
                .Where(candidate => candidate.Id == userId)
                .Select(candidate => candidate.Points)
                .SingleAsync();
            var response = await SavePurchaseAsync(
                user,
                package,
                idempotencyKey,
                currentBalance);
            await transaction.CommitAsync();
            return response;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            var existingPurchase = await FindPurchaseAsync(userId, idempotencyKey);
            if (existingPurchase is not null)
            {
                var currentBalance = await _context.Users
                    .Where(candidate => candidate.Id == userId)
                    .Select(candidate => candidate.Points)
                    .SingleAsync();
                return ToPurchaseResponse(existingPurchase, currentBalance);
            }

            throw;
        }
    }

    private async Task<PointsPurchaseResponse> SavePurchaseAsync(
        User user,
        PointsPackageResponse package,
        string idempotencyKey,
        int balance)
    {
        var now = DateTime.UtcNow;
        var purchase = new PointsPurchase
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PackageId = package.Id,
            IdempotencyKey = idempotencyKey,
            Points = package.Points,
            Price = package.Price,
            Currency = package.Currency,
            PaymentMethod = "Simulated",
            Status = "Completed",
            Reference = $"sim-{Guid.NewGuid():N}",
            CreatedAt = now
        };

        _context.PointsPurchases.Add(purchase);
        _context.PointsTransactions.Add(new PointsTransaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = package.Points,
            TransactionType = PointsTransactionType.Purchase,
            Reason = $"Purchased {package.Points} Points",
            CreatedAt = now
        });

        await _context.SaveChangesAsync();

        return ToPurchaseResponse(purchase, balance);
    }

    public async Task CompleteMentoringSessionAsync(
        Guid studentId,
        Guid mentorId,
        Guid sessionId)
    {
        if (studentId == mentorId)
        {
            throw new InvalidOperationException(
                "A user cannot mentor themselves.");
        }

        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session ID must be a valid Guid.");
        }

        var student = await GetUserAsync(studentId);
        var mentor = await GetUserAsync(mentorId);
        var session = await _context.LearningSessions
            .Include(item => item.Connection)
            .FirstOrDefaultAsync(item => item.Id == sessionId);

        if (session is null)
        {
            throw new KeyNotFoundException("Learning session not found.");
        }

        if (!string.Equals(session.Status, "Completed",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only completed learning sessions can be paid.");
        }

        var participants = session.Connection;
        var connectionContainsParticipants =
            (participants.UserAId == studentId && participants.UserBId == mentorId)
            || (participants.UserAId == mentorId && participants.UserBId == studentId);

        if (!connectionContainsParticipants)
        {
            throw new InvalidOperationException(
                "The learning session does not belong to these users.");
        }

        var alreadyProcessed = await _context.PointsTransactions.AnyAsync(
            transaction => transaction.MentoringSessionId == sessionId);

        if (alreadyProcessed)
        {
            throw new InvalidOperationException(
                "This learning session has already been paid.");
        }

        if (_context.Database.IsRelational())
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                await TransferMentoringPointsAsync(
                    student,
                    mentor,
                    session,
                    studentId,
                    mentorId,
                    sessionId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        else
        {
            if (student.Points < MentoringCost)
            {
                throw new InvalidOperationException(
                    "The student does not have enough Points.");
            }

            student.Points -= MentoringCost;
            mentor.Points += MentoringCost;
            await AddMentoringTransactionsAsync(
                student,
                mentor,
                session,
                studentId,
                mentorId,
                sessionId);
            await _context.SaveChangesAsync();
        }
    }

    private async Task TransferMentoringPointsAsync(
        User student,
        User mentor,
        LearningSession session,
        Guid studentId,
        Guid mentorId,
        Guid sessionId)
    {
        var deducted = await _context.Users
            .Where(user => user.Id == studentId && user.Points >= MentoringCost)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.Points,
                    user => user.Points - MentoringCost));

        if (deducted != 1)
        {
            throw new InvalidOperationException(
                "The student does not have enough Points.");
        }

        var credited = await _context.Users
            .Where(user => user.Id == mentorId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.Points,
                    user => user.Points + MentoringCost));

        if (credited != 1)
        {
            throw new KeyNotFoundException("Mentor not found.");
        }

        await AddMentoringTransactionsAsync(
            student,
            mentor,
            session,
            studentId,
            mentorId,
            sessionId);
        await _context.SaveChangesAsync();
    }

    private Task AddMentoringTransactionsAsync(
        User student,
        User mentor,
        LearningSession session,
        Guid studentId,
        Guid mentorId,
        Guid sessionId)
    {
        var now = DateTime.UtcNow;
        _context.PointsTransactions.AddRange(
            new PointsTransaction
            {
                Id = Guid.NewGuid(),
                UserId = studentId,
                Amount = -MentoringCost,
                TransactionType = PointsTransactionType.MentoringPayment,
                RelatedUserId = mentorId,
                MentoringSessionId = sessionId,
                Reason = $"Learned {session.Title} from {mentor.UserName}",
                CreatedAt = now
            },
            new PointsTransaction
            {
                Id = Guid.NewGuid(),
                UserId = mentorId,
                Amount = MentoringCost,
                TransactionType = PointsTransactionType.MentoringEarning,
                RelatedUserId = studentId,
                MentoringSessionId = sessionId,
                Reason = $"Taught {session.Title} to {student.UserName}",
                CreatedAt = now
            });
        return Task.CompletedTask;
    }

    private async Task<User> GetUserAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        _ = await GetUserAsync(userId);
    }

    private static PointsPackageResponse FindPackage(string? packageId)
    {
        var package = Packages.FirstOrDefault(item =>
            string.Equals(item.Id, packageId, StringComparison.OrdinalIgnoreCase));

        return package ?? throw new ArgumentException("Invalid Points package.");
    }

    private Task<PointsPurchase?> FindPurchaseAsync(
        Guid userId,
        string idempotencyKey)
    {
        return _context.PointsPurchases
            .AsNoTracking()
            .FirstOrDefaultAsync(purchase =>
                purchase.UserId == userId
                && purchase.IdempotencyKey == idempotencyKey);
    }

    private static PointsPurchaseResponse ToPurchaseResponse(
        PointsPurchase purchase,
        int balance)
    {
        return new PointsPurchaseResponse
        {
            Id = purchase.Id,
            PackageId = purchase.PackageId,
            Points = purchase.Points,
            Price = purchase.Price,
            Currency = purchase.Currency,
            PaymentMethod = purchase.PaymentMethod,
            Status = purchase.Status,
            CreatedAt = purchase.CreatedAt,
            Balance = balance
        };
    }
}
