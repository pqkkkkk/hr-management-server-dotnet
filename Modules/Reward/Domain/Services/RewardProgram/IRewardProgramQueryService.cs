using System;
using HrManagement.Api.Shared.DTOs;
using HrManagement.Api.Modules.Reward.Domain.Filter;
namespace HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

public interface IRewardProgramQueryService
{
    Task<RewardProgram?> GetRewardProgramByIdAsync(Guid rewardProgramId);
    Task<PagedResult<RewardProgram>> GetRewardProgramsAsync(RewardProgramFilter filter);
}
