namespace HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;

/// <summary>
/// Command service interface for ActivityLog operations.
/// </summary>
public interface IActivityLogCommandService
{
    /// <summary>
    /// Submits a new activity log.
    /// </summary>
    Task<Entities.ActivityLog> SubmitLogAsync(Entities.ActivityLog log);

    /// <summary>
    /// Approves an activity log.
    /// Validates using the template's validator and calculates points.
    /// </summary>
    Task<Entities.ActivityLog> ApproveLogAsync(string logId, string reviewerId);

    /// <summary>
    /// Rejects an activity log.
    /// </summary>
    Task<Entities.ActivityLog> RejectLogAsync(string logId, string reviewerId, string reason);

    /// <summary>
    /// Deletes an activity log.
    /// </summary>
    Task DeleteLogAsync(string logId);
}
