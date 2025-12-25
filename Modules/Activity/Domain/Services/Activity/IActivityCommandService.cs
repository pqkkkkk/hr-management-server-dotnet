namespace HrManagement.Api.Modules.Activity.Domain.Services.Activity;

/// <summary>
/// Command service interface for Activity operations.
/// </summary>
public interface IActivityCommandService
{
    /// <summary>
    /// Creates a new activity.
    /// </summary>
    Task<Entities.Activity> CreateActivityAsync(Entities.Activity activity);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    Task<Entities.Activity> UpdateActivityAsync(Entities.Activity activity);

    /// <summary>
    /// Deletes an activity by ID.
    /// </summary>
    Task DeleteActivityAsync(string activityId);

    /// <summary>
    /// Changes activity status.
    /// </summary>
    Task<Entities.Activity> UpdateActivityStatusAsync(string activityId, Entities.ActivityEnums.ActivityStatus status);
}
