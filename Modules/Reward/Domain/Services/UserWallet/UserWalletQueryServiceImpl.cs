using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;

namespace HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;

/// <summary>
/// Implementation of IUserWalletQueryService.
/// Handles read operations for user wallets.
/// </summary>
public class UserWalletQueryServiceImpl : IUserWalletQueryService
{
    private readonly IUserWalletDao _userWalletDao;

    public UserWalletQueryServiceImpl(IUserWalletDao userWalletDao)
    {
        _userWalletDao = userWalletDao;
    }

    /// <summary>
    /// Gets a wallet by user ID and program ID with associated program details.
    /// </summary>
    public async Task<UserWallet?> GetWalletByUserAndProgramAsync(string userId, string programId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(programId))
        {
            throw new ArgumentException("Program ID is required.", nameof(programId));
        }

        return await _userWalletDao.GetByUserIdAndProgramIdAsync(userId, programId);
    }

    /// <summary>
    /// Gets all wallets for a specific user.
    /// </summary>
    public async Task<List<UserWallet>> GetWalletsByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        return await _userWalletDao.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Gets all wallets for a specific program with pagination.
    /// </summary>
    public async Task<(List<UserWallet> Items, int TotalCount)> GetWalletsByProgramAsync(
        string programId, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(programId))
        {
            throw new ArgumentException("Program ID is required.", nameof(programId));
        }

        return await _userWalletDao.GetByProgramIdAsync(programId, pageNumber, pageSize);
    }
}
