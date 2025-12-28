using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Reward.Application.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;

namespace HrManagement.Api.Modules.Reward.Application.Controllers;

/// <summary>
/// API endpoints for point transactions (gift, exchange, history).
/// </summary>
[ApiController]
[Route("api/v1/rewards/transactions")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Point Transactions")]
public class PointTransactionController : ControllerBase
{
    private readonly IPointTransactionCommandService _pointTransactionCommandService;
    private readonly IPointTransactionQueryService _pointTransactionQueryService;

    public PointTransactionController(
        IPointTransactionCommandService pointTransactionCommandService,
        IPointTransactionQueryService pointTransactionQueryService)
    {
        _pointTransactionCommandService = pointTransactionCommandService;
        _pointTransactionQueryService = pointTransactionQueryService;
    }
    /// <summary>
    /// Gets transaction history with filtering and pagination.
    /// </summary>
    /// <remarks>
    /// Fetches point transaction history with optional filters:
    /// - EmployeeId: Filter by a specific employee
    /// - FromDate: Filter transactions from this date
    /// - ToDate: Filter transactions up to this date
    /// - PageNumber: Page number (default: 1)
    /// - PageSize: Items per page (default: 10)
    /// 
    /// Each transaction includes its associated items (for EXCHANGE transactions).
    /// </remarks>
    /// <param name="filter">Filter criteria for transactions.</param>
    /// <returns>Paginated list of transactions with items.</returns>
    /// <response code="200">Returns the list of transactions.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PointTransactionDetailResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] PointTransactionFilter filter)
    {
        var result = await _pointTransactionQueryService.GetPointTransactionsAsync(filter);

        var dtoItems = result.Content.Select(item => PointTransactionDetailResponse.FromEntity(item)).ToList();

        var pagedResult = PagedResult<PointTransactionDetailResponse>.Create(
            dtoItems,
            result.TotalElements,
            result.Number + 1,
            result.Size
        );
        return Ok(ApiResponse<PagedResult<PointTransactionDetailResponse>>.Ok(pagedResult));
    }

    /// <summary>
    /// Gets a transaction by ID with full details.
    /// </summary>
    /// <remarks>
    /// Retrieves the full details of a transaction including items (for EXCHANGE transactions).
    /// </remarks>
    /// <param name="id">The transaction ID.</param>
    /// <returns>The transaction details.</returns>
    /// <response code="200">Returns the transaction details.</response>
    /// <response code="404">If the transaction is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PointTransactionDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById([FromRoute] string id)
    {
        var transaction = await _pointTransactionQueryService.GetPointTransactionByIdAsync(id);

        var response = transaction != null
            ? PointTransactionDetailResponse.FromEntity(transaction)
            : throw new KeyNotFoundException($"Point transaction with ID {id} not found.");

        return Ok(ApiResponse<PointTransactionDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Gifts points from manager to employees.
    /// </summary>
    /// <remarks>
    /// Manager can gift points from their giving budget to multiple employees.
    /// Each recipient can receive a different amount of points.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/rewards/transactions/gift
    ///     {
    ///         "programId": "program-123",
    ///         "senderUserId": "manager-456",
    ///         "recipients": [
    ///             { "userId": "employee-001", "points": 20 },
    ///             { "userId": "employee-002", "points": 30 }
    ///         ]
    ///     }
    /// 
    /// **Validation:**
    /// - Sender must have sufficient giving budget
    /// - All recipients must have valid wallets in the program
    /// </remarks>
    /// <param name="request">The gift points request.</param>
    /// <returns>List of created transactions.</returns>
    /// <response code="201">Points gifted successfully.</response>
    /// <response code="400">If validation fails (insufficient budget, invalid recipients, etc.).</response>
    [HttpPost("gift")]
    [ProducesResponseType(typeof(ApiResponse<List<PointTransactionResponse>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GiftPoints([FromBody] GiftPointsRequest request)
    {
        // TODO: Implement service call
        // var command = new GiftPointsCommand(request.ProgramId, request.SenderUserId, 
        //     request.Recipients.Select(r => new GiftRecipient(r.UserId, r.Points)).ToList());
        // var result = await _pointTransactionCommandService.GiftPointsToEmployeesAsync(command);

        // Placeholder response for Swagger documentation
        var giftCommand = request.ToCommand();

        var transactions = await _pointTransactionCommandService.GiftPointsToEmployeesAsync(giftCommand);

        var response = transactions.Select(PointTransactionResponse.FromEntity).ToList();

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<List<PointTransactionResponse>>.Created(response, "Points gifted successfully"));
    }

    /// <summary>
    /// Exchanges points for reward items.
    /// </summary>
    /// <remarks>
    /// Employee can exchange their personal points for reward items.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/rewards/transactions/exchange
    ///     {
    ///         "programId": "program-123",
    ///         "userId": "employee-456",
    ///         "items": [
    ///             { "rewardItemId": "item-001", "quantity": 1 },
    ///             { "rewardItemId": "item-002", "quantity": 2 }
    ///         ]
    ///     }
    /// 
    /// **Validation:**
    /// - User must have sufficient personal points
    /// - All items must be available in sufficient quantity
    /// - Program must be ACTIVE
    /// </remarks>
    /// <param name="request">The exchange points request.</param>
    /// <returns>The created transaction with item details.</returns>
    /// <response code="201">Points exchanged successfully.</response>
    /// <response code="400">If validation fails (insufficient points, insufficient item quantity, etc.).</response>
    [HttpPost("exchange")]
    [ProducesResponseType(typeof(ApiResponse<PointTransactionDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExchangePoints([FromBody] ExchangePointsRequest request)
    {
        // TODO: Implement service call
        // var result = await _pointTransactionCommandService.ExchangePointsAsync(transaction);

        // Placeholder response for Swagger documentation
        var transaction = request.ToEntity();

        var createdTransaction = await _pointTransactionCommandService.ExchangePointsAsync(transaction);

        var response = PointTransactionDetailResponse.FromEntity(createdTransaction);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<PointTransactionDetailResponse>.Created(response, "Points exchanged successfully"));
    }
}
