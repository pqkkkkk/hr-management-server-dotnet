using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

// Type alias to avoid namespace conflict
using ActivityLogEntity = HrManagement.Api.Modules.Activity.Domain.Entities.ActivityLog;

namespace HrManagement.Api.Modules.Activity.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IActivityLogDao.
/// </summary>
public class ActivityLogDao : IActivityLogDao
{
    private readonly AppDbContext _context;

    public ActivityLogDao(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityLogEntity?> GetByIdAsync(string id)
    {
        return await _context.ActivityLogs
            .Include(l => l.Participant)
                .ThenInclude(p => p.Activity)
            .FirstOrDefaultAsync(l => l.ActivityLogId == id);
    }

    public async Task<PagedResult<ActivityLogEntity>> GetAllAsync(ActivityLogFilter filter)
    {
        var query = _context.ActivityLogs
            .Include(l => l.Participant)
                .ThenInclude(p => p.Activity)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.ActivityId))
        {
            query = query.Where(l => l.Participant.ActivityId == filter.ActivityId);
        }

        if (!string.IsNullOrWhiteSpace(filter.EmployeeId))
        {
            query = query.Where(l => l.Participant.EmployeeId == filter.EmployeeId);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(l => l.Status == filter.Status.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(l => l.LogDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(l => l.LogDate <= filter.ToDate.Value);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return PagedResult<ActivityLogEntity>.Create(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    public async Task<IEnumerable<ActivityLogEntity>> GetByParticipantIdAsync(string participantId)
    {
        return await _context.ActivityLogs
            .Where(l => l.ParticipantId == participantId)
            .Include(l => l.Participant)
                .ThenInclude(p => p.Activity)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();
    }

    public async Task<ActivityLogEntity> CreateAsync(ActivityLogEntity log)
    {
        if (string.IsNullOrEmpty(log.ActivityLogId))
        {
            log.ActivityLogId = Guid.NewGuid().ToString();
        }

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        return log;
    }

    public async Task UpdateAsync(ActivityLogEntity log)
    {
        _context.ActivityLogs.Update(log);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var log = await _context.ActivityLogs
            .FirstOrDefaultAsync(l => l.ActivityLogId == id);

        if (log != null)
        {
            _context.ActivityLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalDistanceByDateAsync(string participantId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _context.ActivityLogs
            .Where(l => l.ParticipantId == participantId 
                && l.LogDate >= startOfDay 
                && l.LogDate <= endOfDay
                && l.Status == ActivityLogStatus.APPROVED)
            .SumAsync(l => l.Distance);
    }
}
