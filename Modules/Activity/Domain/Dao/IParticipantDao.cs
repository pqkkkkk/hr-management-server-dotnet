namespace HrManagement.Api.Modules.Activity.Domain.Dao;

/// <summary>
/// Data access interface for Participant entity.
/// </summary>
public interface IParticipantDao
{
    /// <summary>
    /// Gets a participant by ID with related entities.
    /// </summary>
    Task<Entities.Participant?> GetByIdAsync(string id);

    /// <summary>
    /// Gets a participant by activity ID and employee ID.
    /// </summary>
    Task<Entities.Participant?> GetByActivityAndEmployeeAsync(string activityId, string employeeId);

    /// <summary>
    /// Gets all participants for an activity.
    /// </summary>
    Task<IEnumerable<Entities.Participant>> GetByActivityIdAsync(string activityId);

    /// <summary>
    /// Gets all activities a specific employee has participated in.
    /// </summary>
    Task<IEnumerable<Entities.Participant>> GetByEmployeeIdAsync(string employeeId);

    /// <summary>
    /// Creates a new participant.
    /// </summary>
    Task<Entities.Participant> CreateAsync(Entities.Participant participant);

    /// <summary>
    /// Updates an existing participant.
    /// </summary>
    Task UpdateAsync(Entities.Participant participant);

    /// <summary>
    /// Deletes a participant by ID.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Gets leaderboard for an activity (top participants by score).
    /// </summary>
    Task<IEnumerable<Entities.Participant>> GetLeaderboardAsync(string activityId, int top = 10);

    /// <summary>
    /// Gets a participant by activity ID and employee ID with activity logs for statistics.
    /// </summary>
    Task<Entities.Participant?> GetByActivityAndEmployeeWithStatsAsync(string activityId, string employeeId);
}
