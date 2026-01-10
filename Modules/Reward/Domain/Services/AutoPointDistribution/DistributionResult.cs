namespace HrManagement.Api.Modules.Reward.Domain.Services.AutoPointDistribution;

using HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Domain result for auto point distribution operation.
/// Contains the outcome of distributing points to users based on policies.
/// </summary>
public record DistributionResult(
    int TotalUsersProcessed,
    int TotalPointsDistributed,
    int TotalTransactionsCreated,
    List<UserDistributionSummary> UserSummaries
);

/// <summary>
/// Summary of points distributed to a single user.
/// </summary>
public record UserDistributionSummary(
    string UserId,
    string UserName,
    int PointsEarned,
    Dictionary<string, int> PointsByPolicy  // PolicyType name -> points
);
