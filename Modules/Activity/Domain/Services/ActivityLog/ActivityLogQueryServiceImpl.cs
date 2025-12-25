using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;

/// <summary>
/// Implementation of IActivityLogQueryService.
/// </summary>
public class ActivityLogQueryServiceImpl : IActivityLogQueryService
{
    private readonly IActivityLogDao _activityLogDao;

    public ActivityLogQueryServiceImpl(IActivityLogDao activityLogDao)
    {
        _activityLogDao = activityLogDao;
    }

    public async Task<Entities.ActivityLog?> GetActivityLogByIdAsync(string logId)
    {
        return await _activityLogDao.GetByIdAsync(logId);
    }

    public async Task<PagedResult<Entities.ActivityLog>> GetActivityLogsAsync(ActivityLogFilter filter)
    {
        return await _activityLogDao.GetAllAsync(filter);
    }
}
