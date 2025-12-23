using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IRewardItemDao.
/// </summary>
public class RewardItemDao : IRewardItemDao
{
    private readonly AppDbContext _context;

    public RewardItemDao(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a reward item by ID.
    /// </summary>
    public async Task<RewardItem?> GetByIdAsync(string id)
    {
        return await _context.RewardItems
            .FirstOrDefaultAsync(i => i.RewardItemId == id);
    }

    /// <summary>
    /// Gets all reward items for a specific program.
    /// </summary>
    public async Task<List<RewardItem>> GetByProgramIdAsync(string programId)
    {
        return await _context.RewardItems
            .Where(i => i.ProgramId == programId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets multiple reward items by their IDs.
    /// Used for exchange validation.
    /// </summary>
    public async Task<List<RewardItem>> GetByIdsAsync(List<string> ids)
    {
        return await _context.RewardItems
            .Where(i => ids.Contains(i.RewardItemId))
            .ToListAsync();
    }

    /// <summary>
    /// Updates an existing reward item.
    /// </summary>
    public async Task UpdateAsync(RewardItem item)
    {
        _context.RewardItems.Update(item);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates multiple reward items in batch.
    /// </summary>
    public async Task UpdateBatchAsync(List<RewardItem> items)
    {
        _context.RewardItems.UpdateRange(items);
        await _context.SaveChangesAsync();
    }
}
