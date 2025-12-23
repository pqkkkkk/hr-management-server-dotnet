using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Domain.Dao;

/// <summary>
/// Data access interface for RewardItem entity.
/// </summary>
public interface IRewardItemDao
{
    /// <summary>
    /// Gets a reward item by its ID.
    /// </summary>
    Task<RewardItem?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all reward items for a specific program.
    /// </summary>
    Task<List<RewardItem>> GetByProgramIdAsync(string programId);

    /// <summary>
    /// Gets multiple reward items by their IDs.
    /// Used for exchange validation.
    /// </summary>
    Task<List<RewardItem>> GetByIdsAsync(List<string> ids);

    /// <summary>
    /// Updates an existing reward item (e.g., for quantity changes).
    /// </summary>
    Task UpdateAsync(RewardItem item);

    /// <summary>
    /// Updates multiple reward items in batch.
    /// </summary>
    Task UpdateBatchAsync(List<RewardItem> items);
}
