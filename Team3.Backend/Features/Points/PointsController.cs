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

    [HttpGet("me")]
    public async Task<ActionResult<PointsBalanceResponse>> GetBalance()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetBalanceAsync(userId));
    }

    [HttpGet("me/transactions")]
    public async Task<ActionResult<IReadOnlyList<PointsTransactionResponse>>>
        GetTransactions()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetTransactionsAsync(userId));
    }

    [HttpGet("packages")]
    public ActionResult<IReadOnlyList<PointsPackageResponse>> GetPackages()
    {
        return Ok(_pointsService.GetPackages());
    }

    [HttpGet("me/purchases")]
    public async Task<ActionResult<IReadOnlyList<PointsPurchaseResponse>>>
        GetPurchases()
    {
        if (_currentUserService.UserId is not { } userId)
        {
            return Unauthorized();
        }

        return Ok(await _pointsService.GetPurchasesAsync(userId));
    }

    [HttpPost("purchases")]
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
