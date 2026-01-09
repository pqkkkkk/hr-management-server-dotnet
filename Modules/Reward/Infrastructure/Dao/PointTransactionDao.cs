using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Infrastructure.Dao;

/// <summary>
/// EF Core implementation of IPointTransactionDao.
/// </summary>
public class PointTransactionDao : IPointTransactionDao
{
    private readonly AppDbContext _context;

    public PointTransactionDao(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a transaction by ID with related Items and Wallets.
    /// </summary>
    public async Task<PointTransaction?> GetByIdAsync(string id)
    {
        return await _context.PointTransactions
            .Include(t => t.Items)
                .ThenInclude(i => i.RewardItem)
            .Include(t => t.SourceWallet)
                .ThenInclude(w => w!.Program)
            .Include(t => t.DestinationWallet)
                .ThenInclude(w => w!.Program)
            .FirstOrDefaultAsync(t => t.PointTransactionId == id);
    }

    /// <summary>
    /// Gets all transactions with filtering and pagination.
    /// </summary>
    public async Task<PagedResult<PointTransaction>> GetAllAsync(PointTransactionFilter filter)
    {
        var query = _context.PointTransactions.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.EmployeeId))
        {
            // Filter by transactions where user is sender or receiver
            query = query.Where(t =>
                (t.SourceWallet != null && t.SourceWallet.UserId == filter.EmployeeId) ||
                (t.DestinationWallet != null && t.DestinationWallet.UserId == filter.EmployeeId));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
        }

        if (filter.TransactionType.HasValue)
        {
            query = query.Where(t => t.Type == filter.TransactionType.Value);
        }

        // Get total count before pagination
        var totalItems = await query.CountAsync();

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(t => t.Items)
                .ThenInclude(i => i.RewardItem)
            .Include(t => t.SourceWallet)
                .ThenInclude(w => w!.Program)
            .Include(t => t.DestinationWallet)
                .ThenInclude(w => w!.Program)
            .ToListAsync();

        return PagedResult<PointTransaction>.Create(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    /// <summary>
    /// Creates a new transaction.
    /// ID is auto-generated if not set.
    /// </summary>
    public async Task<PointTransaction> CreateAsync(PointTransaction transaction)
    {
        // Generate IDs if not set
        if (string.IsNullOrEmpty(transaction.PointTransactionId))
        {
            transaction.PointTransactionId = Guid.NewGuid().ToString();
        }

        foreach (var item in transaction.Items)
        {
            if (string.IsNullOrEmpty(item.ItemInTransactionId))
            {
                item.ItemInTransactionId = Guid.NewGuid().ToString();
            }
            item.TransactionId = transaction.PointTransactionId;
        }

        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    /// <summary>
    /// Creates multiple transactions in batch.
    /// IDs are auto-generated if not set.
    /// </summary>
    public async Task<List<PointTransaction>> CreateBatchAsync(List<PointTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (string.IsNullOrEmpty(transaction.PointTransactionId))
            {
                transaction.PointTransactionId = Guid.NewGuid().ToString();
            }

            foreach (var item in transaction.Items)
            {
                if (string.IsNullOrEmpty(item.ItemInTransactionId))
                {
                    item.ItemInTransactionId = Guid.NewGuid().ToString();
                }
                item.TransactionId = transaction.PointTransactionId;
            }
        }

        _context.PointTransactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        return transactions;
    }
}
