using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Domain.Dao;

/// <summary>
/// Data access interface for UserWallet entity.
/// </summary>
public interface IUserWalletDao
{
    /// <summary>
    /// Gets a wallet by its ID.
    /// </summary>
    Task<UserWallet?> GetByIdAsync(string id);

    /// <summary>
    /// Gets a wallet by user ID and program ID.
    /// </summary>
    Task<UserWallet?> GetByUserIdAndProgramIdAsync(string userId, string programId);

    /// <summary>
    /// Gets all wallets for a specific program.
    /// </summary>
    Task<List<UserWallet>> GetByProgramIdAsync(string programId);

    /// <summary>
    /// Gets all wallets for a specific user.
    /// </summary>
    Task<List<UserWallet>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Creates a new wallet.
    /// </summary>
    Task<UserWallet> CreateAsync(UserWallet wallet);

    /// <summary>
    /// Creates multiple wallets in batch.
    /// </summary>
    Task<List<UserWallet>> CreateBatchAsync(List<UserWallet> wallets);

    /// <summary>
    /// Updates an existing wallet.
    /// </summary>
    Task UpdateAsync(UserWallet wallet);
}
