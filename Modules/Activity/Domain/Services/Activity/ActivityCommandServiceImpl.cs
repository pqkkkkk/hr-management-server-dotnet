using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Services.Template;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Activity;

/// <summary>
/// Implementation of IActivityCommandService.
/// Uses transactions and EF Core dirty checking for updates.
/// </summary>
public class ActivityCommandServiceImpl : IActivityCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IActivityDao _activityDao;
    private readonly ActivityTemplateRegistry _templateRegistry;

    public ActivityCommandServiceImpl(
        AppDbContext dbContext,
        IActivityDao activityDao,
        ActivityTemplateRegistry templateRegistry)
    {
        _dbContext = dbContext;
        _activityDao = activityDao;
        _templateRegistry = templateRegistry;
    }

    public async Task<Entities.Activity> CreateActivityAsync(Entities.Activity activity)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            ValidateActivity(activity);

            // Set default values
            activity.ActivityId = Guid.NewGuid().ToString();
            activity.CreatedAt = DateTime.UtcNow;
            activity.Status = ActivityStatus.IN_PROGRESS;

            var created = await _activityDao.CreateAsync(activity);
            await transaction.CommitAsync();
            return created;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Entities.Activity> UpdateActivityAsync(Entities.Activity activity)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var existing = await _activityDao.GetByIdAsync(activity.ActivityId)
                ?? throw new KeyNotFoundException($"Activity with ID {activity.ActivityId} not found");

            // Validate dates and config for update
            ValidateDates(activity.StartDate, activity.EndDate);

            var provider = _templateRegistry.GetProviderByTemplateId(existing.TemplateId)
                ?? throw new ArgumentException($"Template with ID '{existing.TemplateId}' not found.");
            var configResult = provider.ValidateConfig(activity.Config);
            if (!configResult.IsValid)
            {
                throw new ArgumentException(configResult.ErrorMessage);
            }

            existing.Name = activity.Name;
            existing.Description = activity.Description;
            existing.BannerUrl = activity.BannerUrl;
            existing.StartDate = activity.StartDate;
            existing.EndDate = activity.EndDate;
            existing.Config = activity.Config;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return existing;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteActivityAsync(string activityId)
    {
        var existing = await _activityDao.GetByIdAsync(activityId)
            ?? throw new KeyNotFoundException($"Activity with ID {activityId} not found");

        // Soft delete - close the activity instead of removing
        existing.Status = ActivityStatus.CLOSED;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Entities.Activity> UpdateActivityStatusAsync(string activityId, ActivityStatus status)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var existing = await _activityDao.GetByIdAsync(activityId)
                ?? throw new KeyNotFoundException($"Activity with ID {activityId} not found");

            existing.Status = status;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return existing;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    #region Validation

    private void ValidateActivity(Entities.Activity activity)
    {
        if (string.IsNullOrWhiteSpace(activity.Name))
        {
            throw new ArgumentException("Activity name is required.");
        }

        // Validate template exists and get provider
        var provider = _templateRegistry.GetProviderByTemplateId(activity.TemplateId)
            ?? throw new ArgumentException($"Template with ID '{activity.TemplateId}' not found.");

        ValidateDates(activity.StartDate, activity.EndDate);

        // Delegate config validation to the provider
        var configResult = provider.ValidateConfig(activity.Config);
        if (!configResult.IsValid)
        {
            throw new ArgumentException(configResult.ErrorMessage);
        }
    }

    private void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date.");
        }
    }

    #endregion
}
