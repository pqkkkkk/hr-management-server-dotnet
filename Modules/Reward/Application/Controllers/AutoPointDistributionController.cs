using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Reward.Application.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Services.AutoPointDistribution;

namespace HrManagement.Api.Modules.Reward.Application.Controllers;

/// <summary>
/// Controller for automatic point distribution operations.
/// Used for demo purposes to trigger point distribution manually.
/// In production, this would typically be handled by a scheduled job.
/// </summary>
[ApiController]
[Route("api/rewards/programs/{programId}/distribute-points")]
public class AutoPointDistributionController : ControllerBase
{
    private readonly IAutoPointDistributionService _distributionService;

    public AutoPointDistributionController(IAutoPointDistributionService distributionService)
    {
        _distributionService = distributionService;
    }

    /// <summary>
    /// Triggers automatic point distribution for a reward program based on timesheet data.
    /// Calculates points for all users according to active policies.
    /// </summary>
    /// <param name="programId">The reward program ID</param>
    /// <param name="request">The date range to evaluate</param>
    /// <returns>Distribution result summary</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AutoPointDistributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutoPointDistributionResponse>> DistributePoints(
        [FromRoute] string programId,
        [FromBody] DistributePointsRequest request)
    {
        if (request.StartDate > request.EndDate)
        {
            return BadRequest("Start date must be before or equal to end date.");
        }

        try
        {
            var result = await _distributionService.DistributePointsAsync(
                programId,
                request.StartDate,
                request.EndDate);

            // Map domain result to response DTO
            var response = new AutoPointDistributionResponse
            {
                TotalUsersProcessed = result.TotalUsersProcessed,
                TotalPointsDistributed = result.TotalPointsDistributed,
                TotalTransactionsCreated = result.TotalTransactionsCreated,
                UserSummaries = result.UserSummaries.Select(us => new UserPointSummaryResponse
                {
                    UserId = us.UserId,
                    UserName = us.UserName,
                    PointsEarned = us.PointsEarned,
                    PointsByPolicy = us.PointsByPolicy
                }).ToList()
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
