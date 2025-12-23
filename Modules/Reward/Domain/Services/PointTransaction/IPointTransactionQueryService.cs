using System;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Shared.DTOs;
namespace HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;

public interface IPointTransactionQueryService
{
    public Task<PointTransaction?> GetPointTransactionByIdAsync(string pointTransactionId);
    public Task<PagedResult<PointTransaction>> GetPointTransactionsAsync(PointTransactionFilter filter);
}
