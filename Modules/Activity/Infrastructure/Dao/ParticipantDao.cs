using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IParticipantDao.
/// </summary>
public class ParticipantDao : IParticipantDao
{
    private readonly AppDbContext _context;

    public ParticipantDao(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Participant?> GetByIdAsync(string id)
    {
        return await _context.Participants
            .Include(p => p.Activity)
            .Include(p => p.ActivityLogs)
            .FirstOrDefaultAsync(p => p.ParticipantId == id);
    }

    public async Task<Participant?> GetByActivityAndEmployeeAsync(string activityId, string employeeId)
    {
        return await _context.Participants
            .Include(p => p.Activity)
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<Participant>> GetByActivityIdAsync(string activityId)
    {
        return await _context.Participants
            .Where(p => p.ActivityId == activityId)
            .Include(p => p.ActivityLogs)
            .OrderByDescending(p => (double)p.TotalScore)
            .ToListAsync();
    }

    public async Task<IEnumerable<Participant>> GetByEmployeeIdAsync(string employeeId)
    {
        return await _context.Participants
            .Where(p => p.EmployeeId == employeeId)
            .Include(p => p.Activity)
            .OrderByDescending(p => p.JoinedAt)
            .ToListAsync();
    }

    public async Task<Participant> CreateAsync(Participant participant)
    {
        if (string.IsNullOrEmpty(participant.ParticipantId))
        {
            participant.ParticipantId = Guid.NewGuid().ToString();
        }

        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        return participant;
    }

    public async Task UpdateAsync(Participant participant)
    {
        _context.Participants.Update(participant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var participant = await _context.Participants
            .Include(p => p.ActivityLogs)
            .FirstOrDefaultAsync(p => p.ParticipantId == id);

        if (participant != null)
        {
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Participant>> GetLeaderboardAsync(string activityId, int top = 10)
    {
        return await _context.Participants
            .Where(p => p.ActivityId == activityId && p.Status == ParticipantStatus.ACTIVE)
            .OrderByDescending(p => (double)p.TotalScore)
            .Take(top)
            .ToListAsync();
    }

    public async Task<Participant?> GetByActivityAndEmployeeWithStatsAsync(string activityId, string employeeId)
    {
        return await _context.Participants
            .Include(p => p.ActivityLogs)
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.EmployeeId == employeeId);
    }
}
