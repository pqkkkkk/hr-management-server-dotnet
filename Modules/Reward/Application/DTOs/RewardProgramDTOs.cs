using System.ComponentModel.DataAnnotations;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request DTO for creating a new reward program with items and policies.
/// </summary>
public record CreateRewardProgramRequest
{
    /// <summary>
    /// Name of the reward program.
    /// </summary>
    /// <example>Q4 2024 Employee Recognition</example>
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the reward program.
    /// </summary>
    /// <example>Quarterly reward program to recognize outstanding employees</example>
    public string? Description { get; init; }

    /// <summary>
    /// Start date of the program.
    /// </summary>
    /// <example>2024-10-01</example>
    [Required]
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the program.
    /// </summary>
    /// <example>2024-12-31</example>
    [Required]
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Default points budget allocated to managers for gifting.
    /// </summary>
    /// <example>100</example>
    public int DefaultGivingBudget { get; init; } = 100;

    /// <summary>
    /// URL to the program banner image.
    /// </summary>
    /// <example>https://example.com/banner.jpg</example>
    [StringLength(500)]
    public string? BannerUrl { get; init; }

    /// <summary>
    /// List of reward items available in this program.
    /// </summary>
    public List<CreateRewardItemRequest> Items { get; init; } = new();

    /// <summary>
    /// List of policies for automatic point distribution.
    /// </summary>
    public List<CreateRewardPolicyRequest> Policies { get; init; } = new();
}

/// <summary>
/// Request DTO for creating a reward item within a program.
/// </summary>
public record CreateRewardItemRequest
{
    /// <summary>
    /// Name of the reward item.
    /// </summary>
    /// <example>Amazon Gift Card $50</example>
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Points required to redeem this item.
    /// </summary>
    /// <example>500</example>
    [Required]
    [Range(1, float.MaxValue)]
    public float RequiredPoints { get; init; }

    /// <summary>
    /// Available quantity of this item.
    /// </summary>
    /// <example>10</example>
    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; init; }

    /// <summary>
    /// URL to the item image.
    /// </summary>
    /// <example>https://example.com/gift-card.jpg</example>
    [StringLength(500)]
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Request DTO for creating a reward policy.
/// </summary>
public record CreateRewardPolicyRequest
{
    /// <summary>
    /// Type of policy: NOT_LATE, OVERTIME, FULL_ATTENDANCE.
    /// </summary>
    /// <example>OVERTIME</example>
    [Required]
    public PolicyType PolicyType { get; init; }

    /// <summary>
    /// The unit value for calculation (e.g., 30 minutes for overtime).
    /// </summary>
    /// <example>30</example>
    [Required]
    [Range(1, int.MaxValue)]
    public int UnitValue { get; init; } = 1;

    /// <summary>
    /// Points awarded per unit achieved.
    /// </summary>
    /// <example>5</example>
    [Required]
    [Range(1, int.MaxValue)]
    public int PointsPerUnit { get; init; }
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for a reward program (list view).
/// </summary>
public record RewardProgramResponse
{
    /// <summary>
    /// Unique identifier of the reward program.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string RewardProgramId { get; init; } = string.Empty;

    /// <summary>
    /// Name of the reward program.
    /// </summary>
    /// <example>Q4 2024 Employee Recognition</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of the reward program.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Start date of the program.
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the program.
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Status of the program: ACTIVE or INACTIVE.
    /// </summary>
    public ProgramStatus Status { get; init; }

    /// <summary>
    /// Default giving budget for managers.
    /// </summary>
    public int DefaultGivingBudget { get; init; }

    /// <summary>
    /// URL to the program banner.
    /// </summary>
    public string? BannerUrl { get; init; }
}

/// <summary>
/// Response DTO for reward program detail view with items and policies.
/// </summary>
public record RewardProgramDetailResponse : RewardProgramResponse
{
    /// <summary>
    /// List of reward items in this program.
    /// </summary>
    public List<RewardItemResponse> Items { get; init; } = new();

    /// <summary>
    /// List of policies for this program.
    /// </summary>
    public List<RewardPolicyResponse> Policies { get; init; } = new();
}

/// <summary>
/// Response DTO for a reward item.
/// </summary>
public record RewardItemResponse
{
    /// <summary>
    /// Unique identifier of the reward item.
    /// </summary>
    public string RewardItemId { get; init; } = string.Empty;

    /// <summary>
    /// ID of the program this item belongs to.
    /// </summary>
    public string ProgramId { get; init; } = string.Empty;

    /// <summary>
    /// Name of the reward item.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Points required to redeem this item.
    /// </summary>
    public float RequiredPoints { get; init; }

    /// <summary>
    /// Available quantity of this item.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// URL to the item image.
    /// </summary>
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Response DTO for a reward policy.
/// </summary>
public record RewardPolicyResponse
{
    /// <summary>
    /// Unique identifier of the policy.
    /// </summary>
    public string PolicyId { get; init; } = string.Empty;

    /// <summary>
    /// ID of the program this policy belongs to.
    /// </summary>
    public string ProgramId { get; init; } = string.Empty;

    /// <summary>
    /// Type of policy.
    /// </summary>
    public PolicyType PolicyType { get; init; }

    /// <summary>
    /// Calculation period (currently always WEEKLY).
    /// </summary>
    public CalculationPeriod CalculationPeriod { get; init; }

    /// <summary>
    /// The unit value for calculation.
    /// </summary>
    public int UnitValue { get; init; }

    /// <summary>
    /// Points awarded per unit.
    /// </summary>
    public int PointsPerUnit { get; init; }

    /// <summary>
    /// Whether this policy is active.
    /// </summary>
    public bool IsActive { get; init; }
}

#endregion
