using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IRewardProgramDao.
/// </summary>
public class RewardProgramDao : IRewardProgramDao
{
    private readonly AppDbContext _context;

    public RewardProgramDao(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a reward program by ID with related Items and Policies.
    /// </summary>
    public async Task<RewardProgram?> GetByIdAsync(string id)
    {
        return await _context.RewardPrograms
            .Include(p => p.RewardItems)
            .Include(p => p.Policies)
            .FirstOrDefaultAsync(p => p.RewardProgramId == id);
    }

    /// <summary>
    /// Gets all reward programs with filtering and pagination.
    /// </summary>
    public async Task<PagedResult<RewardProgram>> GetAllAsync(RewardProgramFilter filter)
    {
        var query = _context.RewardPrograms.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            query = query.Where(p => p.Name.Contains(filter.NameContains));
        }

        if (filter.IsActive.HasValue)
        {
            var targetStatus = filter.IsActive.Value ? ProgramStatus.ACTIVE : ProgramStatus.INACTIVE;
            query = query.Where(p => p.Status == targetStatus);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(p => p.StartDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(p => p.RewardItems)
            .Include(p => p.Policies)
            .ToListAsync();

        return PagedResult<RewardProgram>.Create(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    /// <summary>
    /// Creates a new reward program with its items and policies.
    /// IDs are auto-generated if not set.
    /// </summary>
    public async Task<RewardProgram> CreateAsync(RewardProgram program)
    {
        // Generate IDs if not set
        if (string.IsNullOrEmpty(program.RewardProgramId))
        {
            program.RewardProgramId = Guid.NewGuid().ToString();
        }

        foreach (var item in program.RewardItems)
        {
            if (string.IsNullOrEmpty(item.RewardItemId))
            {
                item.RewardItemId = Guid.NewGuid().ToString();
            }
            item.ProgramId = program.RewardProgramId;
        }

        foreach (var policy in program.Policies)
        {
            if (string.IsNullOrEmpty(policy.PolicyId))
            {
                policy.PolicyId = Guid.NewGuid().ToString();
            }
            policy.ProgramId = program.RewardProgramId;
        }

        _context.RewardPrograms.Add(program);
        await _context.SaveChangesAsync();

        return program;
    }

    /// <summary>
    /// Updates an existing reward program.
    /// </summary>
    public async Task UpdateAsync(RewardProgram program)
    {
        _context.RewardPrograms.Update(program);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a reward program by ID.
    /// This will cascade delete related Items and Policies.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        var program = await _context.RewardPrograms
            .Include(p => p.RewardItems)
            .Include(p => p.Policies)
            .FirstOrDefaultAsync(p => p.RewardProgramId == id);

        if (program != null)
        {
            _context.RewardPrograms.Remove(program);
            await _context.SaveChangesAsync();
        }
    }
}
