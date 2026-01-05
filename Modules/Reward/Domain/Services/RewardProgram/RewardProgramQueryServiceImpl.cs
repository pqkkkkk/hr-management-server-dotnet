using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

/// <summary>
/// Implementation of IRewardProgramQueryService.
/// Handles read operations for reward programs.
/// </summary>
public class RewardProgramQueryServiceImpl : IRewardProgramQueryService
{
    private readonly IRewardProgramDao _rewardProgramDao;

    public RewardProgramQueryServiceImpl(IRewardProgramDao rewardProgramDao)
    {
        _rewardProgramDao = rewardProgramDao;
    }

    /// <summary>
    /// Gets a reward program by ID with its items and policies.
    /// </summary>
    public async Task<RewardProgram?> GetRewardProgramByIdAsync(string rewardProgramId)
    {
        return await _rewardProgramDao.GetByIdAsync(rewardProgramId);
    }

    /// <summary>
    /// Gets all reward programs with filtering and pagination.
    /// </summary>
    public async Task<PagedResult<RewardProgram>> GetRewardProgramsAsync(RewardProgramFilter filter)
    {
        return await _rewardProgramDao.GetAllAsync(filter);
    }

    /// <summary>
    /// Gets the currently active reward program.
    /// According to business rules, only one program can be active at a time.
    /// </summary>
    public async Task<RewardProgram?> GetActiveRewardProgramAsync()
    {
        return await _rewardProgramDao.GetActiveAsync();
    }
}
