namespace HrManagement.Api.Modules.Activity.Domain.Services.Participant;

/// <summary>
/// Command service interface for Participant operations.
/// </summary>
public interface IParticipantCommandService
{
    /// <summary>
    /// Registers an employee to an activity.
    /// </summary>
    Task<Entities.Participant> RegisterParticipantAsync(string activityId, string employeeId);

    /// <summary>
    /// Removes a participant from an activity.
    /// </summary>
    Task RemoveParticipantAsync(string activityId, string employeeId);

    /// <summary>
    /// Updates participant status.
    /// </summary>
    Task<Entities.Participant> UpdateParticipantStatusAsync(string participantId, Entities.ActivityEnums.ParticipantStatus status);
}
