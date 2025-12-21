using HrManagement.Api.Modules.Reward.Domain.Commands;
using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;

public interface IPointTransactionCommandService
{
    /// <summary>
    /// Manager gifts points to employees.
    /// Deducts from sender's giving_budget, adds to recipients' personal_point.
    /// </summary>
    Task<List<PointTransaction>> GiftPointsToEmployeesAsync(GiftPointsCommand command);

    /// <summary>
    /// Employee exchanges points for reward items.
    /// Entity should have Items navigation property set with RewardItem and Quantity.
    /// Validates balance and stock, then creates transaction.
    /// </summary>
    Task<PointTransaction> ExchangePointsAsync(PointTransaction transaction);

    /// <summary>
    /// Cron job: auto distribute points based on reward program policies (WEEKLY).
    /// </summary>
    Task ProcessRewardProgramPolicyAsync();
}
