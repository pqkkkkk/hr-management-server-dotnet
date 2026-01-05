using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Activity.Application.DTOs;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.Controllers;

/// <summary>
/// API endpoints for managing activity logs.
/// </summary>
[ApiController]
[Route("api/v1/activity-logs")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Activity Logs")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogCommandService _activityLogCommandService;
    private readonly IActivityLogQueryService _activityLogQueryService;
    private readonly IParticipantDao _participantDao;

    public ActivityLogController(
        IActivityLogCommandService activityLogCommandService,
        IActivityLogQueryService activityLogQueryService,
        IParticipantDao participantDao)
    {
        _activityLogCommandService = activityLogCommandService;
        _activityLogQueryService = activityLogQueryService;
        _participantDao = participantDao;
    }

    /// <summary>
    /// Submits a new activity log.
    /// </summary>
    /// <remarks>
    /// Submits a log entry for an activity (e.g., running distance for a day).
    /// The log will be in PENDING status until approved by a manager.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/activity-logs
    ///     {
    ///         "activityId": "activity-guid",
    ///         "employeeId": "employee-guid",
    ///         "distance": 5.5,
    ///         "durationMinutes": 30,
    ///         "proofUrl": "https://example.com/screenshot.jpg",
    ///         "logDate": "2024-01-15"
    ///     }
    /// </remarks>
    /// <param name="request">The activity log submission request.</param>
    /// <returns>The created activity log.</returns>
    /// <response code="201">Returns the newly created activity log.</response>
    /// <response code="400">If the request is invalid or participant not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ActivityLogResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitLog([FromBody] SubmitActivityLogRequest request)
    {
        // Find participant by activity and employee
        var participant = await _participantDao.GetByActivityAndEmployeeAsync(request.ActivityId, request.EmployeeId)
            ?? throw new InvalidOperationException($"Employee {request.EmployeeId} is not a participant of activity {request.ActivityId}.");

        var log = request.ToEntity(participant.ParticipantId);
        var createdLog = await _activityLogCommandService.SubmitLogAsync(log);
        var response = ActivityLogResponse.FromEntity(createdLog);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ActivityLogResponse>.Created(response));
    }

    /// <summary>
    /// Gets an activity log by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the activity log.</param>
    /// <returns>The activity log details.</returns>
    /// <response code="200">Returns the activity log details.</response>
    /// <response code="404">If the activity log is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ActivityLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLogById([FromRoute] string id)
    {
        var log = await _activityLogQueryService.GetActivityLogByIdAsync(id)
            ?? throw new KeyNotFoundException($"Activity log with ID {id} not found.");
        var response = ActivityLogResponse.FromEntity(log);
        return Ok(ApiResponse<ActivityLogResponse>.Ok(response));
    }

    /// <summary>
    /// Gets all activity logs with filtering and pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 10).</param>
    /// <param name="activityId">Filter by activity ID.</param>
    /// <param name="employeeId">Filter by employee ID.</param>
    /// <param name="status">Filter by log status.</param>
    /// <param name="fromDate">Filter by log date (from).</param>
    /// <param name="toDate">Filter by log date (to).</param>
    /// <returns>Paginated list of activity logs.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ActivityLogResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? activityId = null,
        [FromQuery] string? employeeId = null,
        [FromQuery] ActivityLogStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var filter = new ActivityLogFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActivityId = activityId,
            EmployeeId = employeeId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _activityLogQueryService.GetActivityLogsAsync(filter);

        var response = PagedResult<ActivityLogResponse>.Create(
            result.Content.Select(ActivityLogResponse.FromEntity).ToList(),
            result.TotalElements,
            result.Number + 1,
            result.Size
        );

        return Ok(ApiResponse<PagedResult<ActivityLogResponse>>.Ok(response));
    }

    /// <summary>
    /// Deletes an activity log.
    /// </summary>
    /// <param name="id">The unique identifier of the activity log.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLog([FromRoute] string id)
    {
        await _activityLogCommandService.DeleteLogAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Approves an activity log (Manager).
    /// </summary>
    /// <remarks>
    /// Validates the log using the activity template's validator and calculates points.
    /// </remarks>
    /// <param name="id">The activity log ID.</param>
    /// <param name="reviewerId">The reviewer's employee ID.</param>
    /// <returns>The approved activity log.</returns>
    [HttpPatch("{id}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ActivityLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveLog(
        [FromRoute] string id,
        [FromQuery] string reviewerId)
    {
        var approvedLog = await _activityLogCommandService.ApproveLogAsync(id, reviewerId);
        var response = ActivityLogResponse.FromEntity(approvedLog);
        return Ok(ApiResponse<ActivityLogResponse>.Ok(response));
    }

    /// <summary>
    /// Rejects an activity log (Manager).
    /// </summary>
    /// <param name="id">The activity log ID.</param>
    /// <param name="reviewerId">The reviewer's employee ID.</param>
    /// <param name="request">The rejection request with reason.</param>
    /// <returns>The rejected activity log.</returns>
    [HttpPatch("{id}/reject")]
    [ProducesResponseType(typeof(ApiResponse<ActivityLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectLog(
        [FromRoute] string id,
        [FromQuery] string reviewerId,
        [FromBody] RejectActivityLogRequest request)
    {
        var rejectedLog = await _activityLogCommandService.RejectLogAsync(id, reviewerId, request.Reason);
        var response = ActivityLogResponse.FromEntity(rejectedLog);
        return Ok(ApiResponse<ActivityLogResponse>.Ok(response));
    }
}
