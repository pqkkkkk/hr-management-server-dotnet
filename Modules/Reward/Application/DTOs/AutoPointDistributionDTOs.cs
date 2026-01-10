using System.ComponentModel.DataAnnotations;

namespace HrManagement.Api.Modules.Reward.Application.DTOs;

/// <summary>
/// DTOs for Auto Point Distribution API
/// </summary>

#region Request DTOs

/// <summary>
/// Request body for triggering auto point distribution.
/// </summary>
public class DistributePointsRequest
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// API response for auto point distribution operation.
/// </summary>
public class AutoPointDistributionResponse
{
    public int TotalUsersProcessed { get; set; }
    public int TotalPointsDistributed { get; set; }
    public int TotalTransactionsCreated { get; set; }
    public List<UserPointSummaryResponse> UserSummaries { get; set; } = new();
}

public class UserPointSummaryResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public Dictionary<string, int> PointsByPolicy { get; set; } = new();
}

#endregion
