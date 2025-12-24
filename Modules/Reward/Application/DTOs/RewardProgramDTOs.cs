using System.ComponentModel.DataAnnotations;
using HrManagement.Api.Modules.Reward.Domain.Entities;
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

    /// <summary>
    /// Converts this request to a RewardProgram entity.
    /// </summary>
    public RewardProgram ToEntity()
    {
        return new RewardProgram
        {
            Name = Name,
            Description = Description,
            StartDate = StartDate,
            EndDate = EndDate,
            DefaultGivingBudget = DefaultGivingBudget,
            BannerUrl = BannerUrl,
            RewardItems = Items.Select(i => i.ToEntity()).ToList(),
            Policies = Policies.Select(p => p.ToEntity()).ToList()
        };
    }
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

    /// <summary>
    /// Converts this request to a RewardItem entity.
    /// </summary>
    public RewardItem ToEntity()
    {
        return new RewardItem
        {
            Name = Name,
            RequiredPoints = RequiredPoints,
            Quantity = Quantity,
            ImageUrl = ImageUrl
        };
    }
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

    /// <summary>
    /// Converts this request to a RewardProgramPolicy entity.
    /// </summary>
    public RewardProgramPolicy ToEntity()
    {
        return new RewardProgramPolicy
        {
            PolicyType = PolicyType,
            UnitValue = UnitValue,
            PointsPerUnit = PointsPerUnit
        };
    }
}

/// <summary>
/// Request DTO for updating an existing reward program.
/// All fields are optional - only provided fields will be updated.
/// </summary>
public record UpdateRewardProgramRequest
{
    /// <summary>
    /// New name of the reward program.
    /// </summary>
    /// <example>Q4 2024 Employee Recognition - Updated</example>
    [StringLength(255)]
    public string? Name { get; init; }

    /// <summary>
    /// New description of the reward program.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// New start date of the program.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// New end date of the program.
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// New default points budget for managers.
    /// </summary>
    public int? DefaultGivingBudget { get; init; }

    /// <summary>
    /// New banner URL.
    /// </summary>
    [StringLength(500)]
    public string? BannerUrl { get; init; }

    /// <summary>
    /// Updated list of reward items. Required - replaces existing items.
    /// </summary>
    [Required]
    public List<UpdateRewardItemRequest> Items { get; init; } = new();

    /// <summary>
    /// Updated list of policies.
    /// </summary>
    public List<UpdateRewardPolicyRequest> Policies { get; init; } = new();

    /// <summary>
    /// Converts this update request to a RewardProgram entity.
    /// </summary>
    public RewardProgram ToEntity(string programId)
    {
        return new RewardProgram
        {
            RewardProgramId = programId,
            Name = Name ?? string.Empty,
            Description = Description,
            StartDate = StartDate ?? DateTime.UtcNow,
            EndDate = EndDate ?? DateTime.UtcNow.AddMonths(3),
            DefaultGivingBudget = DefaultGivingBudget ?? 100,
            BannerUrl = BannerUrl,
            RewardItems = Items.Select(i => i.ToEntity(programId)).ToList(),
            Policies = Policies.Select(p => p.ToEntity(programId)).ToList()
        };
    }
}

/// <summary>
/// Request DTO for updating a reward item.
/// </summary>
public record UpdateRewardItemRequest
{
    /// <summary>
    /// ID of existing item to update. If null, creates a new item.
    /// </summary>
    public string? RewardItemId { get; init; }

    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [Range(1, float.MaxValue)]
    public float RequiredPoints { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; init; }

    [StringLength(500)]
    public string? ImageUrl { get; init; }

    public RewardItem ToEntity(string programId)
    {
        return new RewardItem
        {
            RewardItemId = RewardItemId ?? Guid.NewGuid().ToString(),
            ProgramId = programId,
            Name = Name,
            RequiredPoints = RequiredPoints,
            Quantity = Quantity,
            ImageUrl = ImageUrl
        };
    }
}

