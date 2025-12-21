using System;

namespace HrManagement.Api.Modules.Reward.Domain.Dao;

public interface IRewardProgramDao
{
    public Task<RewardProgram> GetRewardProgramAsync(Guid id);
    public Task<RewardProgram> CreateRewardProgramAsync(RewardProgram rewardProgram);
}
