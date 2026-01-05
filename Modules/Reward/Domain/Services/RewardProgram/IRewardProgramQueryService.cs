using System;
using HrManagement.Api.Shared.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Filter;
namespace HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

public interface IRewardProgramQueryService
{
    Task<RewardProgram?> GetRewardProgramByIdAsync(string rewardProgramId);
    Task<PagedResult<RewardProgram>> GetRewardProgramsAsync(RewardProgramFilter filter);
    
    /// <summary>
    /// Gets the currently active reward program.
    /// According to business rules, only one program can be active at a time.
    /// </summary>
    Task<RewardProgram?> GetActiveRewardProgramAsync();
}
