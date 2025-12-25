using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;

/// <summary>
/// Query service interface for ActivityLog operations.
/// </summary>
public interface IActivityLogQueryService
{
    /// <summary>
    /// Gets an activity log by ID.
    /// </summary>
    Task<Entities.ActivityLog?> GetActivityLogByIdAsync(string logId);

    /// <summary>
    /// Gets activity logs with filtering and pagination.
    /// Filter supports: activityId, employeeId, status, fromDate, toDate.
    /// </summary>
    Task<PagedResult<Entities.ActivityLog>> GetActivityLogsAsync(ActivityLogFilter filter);
}
