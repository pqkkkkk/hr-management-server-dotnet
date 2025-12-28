using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Activity;

/// <summary>
/// Implementation of IActivityQueryService.
/// </summary>
public class ActivityQueryServiceImpl : IActivityQueryService
{
    private readonly IActivityDao _activityDao;
    private readonly IParticipantDao _participantDao;
    private readonly IActivityLogDao _activityLogDao;

    public ActivityQueryServiceImpl(
        IActivityDao activityDao,
        IParticipantDao participantDao,
        IActivityLogDao activityLogDao)
    {
        _activityDao = activityDao;
        _participantDao = participantDao;
        _activityLogDao = activityLogDao;
    }

    public async Task<Entities.Activity?> GetActivityByIdAsync(string activityId)
    {
        return await _activityDao.GetByIdAsync(activityId);
    }

    public async Task<PagedResult<Entities.Activity>> GetActivitiesAsync(ActivityFilter filter)
    {
        return await _activityDao.GetAllAsync(filter);
    }

    public async Task<PagedResult<Entities.Activity>> GetMyActivitiesAsync(string employeeId, ActivityFilter filter)
    {
        return await _activityDao.GetByEmployeeIdAsync(employeeId, filter);
    }

    public async Task<IEnumerable<Entities.Participant>> GetLeaderboardAsync(string activityId, int top = 10)
    {
        return await _participantDao.GetLeaderboardAsync(activityId, top);
    }

    public async Task<ActivityStatistics> GetActivityStatisticsAsync(string activityId)
    {
        var activity = await _activityDao.GetByIdAsync(activityId);
        if (activity == null)
        {
            throw new KeyNotFoundException($"Activity with ID {activityId} not found");
        }

        var participants = await _participantDao.GetByActivityIdAsync(activityId);
        var logs = await _activityLogDao.GetAllAsync(new ActivityLogFilter { ActivityId = activityId, PageSize = int.MaxValue });

        var participantList = participants.ToList();
        var logList = logs.Content.ToList();

        var totalDistance = logList
            .Where(l => l.Status == ActivityLogStatus.APPROVED)
            .Sum(l => l.Distance);

        return new ActivityStatistics
        {
            ActivityId = activityId,
            TotalParticipants = participantList.Count,
            ActiveParticipants = participantList.Count(p => p.Status == ParticipantStatus.ACTIVE),
            TotalDistance = totalDistance,
            TotalLogs = logList.Count,
            PendingLogs = logList.Count(l => l.Status == ActivityLogStatus.PENDING),
            ApprovedLogs = logList.Count(l => l.Status == ActivityLogStatus.APPROVED),
            RejectedLogs = logList.Count(l => l.Status == ActivityLogStatus.REJECTED),
            AverageDistancePerParticipant = participantList.Count > 0
                ? totalDistance / participantList.Count
                : 0
        };
    }
}
