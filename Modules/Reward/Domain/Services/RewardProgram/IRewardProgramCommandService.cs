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
    /// Deactivates a reward program. Points become frozen (no gift/exchange).
    /// </summary>
    Task DeactivateRewardProgramAsync(string programId);
}
