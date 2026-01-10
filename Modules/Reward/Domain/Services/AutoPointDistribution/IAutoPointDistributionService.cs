namespace HrManagement.Api.Modules.Reward.Domain.Services.AutoPointDistribution;

/// <summary>
/// Service interface for automatic point distribution based on reward program policies.
/// </summary>
public interface IAutoPointDistributionService
{
    /// <summary>
    /// Distributes points to all users in a reward program based on their timesheet data
    /// and the program's active policies.
    /// </summary>
    /// <param name="programId">The reward program ID</param>
    /// <param name="startDate">Start date of the period to evaluate</param>
    /// <param name="endDate">End date of the period to evaluate</param>
    /// <returns>Result containing distribution summary</returns>
    Task<DistributionResult> DistributePointsAsync(
        string programId,
        DateTime startDate,
        DateTime endDate);
}
