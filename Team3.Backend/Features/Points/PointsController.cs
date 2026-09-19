using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Points.Dtos;

namespace Team3.Backend.Features.Points;

[ApiController]
[Authorize]
[Route("api/points")]
public class PointsController : ControllerBase
{
    private readonly PointsService _pointsService;
    private readonly ICurrentUserService _currentUserService;

    public PointsController(
        PointsService pointsService,
        ICurrentUserService currentUserService)
    {
        _pointsService = pointsService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the authenticated user's current points balance.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PointsBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PointsBalanceResponse>> GetBalance()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetBalanceAsync(userId));
    }

    /// <summary>
    /// Returns the authenticated user's points transaction history.
    /// </summary>
    [HttpGet("me/transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<PointsTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PointsTransactionResponse>>>
        GetTransactions()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetTransactionsAsync(userId));
    }

    /// <summary>
    /// Returns the available points purchase packages.
    /// </summary>
    [HttpGet("packages")]
    [ProducesResponseType(typeof(IReadOnlyList<PointsPackageResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<PointsPackageResponse>> GetPackages()
    {
        return Ok(_pointsService.GetPackages());
    }

    /// <summary>
    /// Returns the authenticated user's points purchase history.
    /// </summary>
    [HttpGet("me/purchases")]
    [ProducesResponseType(typeof(IReadOnlyList<PointsPurchaseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PointsPurchaseResponse>>>
        GetPurchases()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetPurchasesAsync(userId));
    }

    /// <summary>
    /// Purchases a points package for the authenticated user.
    /// </summary>
    [HttpPost("purchases")]
    [ProducesResponseType(typeof(PointsPurchaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PointsPurchaseResponse>> Purchase(
        [FromBody] PurchasePointsRequest request)
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _pointsService.PurchaseAsync(
                userId,
                request.PackageId,
                request.IdempotencyKey));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
