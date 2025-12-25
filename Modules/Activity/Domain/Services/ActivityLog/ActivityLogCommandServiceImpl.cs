using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Services.Template;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

// Type alias to avoid namespace conflict with ActivityLog folder
using ActivityLogEntity = HrManagement.Api.Modules.Activity.Domain.Entities.ActivityLog;

namespace HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;

/// <summary>
/// Implementation of IActivityLogCommandService.
/// Uses transactions and EF Core dirty checking for updates.
/// </summary>
public class ActivityLogCommandServiceImpl : IActivityLogCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IActivityLogDao _activityLogDao;
    private readonly IActivityDao _activityDao;
    private readonly IParticipantDao _participantDao;
    private readonly ActivityTemplateRegistry _templateRegistry;

    public ActivityLogCommandServiceImpl(
        AppDbContext dbContext,
        IActivityLogDao activityLogDao,
        IActivityDao activityDao,
        IParticipantDao participantDao,
        ActivityTemplateRegistry templateRegistry)
    {
        _dbContext = dbContext;
        _activityLogDao = activityLogDao;
        _activityDao = activityDao;
        _participantDao = participantDao;
        _templateRegistry = templateRegistry;
    }

    public async Task<ActivityLogEntity> SubmitLogAsync(ActivityLogEntity log)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Validate participant exists
            var participant = await _participantDao.GetByIdAsync(log.ParticipantId)
                ?? throw new KeyNotFoundException($"Participant with ID {log.ParticipantId} not found");

            // Get activity to check status
            var activity = await _activityDao.GetByIdAsync(participant.ActivityId)
                ?? throw new KeyNotFoundException($"Activity not found");

            // Check if activity is in progress
            if (activity.Status != ActivityStatus.IN_PROGRESS)
            {
                throw new InvalidOperationException($"Activity is not in progress. Current status: {activity.Status}");
            }

            // Set default values
            log.ActivityLogId = Guid.NewGuid().ToString();
            log.Status = ActivityLogStatus.PENDING;
            log.CreatedAt = DateTime.UtcNow;

            var created = await _activityLogDao.CreateAsync(log);
            await transaction.CommitAsync();
            return created;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ActivityLogEntity> ApproveLogAsync(string logId, string reviewerId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var log = await _activityLogDao.GetByIdAsync(logId)
                ?? throw new KeyNotFoundException($"Activity log with ID {logId} not found");

            if (log.Status != ActivityLogStatus.PENDING)
            {
                throw new InvalidOperationException($"Log is not pending. Current status: {log.Status}");
            }

            // Get activity and template for validation
            var activity = log.Participant.Activity;
            var provider = _templateRegistry.GetProviderByTemplateId(activity.TemplateId)
                ?? throw new InvalidOperationException($"Template provider not found for template: {activity.TemplateId}");

            // Validate the log
            var validationResult = provider.ValidateLog(log);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Validation failed: {validationResult.ErrorMessage}");
            }

            // Calculate points
            var points = provider.CalculatePoints(log);

            // Update log status - EF dirty checking
            log.Status = ActivityLogStatus.APPROVED;
            log.ReviewerId = reviewerId;

            // Update participant's total score - EF dirty checking
            log.Participant.TotalScore += points;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return log;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ActivityLogEntity> RejectLogAsync(string logId, string reviewerId, string reason)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var log = await _activityLogDao.GetByIdAsync(logId)
                ?? throw new KeyNotFoundException($"Activity log with ID {logId} not found");

            if (log.Status != ActivityLogStatus.PENDING)
            {
                throw new InvalidOperationException($"Log is not pending. Current status: {log.Status}");
            }

            // Update using EF dirty checking
            log.Status = ActivityLogStatus.REJECTED;
            log.ReviewerId = reviewerId;
            log.RejectReason = reason;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return log;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteLogAsync(string logId)
    {
        var log = await _activityLogDao.GetByIdAsync(logId)
            ?? throw new KeyNotFoundException($"Activity log with ID {logId} not found");

        await _activityLogDao.DeleteAsync(logId);
    }
}
