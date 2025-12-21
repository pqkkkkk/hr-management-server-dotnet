namespace HrManagement.Api.Modules.Reward.Application.DTOs;

/// <summary>
/// Response DTO for a user's wallet in a reward program.
/// </summary>
public record UserWalletResponse
{
    /// <summary>
    /// Unique identifier of the wallet.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string UserWalletId { get; init; } = string.Empty;

    /// <summary>
    /// User ID who owns this wallet.
    /// </summary>
    /// <example>user-123</example>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// ID of the reward program this wallet is for.
    /// </summary>
    /// <example>program-456</example>
    public string ProgramId { get; init; } = string.Empty;

    /// <summary>
    /// Personal points balance that can be exchanged for items.
    /// </summary>
    /// <example>150</example>
    public int PersonalPoint { get; init; }

    /// <summary>
    /// Budget available for gifting points to employees (managers only).
    /// </summary>
    /// <example>100</example>
    public int GivingBudget { get; init; }

    /// <summary>
    /// Associated reward program information.
    /// </summary>
    public RewardProgramResponse? Program { get; init; }
}

/// <summary>
/// Simplified response DTO for wallet balance only.
/// </summary>
public record WalletBalanceResponse
{
    /// <summary>
    /// Personal points balance.
    /// </summary>
    /// <example>150</example>
    public int PersonalPoint { get; init; }

    /// <summary>
    /// Giving budget balance (for managers).
    /// </summary>
    /// <example>100</example>
    public int GivingBudget { get; init; }
}