/// <summary>
/// Request DTO for updating a reward policy.
/// </summary>
public record UpdateRewardPolicyRequest
{
    /// <summary>
    /// ID of existing policy to update. If null, creates a new policy.
    /// </summary>
    public string? PolicyId { get; init; }

    [Required]
    public PolicyType PolicyType { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int UnitValue { get; init; } = 1;

    [Required]
    [Range(1, int.MaxValue)]
    public int PointsPerUnit { get; init; }

    public RewardProgramPolicy ToEntity(string programId)
    {
        return new RewardProgramPolicy
        {
            PolicyId = PolicyId ?? Guid.NewGuid().ToString(),
            ProgramId = programId,
            PolicyType = PolicyType,
            UnitValue = UnitValue,
            PointsPerUnit = PointsPerUnit
        };
    }
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for a reward program (list view).
/// </summary>
public record RewardProgramResponse
{
    public string RewardProgramId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public ProgramStatus Status { get; init; }
    public int DefaultGivingBudget { get; init; }
    public string? BannerUrl { get; init; }

    /// <summary>
    /// Creates a response from a RewardProgram entity.
    /// </summary>
    public static RewardProgramResponse FromEntity(RewardProgram entity)
    {
        return new RewardProgramResponse
        {
            RewardProgramId = entity.RewardProgramId,
            Name = entity.Name,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            DefaultGivingBudget = entity.DefaultGivingBudget,
            BannerUrl = entity.BannerUrl
        };
    }
}

/// <summary>
/// Response DTO for reward program detail view with items and policies.
/// </summary>
public record RewardProgramDetailResponse : RewardProgramResponse
{
    public List<RewardItemResponse> Items { get; init; } = new();
    public List<RewardPolicyResponse> Policies { get; init; } = new();

    /// <summary>
    /// Creates a detail response from a RewardProgram entity with items and policies.
    /// </summary>
    public static new RewardProgramDetailResponse FromEntity(RewardProgram entity)
    {
        return new RewardProgramDetailResponse
        {
            RewardProgramId = entity.RewardProgramId,
            Name = entity.Name,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            DefaultGivingBudget = entity.DefaultGivingBudget,
            BannerUrl = entity.BannerUrl,
            Items = entity.RewardItems.Select(RewardItemResponse.FromEntity).ToList(),
            Policies = entity.Policies.Select(RewardPolicyResponse.FromEntity).ToList()
        };
    }
}

/// <summary>
/// Response DTO for a reward item.
/// </summary>
public record RewardItemResponse
{
    public string RewardItemId { get; init; } = string.Empty;
    public string ProgramId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public float RequiredPoints { get; init; }
    public int Quantity { get; init; }
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Creates a response from a RewardItem entity.
    /// </summary>
    public static RewardItemResponse FromEntity(RewardItem entity)
    {
        return new RewardItemResponse
        {
            RewardItemId = entity.RewardItemId,
            ProgramId = entity.ProgramId,
            Name = entity.Name,
            RequiredPoints = entity.RequiredPoints,
            Quantity = entity.Quantity,
            ImageUrl = entity.ImageUrl
        };
    }
}

/// <summary>
/// Response DTO for a reward policy.
/// </summary>
public record RewardPolicyResponse
{
    public string PolicyId { get; init; } = string.Empty;
    public string ProgramId { get; init; } = string.Empty;
    public PolicyType PolicyType { get; init; }
    public CalculationPeriod CalculationPeriod { get; init; }
    public int UnitValue { get; init; }
    public int PointsPerUnit { get; init; }
    public bool IsActive { get; init; }

    /// <summary>
    /// Creates a response from a RewardProgramPolicy entity.
    /// </summary>
    public static RewardPolicyResponse FromEntity(RewardProgramPolicy entity)
    {
        return new RewardPolicyResponse
        {
            PolicyId = entity.PolicyId,
            ProgramId = entity.ProgramId,
            PolicyType = entity.PolicyType,
            CalculationPeriod = entity.CalculationPeriod,
            UnitValue = entity.UnitValue,
            PointsPerUnit = entity.PointsPerUnit,
            IsActive = entity.IsActive
        };
    }
}

#endregion
