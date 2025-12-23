using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;
using HrManagement.Api.Shared.DTOs;

namespace HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;

/// <summary>
/// Implementation of IPointTransactionQueryService.
/// Handles read operations for point transactions.
/// </summary>
public class PointTransactionQueryServiceImpl : IPointTransactionQueryService
{
    private readonly IPointTransactionDao _pointTransactionDao;

    public PointTransactionQueryServiceImpl(IPointTransactionDao pointTransactionDao)
    {
        _pointTransactionDao = pointTransactionDao;
    }

    /// <summary>
    /// Gets a transaction by ID with its items.
    /// </summary>
    public async Task<PointTransaction?> GetPointTransactionByIdAsync(string pointTransactionId)
    {
        return await _pointTransactionDao.GetByIdAsync(pointTransactionId);
    }

    /// <summary>
    /// Gets all transactions with filtering and pagination.
    /// </summary>
    public async Task<PagedResult<PointTransaction>> GetPointTransactionsAsync(PointTransactionFilter filter)
    {
        return await _pointTransactionDao.GetAllAsync(filter);
    }
}
