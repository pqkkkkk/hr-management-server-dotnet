using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Services.Participant;

/// <summary>
/// Implementation of IParticipantCommandService.
/// Uses transactions and EF Core dirty checking for updates.
/// </summary>
public class ParticipantCommandServiceImpl : IParticipantCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IActivityDao _activityDao;
    private readonly IParticipantDao _participantDao;

    public ParticipantCommandServiceImpl(
        AppDbContext dbContext,
        IActivityDao activityDao,
        IParticipantDao participantDao)
    {
        _dbContext = dbContext;
        _activityDao = activityDao;
        _participantDao = participantDao;
    }

    public async Task<Entities.Participant> RegisterParticipantAsync(string activityId, string employeeId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var activity = await _activityDao.GetByIdAsync(activityId)
                ?? throw new KeyNotFoundException($"Activity with ID {activityId} not found");

            // Check if activity is open for registration
            if (activity.Status != ActivityStatus.OPEN && activity.Status != ActivityStatus.IN_PROGRESS)
            {
                throw new InvalidOperationException($"Activity is not open for registration. Current status: {activity.Status}");
            }

            // Check if already registered
            var existing = await _participantDao.GetByActivityAndEmployeeAsync(activityId, employeeId);
            if (existing != null)
            {
                throw new InvalidOperationException($"Employee {employeeId} is already registered for this activity");
            }

            var participant = new Entities.Participant
            {
                ParticipantId = Guid.NewGuid().ToString(),
                ActivityId = activityId,
                EmployeeId = employeeId,
                JoinedAt = DateTime.UtcNow,
                Status = ParticipantStatus.ACTIVE,
                TotalScore = 0
            };

            var created = await _participantDao.CreateAsync(participant);
            await transaction.CommitAsync();
            return created;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveParticipantAsync(string activityId, string employeeId)
    {
        var participant = await _participantDao.GetByActivityAndEmployeeAsync(activityId, employeeId)
            ?? throw new KeyNotFoundException($"Participant not found for activity {activityId} and employee {employeeId}");

        await _participantDao.DeleteAsync(participant.ParticipantId);
    }

    public async Task<Entities.Participant> UpdateParticipantStatusAsync(string participantId, ParticipantStatus status)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var participant = await _participantDao.GetByIdAsync(participantId)
                ?? throw new KeyNotFoundException($"Participant with ID {participantId} not found");

            participant.Status = status;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return participant;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
