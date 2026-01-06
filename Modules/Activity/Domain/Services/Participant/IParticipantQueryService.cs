namespace HrManagement.Api.Modules.Activity.Domain.Services.Participant;

/// <summary>
/// Query service interface for Participant operations.
/// </summary>
public interface IParticipantQueryService
{
    /// <summary>
    /// Gets a participant's statistics by activity ID and employee ID.
    /// </summary>
    Task<Entities.Participant?> GetParticipantStatsAsync(string activityId, string employeeId);
}
