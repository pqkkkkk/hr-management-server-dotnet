using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

public interface IRewardProgramCommandService
{
    /// <summary>
    /// Creates a new reward program with its items and policies.
    /// Entity should have RewardItems and Policies navigation properties set.
    /// Auto-creates wallets for all users (managers get giving_budget).
    /// </summary>
    Task<RewardProgram> CreateRewardProgramAsync(RewardProgram program);

    /// <summary>
    /// Updates an existing reward program.
    /// The entity should contain the updated values. EF Core change tracking handles partial updates.
    /// </summary>
    Task<RewardProgram> UpdateRewardProgramAsync(RewardProgram program);

    /// <summary>
    /// Deletes a reward program by ID.
    /// </summary>
    Task DeleteRewardProgramAsync(string programId);

    /// <summary>
    /// Deactivates a reward program. Points become frozen (no gift/exchange).
    /// </summary>
    Task DeactivateRewardProgramAsync(string programId);
}
