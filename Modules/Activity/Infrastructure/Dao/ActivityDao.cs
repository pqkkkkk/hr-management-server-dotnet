using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IActivityDao.
/// </summary>
public class ActivityDao : IActivityDao
{
    private readonly AppDbContext _context;

    public ActivityDao(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Activity?> GetByIdAsync(string id)
    {
        return await _context.Activities
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.ActivityId == id);
    }

    public async Task<PagedResult<Domain.Entities.Activity>> GetAllAsync(ActivityFilter filter)
    {
        var query = _context.Activities.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            query = query.Where(a => a.Name.Contains(filter.NameContains));
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(a => a.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        if (filter.IsActive.HasValue && filter.IsActive.Value)
        {
            query = query.Where(a => a.Status == ActivityStatus.OPEN || a.Status == ActivityStatus.IN_PROGRESS);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(a => a.StartDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(a => a.EndDate <= filter.ToDate.Value);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(a => a.Participants)
            .ToListAsync();

        return PagedResult<Domain.Entities.Activity>.Create(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    public async Task<PagedResult<Domain.Entities.Activity>> GetByEmployeeIdAsync(string employeeId, ActivityFilter filter)
    {
        var query = _context.Activities
            .Where(a => a.Participants.Any(p => p.EmployeeId == employeeId));

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            query = query.Where(a => a.Name.Contains(filter.NameContains));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        if (filter.IsActive.HasValue && filter.IsActive.Value)
        {
            query = query.Where(a => a.Status == ActivityStatus.OPEN || a.Status == ActivityStatus.IN_PROGRESS);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(a => a.Participants)
            .ToListAsync();

        return PagedResult<Domain.Entities.Activity>.Create(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    public async Task<Domain.Entities.Activity> CreateAsync(Domain.Entities.Activity activity)
    {
        if (string.IsNullOrEmpty(activity.ActivityId))
        {
            activity.ActivityId = Guid.NewGuid().ToString();
        }

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return activity;
    }

    public async Task UpdateAsync(Domain.Entities.Activity activity)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var activity = await _context.Activities
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.ActivityId == id);

        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }
}
