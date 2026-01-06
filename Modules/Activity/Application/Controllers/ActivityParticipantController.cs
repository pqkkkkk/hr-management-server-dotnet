using Microsoft.AspNetCore.Mvc;
using HrManagement.Api.Modules.Activity.Application.DTOs;
using HrManagement.Api.Modules.Activity.Domain.Services.Participant;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Application.Controllers;

/// <summary>
/// API endpoints for managing activity participants.
/// </summary>
[ApiController]
[Route("api/v1/activity-participants")]
[Produces("application/json")]
[Microsoft.AspNetCore.Http.Tags("Activity Participants")]
public class ActivityParticipantController : ControllerBase
{
    private readonly IParticipantQueryService _participantQueryService;

    public ActivityParticipantController(IParticipantQueryService participantQueryService)
    {
        _participantQueryService = participantQueryService;
    }

    /// <summary>
    /// Gets participant statistics by activity ID and employee ID.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="employeeId">The employee ID.</param>
    /// <returns>Participant statistics including total distance, submission counts, and score.</returns>
    /// <response code="200">Returns the participant statistics.</response>
    /// <response code="404">If the participant is not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ParticipantStatsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParticipantStats(
        [FromQuery] string activityId,
        [FromQuery] string employeeId)
    {
        var participant = await _participantQueryService.GetParticipantStatsAsync(activityId, employeeId)
            ?? throw new KeyNotFoundException($"Participant with employeeId {employeeId} in activity {activityId} not found.");

        var response = ParticipantStatsResponse.FromEntity(participant);
        return Ok(ApiResponse<ParticipantStatsResponse>.Ok(response));
    }
}
