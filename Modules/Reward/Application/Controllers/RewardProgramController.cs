using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Reward.Application.DTOs;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Application.Controllers;

/// <summary>
/// API endpoints for managing reward programs.
/// </summary>
[ApiController]
[Route("api/rewards/programs")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Reward Programs")]
public class RewardProgramController : ControllerBase
{
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
        // TODO: Implement service call
        // var result = await _rewardProgramCommandService.CreateRewardProgramAsync(program);

        // Placeholder response for Swagger documentation
        var response = new RewardProgramDetailResponse
        {
            RewardProgramId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = Domain.Entities.RewardEnums.ProgramStatus.ACTIVE,
            DefaultGivingBudget = request.DefaultGivingBudget,
            BannerUrl = request.BannerUrl,
            Items = request.Items.Select(i => new RewardItemResponse
            {
                RewardItemId = Guid.NewGuid().ToString(),
                ProgramId = "placeholder",
                Name = i.Name,
                RequiredPoints = i.RequiredPoints,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList(),
            Policies = request.Policies.Select(p => new RewardPolicyResponse
            {
                PolicyId = Guid.NewGuid().ToString(),
                ProgramId = "placeholder",
                PolicyType = p.PolicyType,
                CalculationPeriod = Domain.Entities.RewardEnums.CalculationPeriod.WEEKLY,
                UnitValue = p.UnitValue,
                PointsPerUnit = p.PointsPerUnit,
                IsActive = true
            }).ToList()
        };

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
        // TODO: Implement service call
        // var result = await _rewardProgramQueryService.GetRewardProgramByIdAsync(Guid.Parse(id));

        // Placeholder response for Swagger documentation
        var response = new RewardProgramDetailResponse
        {
            RewardProgramId = id,
            Name = "Sample Reward Program",
            Description = "A sample reward program for demonstration",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            Status = Domain.Entities.RewardEnums.ProgramStatus.ACTIVE,
            DefaultGivingBudget = 100,
            BannerUrl = null,
            Items = new List<RewardItemResponse>
            {
                new RewardItemResponse
                {
                    RewardItemId = Guid.NewGuid().ToString(),
                    ProgramId = id,
                    Name = "Sample Gift Card",
                    RequiredPoints = 500,
                    Quantity = 10,
                    ImageUrl = null
                }
            },
            Policies = new List<RewardPolicyResponse>
            {
                new RewardPolicyResponse
                {
                    PolicyId = Guid.NewGuid().ToString(),
                    ProgramId = id,
                    PolicyType = Domain.Entities.RewardEnums.PolicyType.OVERTIME,
                    CalculationPeriod = Domain.Entities.RewardEnums.CalculationPeriod.WEEKLY,
                    UnitValue = 30,
                    PointsPerUnit = 5,
                    IsActive = true
                }
            }
        };

        return Ok(ApiResponse<RewardProgramDetailResponse>.Ok(response));
    }
}
