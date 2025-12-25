using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Activity.Application.DTOs;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Modules.Activity.Domain.Services.Activity;
using HrManagement.Api.Modules.Activity.Domain.Services.Participant;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.Controllers;

/// <summary>
/// API endpoints for managing activities.
/// </summary>
[ApiController]
[Route("api/v1/activities")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Activities")]
public class ActivityController : ControllerBase
{
    private readonly IActivityCommandService _activityCommandService;
    private readonly IActivityQueryService _activityQueryService;
    private readonly IParticipantCommandService _participantCommandService;

    public ActivityController(
        IActivityCommandService activityCommandService,
        IActivityQueryService activityQueryService,
        IParticipantCommandService participantCommandService)
    {
        _activityCommandService = activityCommandService;
        _activityQueryService = activityQueryService;
        _participantCommandService = participantCommandService;
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <remarks>
    /// Creates an activity with the specified template and configuration.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/activities
    ///     {
    ///         "name": "Marathon Challenge 2024",
    ///         "templateId": "RUNNING_SIMPLE",
    ///         "type": "RUNNING",
    ///         "description": "Run as much as you can!",
    ///         "startDate": "2024-01-01",
    ///         "endDate": "2024-03-31",
    ///         "config": {
    ///             "min_pace": 4,
    ///             "max_pace": 10,
    ///             "points_per_km": 10
    ///         }
    ///     }
    /// </remarks>
    /// <param name="request">The activity creation request.</param>
    /// <returns>The created activity with details.</returns>
    /// <response code="201">Returns the newly created activity.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ActivityDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
    {
        var activity = request.ToEntity();
        var createdActivity = await _activityCommandService.CreateActivityAsync(activity);
        var response = ActivityDetailResponse.FromEntity(createdActivity);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ActivityDetailResponse>.Created(response));
    }

    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the activity.</param>
    /// <returns>The activity with all details.</returns>
    /// <response code="200">Returns the activity details.</response>
    /// <response code="404">If the activity is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ActivityDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityById([FromRoute] string id)
    {
        var activity = await _activityQueryService.GetActivityByIdAsync(id)
            ?? throw new KeyNotFoundException($"Activity with ID {id} not found.");
        var response = ActivityDetailResponse.FromEntity(activity);
        return Ok(ApiResponse<ActivityDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Gets all activities with filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 10).</param>
    /// <param name="nameContains">Filter by name containing this text.</param>
    /// <param name="type">Filter by activity type.</param>
    /// <param name="status">Filter by status.</param>
    /// <param name="isActive">Filter by active/inactive status.</param>
    /// <returns>Paginated list of activities.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ActivityResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActivities(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameContains = null,
        [FromQuery] ActivityType? type = null,
        [FromQuery] ActivityStatus? status = null,
        [FromQuery] bool? isActive = null)
    {
        var filter = new ActivityFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            NameContains = nameContains,
            Type = type,
            Status = status,
            IsActive = isActive
        };

        var result = await _activityQueryService.GetActivitiesAsync(filter);

        var response = new PagedResult<ActivityResponse>(
            result.Items.Select(ActivityResponse.FromEntity).ToList(),
            result.TotalItems,
            result.Page,
            result.PageSize
        );

        return Ok(ApiResponse<PagedResult<ActivityResponse>>.Ok(response));
    }

    /// <summary>
    /// Gets activities that an employee has joined.
    /// </summary>
    /// <param name="employeeId">The employee ID.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 10).</param>
    /// <returns>Paginated list of joined activities.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ActivityResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyActivities(
        [FromQuery] string employeeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ActivityFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _activityQueryService.GetMyActivitiesAsync(employeeId, filter);

        var response = new PagedResult<ActivityResponse>(
            result.Items.Select(ActivityResponse.FromEntity).ToList(),
            result.TotalItems,
            result.Page,
            result.PageSize
        );

        return Ok(ApiResponse<PagedResult<ActivityResponse>>.Ok(response));
    }

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="id">The unique identifier of the activity.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated activity.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ActivityDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateActivity(
        [FromRoute] string id,
        [FromBody] UpdateActivityRequest request)
    {
        var activity = request.ToEntity(id);
        var updatedActivity = await _activityCommandService.UpdateActivityAsync(activity);
        var response = ActivityDetailResponse.FromEntity(updatedActivity);
        return Ok(ApiResponse<ActivityDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Deletes an activity.
    /// </summary>
    /// <param name="id">The unique identifier of the activity.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActivity([FromRoute] string id)
    {
        await _activityCommandService.DeleteActivityAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Updates the status of an activity.
    /// </summary>
    /// <param name="id">The unique identifier of the activity.</param>
    /// <param name="request">The status update request.</param>
    /// <returns>The updated activity.</returns>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<ActivityDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateActivityStatus(
        [FromRoute] string id,
        [FromBody] UpdateActivityStatusRequest request)
    {
        var updatedActivity = await _activityCommandService.UpdateActivityStatusAsync(id, request.Status);
        var response = ActivityDetailResponse.FromEntity(updatedActivity);
        return Ok(ApiResponse<ActivityDetailResponse>.Ok(response));
    }

    /// <summary>
    /// Registers an employee to an activity.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="employeeId">The employee ID to register.</param>
    /// <returns>The created participant.</returns>
    [HttpPost("{id}/register")]
    [ProducesResponseType(typeof(ApiResponse<ParticipantResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterParticipant(
        [FromRoute] string id,
        [FromQuery] string employeeId)
    {
        var participant = await _participantCommandService.RegisterParticipantAsync(id, employeeId);
        var response = ParticipantResponse.FromEntity(participant);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ParticipantResponse>.Created(response));
    }

    /// <summary>
    /// Unregisters an employee from an activity.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="employeeId">The employee ID to unregister.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}/unregister")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnregisterParticipant(
        [FromRoute] string id,
        [FromQuery] string employeeId)
    {
        await _participantCommandService.RemoveParticipantAsync(id, employeeId);
        return NoContent();
    }

    /// <summary>
    /// Gets the leaderboard for an activity.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="top">Number of top participants to return (default: 10).</param>
    /// <returns>List of top participants.</returns>
    [HttpGet("{id}/leaderboard")]
    [ProducesResponseType(typeof(ApiResponse<List<LeaderboardEntryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaderboard(
        [FromRoute] string id,
        [FromQuery] int top = 10)
    {
        var participants = await _activityQueryService.GetLeaderboardAsync(id, top);
        var response = participants
            .Select((p, index) => LeaderboardEntryResponse.FromEntity(p, index + 1))
            .ToList();
        return Ok(ApiResponse<List<LeaderboardEntryResponse>>.Ok(response));
    }

    /// <summary>
    /// Gets statistics for an activity (Admin/Manager).
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <returns>Activity statistics.</returns>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<ActivityStatisticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityStatistics([FromRoute] string id)
    {
        var statistics = await _activityQueryService.GetActivityStatisticsAsync(id);
        var response = ActivityStatisticsResponse.FromEntity(statistics);
        return Ok(ApiResponse<ActivityStatisticsResponse>.Ok(response));
    }
}
