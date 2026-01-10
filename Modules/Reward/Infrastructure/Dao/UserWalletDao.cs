using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IUserWalletDao.
/// </summary>
public class UserWalletDao : IUserWalletDao
{
    private readonly AppDbContext _context;

    public UserWalletDao(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a wallet by ID with related Program.
    /// </summary>
    public async Task<UserWallet?> GetByIdAsync(string id)
    {
        return await _context.UserWallets
            .Include(w => w.Program)
            .FirstOrDefaultAsync(w => w.UserWalletId == id);
    }

    /// <summary>
    /// Gets a wallet by user ID and program ID.
    /// </summary>
    public async Task<UserWallet?> GetByUserIdAndProgramIdAsync(string userId, string programId)
    {
        return await _context.UserWallets
            .Include(w => w.Program)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProgramId == programId);
    }

    /// <summary>
    /// Gets all wallets for a specific program.
    /// </summary>
    public async Task<List<UserWallet>> GetByProgramIdAsync(string programId)
    {
        return await _context.UserWallets
            .Where(w => w.ProgramId == programId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets wallets for a specific program with pagination.
    /// </summary>
    public async Task<(List<UserWallet> Items, int TotalCount)> GetByProgramIdAsync(
        string programId, int pageNumber, int pageSize)
    {
        var query = _context.UserWallets
            .Where(w => w.ProgramId == programId)
            .OrderBy(w => w.UserName);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Gets all wallets for a specific user with Program details.
    /// </summary>
    public async Task<List<UserWallet>> GetByUserIdAsync(string userId)
    {
        return await _context.UserWallets
            .Include(w => w.Program)
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new wallet.
    /// ID is auto-generated if not set.
    /// </summary>
    public async Task<UserWallet> CreateAsync(UserWallet wallet)
    {
        if (string.IsNullOrEmpty(wallet.UserWalletId))
        {
            wallet.UserWalletId = Guid.NewGuid().ToString();
        }

        _context.UserWallets.Add(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }

    /// <summary>
    /// Creates multiple wallets in batch.
    /// IDs are auto-generated if not set.
    /// </summary>
    public async Task<List<UserWallet>> CreateBatchAsync(List<UserWallet> wallets)
    {
        foreach (var wallet in wallets)
        {
            if (string.IsNullOrEmpty(wallet.UserWalletId))
            {
                wallet.UserWalletId = Guid.NewGuid().ToString();
            }
        }

        _context.UserWallets.AddRange(wallets);
        await _context.SaveChangesAsync();

        return wallets;
    }

    /// <summary>
    /// Updates an existing wallet.
    /// </summary>
    public async Task UpdateAsync(UserWallet wallet)
    {
        _context.UserWallets.Update(wallet);
        await _context.SaveChangesAsync();
    }
}
