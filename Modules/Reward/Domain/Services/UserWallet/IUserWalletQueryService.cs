using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;

/// <summary>
/// Query service interface for UserWallet operations.
/// </summary>
public interface IUserWalletQueryService
{
    /// <summary>
    /// Gets a wallet by user ID and program ID with associated program details.
    /// </summary>
    Task<UserWallet?> GetWalletByUserAndProgramAsync(string userId, string programId);

    /// <summary>
    /// Gets all wallets for a specific user.
    /// </summary>
    Task<List<UserWallet>> GetWalletsByUserIdAsync(string userId);

    /// <summary>
    /// Gets all wallets for a specific program with pagination.
    /// Used for admin view to see all users in a reward program.
    /// </summary>
    Task<(List<UserWallet> Items, int TotalCount)> GetWalletsByProgramAsync(
        string programId, int pageNumber = 1, int pageSize = 20);
}
