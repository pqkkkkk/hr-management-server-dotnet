using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Activity;

/// <summary>
/// Query service interface for Activity operations.
/// </summary>
public interface IActivityQueryService
{
    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    Task<Entities.Activity?> GetActivityByIdAsync(string activityId);

    /// <summary>
    /// Gets all activities with filtering and pagination.
    /// </summary>
    Task<PagedResult<Entities.Activity>> GetActivitiesAsync(ActivityFilter filter);

    /// <summary>
    /// Gets activities that an employee has joined.
    /// </summary>
    Task<PagedResult<Entities.Activity>> GetMyActivitiesAsync(string employeeId, ActivityFilter filter);

    /// <summary>
    /// Gets leaderboard for an activity.
    /// </summary>
    Task<IEnumerable<Entities.Participant>> GetLeaderboardAsync(string activityId, int top = 10);

    /// <summary>
    /// Gets statistics for an activity (for admin/manager).
    /// </summary>
    Task<ActivityStatistics> GetActivityStatisticsAsync(string activityId);
}
