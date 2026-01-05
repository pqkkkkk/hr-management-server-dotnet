using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Domain.Dao;

/// <summary>
/// Data access interface for RewardProgram entity.
/// </summary>
public interface IRewardProgramDao
{
    /// <summary>
    /// Gets a reward program by ID with related entities (Items, Policies).
    /// </summary>
    Task<RewardProgram?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all reward programs with filtering and pagination.
    /// </summary>
    Task<PagedResult<RewardProgram>> GetAllAsync(RewardProgramFilter filter);

    /// <summary>
    /// Creates a new reward program with its items and policies.
    /// </summary>
    Task<RewardProgram> CreateAsync(RewardProgram program);

    /// <summary>
    /// Updates an existing reward program.
    /// </summary>
    Task UpdateAsync(RewardProgram program);

    /// <summary>
    /// Deletes a reward program by ID.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Gets the currently active reward program.
    /// According to business rules, only one program can be active at a time.
    /// </summary>
    Task<RewardProgram?> GetActiveAsync();
}
