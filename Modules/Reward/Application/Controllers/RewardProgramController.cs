using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Reward.Application.DTOs;
using HrManagement.Api.Shared.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

namespace HrManagement.Api.Modules.Reward.Application.Controllers;

/// <summary>
/// API endpoints for managing reward programs.
/// </summary>
[ApiController]
[Route("api/v1/rewards/programs")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Reward Programs")]
public class RewardProgramController : ControllerBase
{
    private readonly IRewardProgramCommandService _rewardProgramCommandService;
    private readonly IRewardProgramQueryService _rewardProgramQueryService;

    public RewardProgramController(
        IRewardProgramCommandService rewardProgramCommandService,
        IRewardProgramQueryService rewardProgramQueryService)
    {
        _rewardProgramCommandService = rewardProgramCommandService;
        _rewardProgramQueryService = rewardProgramQueryService;
    }

    /// <summary>
    /// Creates a new reward program with items and policies.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a reward program along with its reward items and policies.
    /// It also automatically creates user wallets for all users in the system.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/rewards/programs
    ///     {
    ///         "name": "Q4 2024 Recognition",
    ///         "description": "Quarterly employee recognition program",
    ///         "startDate": "2024-10-01",
    ///         "endDate": "2024-12-31",
    ///         "defaultGivingBudget": 100,
    ///         "items": [
    ///             { "name": "Gift Card $50", "requiredPoints": 500, "quantity": 10 }
    ///         ],
    ///         "policies": [
    ///             { "policyType": "OVERTIME", "unitValue": 30, "pointsPerUnit": 5 }
    ///         ]
    ///     }
    /// </remarks>
    /// <param name="request">The reward program creation request.</param>
    /// <returns>The created reward program with details.</returns>
    /// <response code="201">Returns the newly created reward program.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RewardProgramDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRewardProgram([FromBody] CreateRewardProgramRequest request)
    {
        var rewardProgramInfo = request.ToEntity();

        var createdProgram = await _rewardProgramCommandService.CreateRewardProgramAsync(rewardProgramInfo);

        var response = RewardProgramDetailResponse.FromEntity(createdProgram);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<RewardProgramDetailResponse>.Created(response));
    }

    /// <summary>
    /// Gets a reward program by ID with its items and policies.
    /// </summary>
    /// <remarks>
    /// Retrieves the full details of a reward program including:
    /// - Program information (name, dates, status, etc.)
    /// - List of reward items available
    /// - List of policies for automatic point distribution
    /// </remarks>
    /// <param name="id">The unique identifier of the reward program.</param>
    /// <returns>The reward program with all details.</returns>
    /// <response code="200">Returns the reward program details.</response>
    /// <response code="404">If the reward program is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RewardProgramDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRewardProgramById([FromRoute] string id)
    {
        var rewardProgram = await _rewardProgramQueryService.GetRewardProgramByIdAsync(id);

        var response = RewardProgramDetailResponse.FromEntity(rewardProgram == null
            ? throw new KeyNotFoundException($"Reward program with ID {id} not found.")
            : rewardProgram);

        return Ok(ApiResponse<RewardProgramDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Gets all reward programs with filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 10).</param>
    /// <param name="nameContains">Filter by name containing this text.</param>
    /// <param name="isActive">Filter by active/inactive status.</param>
    /// <returns>Paginated list of reward programs.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RewardProgramResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRewardPrograms(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameContains = null,
        [FromQuery] bool? isActive = null)
    {
        var filter = new Domain.Filter.RewardProgramFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            NameContains = nameContains,
            IsActive = isActive
        };

        var result = await _rewardProgramQueryService.GetRewardProgramsAsync(filter);

        var response = PagedResult<RewardProgramResponse>.Create(
            result.Content.Select(RewardProgramResponse.FromEntity).ToList(),
            result.TotalElements,
            result.Number + 1,
            result.Size
        );

        return Ok(ApiResponse<PagedResult<RewardProgramResponse>>.Ok(response));
    }

    /// <summary>
    /// Updates an existing reward program.
    /// </summary>
    /// <remarks>
    /// If items or policies are provided, they will replace the existing ones.
    /// </remarks>
    /// <param name="id">The unique identifier of the reward program.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated reward program.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RewardProgramDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRewardProgram(
        [FromRoute] string id,
        [FromBody] UpdateRewardProgramRequest request)
    {
        // Map DTO to Entity (controller responsibility)
        var program = request.ToEntity(id);

        var updatedProgram = await _rewardProgramCommandService.UpdateRewardProgramAsync(program);
        var response = RewardProgramDetailResponse.FromEntity(updatedProgram);
        return Ok(ApiResponse<RewardProgramDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Deletes a reward program.
    /// </summary>
    /// <remarks>
    /// Cannot delete a program that has transactions. Use deactivate instead.
    /// </remarks>
    /// <param name="id">The unique identifier of the reward program.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRewardProgram([FromRoute] string id)
    {
        await _rewardProgramCommandService.DeleteRewardProgramAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a reward program.
    /// </summary>
    /// <remarks>
    /// Deactivating freezes all points - no more gifting or exchanging allowed.
    /// </remarks>
    /// <param name="id">The unique identifier of the reward program.</param>
    /// <returns>No content on success.</returns>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateRewardProgram([FromRoute] string id)
    {
        await _rewardProgramCommandService.DeactivateRewardProgramAsync(id);
        return NoContent();
    }
}
