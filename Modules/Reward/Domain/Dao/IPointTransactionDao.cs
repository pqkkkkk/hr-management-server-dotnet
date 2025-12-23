using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Domain.Dao;

/// <summary>
/// Data access interface for PointTransaction entity.
/// </summary>
public interface IPointTransactionDao
{
    /// <summary>
    /// Gets a transaction by ID with related entities (Items, Wallets).
    /// </summary>
    Task<PointTransaction?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all transactions with filtering and pagination.
    /// </summary>
    Task<PagedResult<PointTransaction>> GetAllAsync(PointTransactionFilter filter);

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    Task<PointTransaction> CreateAsync(PointTransaction transaction);

    /// <summary>
    /// Creates multiple transactions in batch.
    /// Used for gift points to multiple recipients.
    /// </summary>
    Task<List<PointTransaction>> CreateBatchAsync(List<PointTransaction> transactions);
}
