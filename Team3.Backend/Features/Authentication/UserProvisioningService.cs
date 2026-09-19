using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Authentication;

public class UserProvisioningService : IUserProvisioningService
{
    private const string DefaultRole = "User";

    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserProvisioningService(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task EnsureLocalUserAsync(ClaimsPrincipal principal)
    {
        var firebaseUid = principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new InvalidOperationException(
                "The Firebase token does not contain a subject claim.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(candidate =>
                candidate.FirebaseUid == firebaseUid);

        if (user is null)
        {
            user = await CreateOrLoadUserAsync(principal, firebaseUid);
            await EnsureInitialBalanceTransactionAsync(user);
        }

        await EnsureDefaultRoleAsync(user);
        await AddLocalClaimsAsync(principal, user);
    }

    private async Task EnsureInitialBalanceTransactionAsync(User user)
    {
        var existingTransaction = await _context.PointsTransactions
            .FirstOrDefaultAsync(transaction =>
                transaction.UserId == user.Id
                && transaction.TransactionType == PointsTransactionType.InitialBalance);

        if (existingTransaction is not null)
        {
            return;
        }

        var transaction = new PointsTransaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = 50,
            TransactionType = PointsTransactionType.InitialBalance,
            Reason = "Initial Points balance",
            CreatedAt = DateTime.UtcNow
        };

        _context.PointsTransactions.Add(transaction);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.Entry(transaction).State = EntityState.Detached;

            if (!await _context.PointsTransactions.AnyAsync(existing =>
                    existing.UserId == user.Id
                    && existing.TransactionType == PointsTransactionType.InitialBalance))
            {
                throw;
            }
        }
    }

    private async Task<User> CreateOrLoadUserAsync(
        ClaimsPrincipal principal,
        string firebaseUid)
    {
        var email = principal.FindFirstValue("email");
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            Email = email,
            UserName = email ?? firebaseUid,
            EmailConfirmed = string.Equals(
                principal.FindFirstValue("email_verified"),
                "true",
                StringComparison.OrdinalIgnoreCase)
        };

        try
        {
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                return user;
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(candidate =>
                    candidate.FirebaseUid == firebaseUid);

            if (existingUser is not null)
            {
                return existingUser;
            }

            var errors = string.Join(
                ", ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Failed to provision local user: {errors}");
        }
        catch (DbUpdateException)
        {
            _context.Entry(user).State = EntityState.Detached;

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(candidate =>
                    candidate.FirebaseUid == firebaseUid);

            if (existingUser is not null)
            {
                return existingUser;
            }

            throw;
        }
    }

    private async Task EnsureDefaultRoleAsync(User user)
    {
        var role = await _roleManager.FindByNameAsync(DefaultRole);

        if (role is null)
        {
            var result = await _roleManager.CreateAsync(
                new IdentityRole<Guid>(DefaultRole));

            if (!result.Succeeded)
            {
                role = await _roleManager.FindByNameAsync(DefaultRole);

                if (role is null)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Failed to provision default role: {errors}");
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count == 0)
        {
            var result = await _userManager.AddToRoleAsync(
                user,
                DefaultRole);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to assign default role: {errors}");
            }
        }
    }

    private async Task AddLocalClaimsAsync(
        ClaimsPrincipal principal,
        User user)
    {
        var identity = principal.Identities.FirstOrDefault(
            candidate => candidate.IsAuthenticated);

        if (identity is null)
        {
            throw new InvalidOperationException(
                "The authenticated principal has no authenticated identity.");
        }

        foreach (var claimsIdentity in principal.Identities)
        {
            foreach (var roleClaim in claimsIdentity.Claims
                         .Where(claim => claim.Type == ClaimTypes.Role
                             || claim.Type == "role")
                         .ToList())
            {
                claimsIdentity.RemoveClaim(roleClaim);
            }
        }

        identity.AddClaim(new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()));

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
    }
}
