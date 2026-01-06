using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Modules.Reward.Application.DTOs;

/// <summary>
/// Response DTO for a user's wallet in a reward program.
/// </summary>
public record UserWalletResponse
{
    public string UserWalletId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string ProgramId { get; init; } = string.Empty;
    public int PersonalPoint { get; init; }
    public int GivingBudget { get; init; }
    public RewardProgramResponse? Program { get; init; }

    /// <summary>
    /// Creates a response from a UserWallet entity.
    /// </summary>
    public static UserWalletResponse FromEntity(UserWallet entity)
    {
        return new UserWalletResponse
        {
            UserWalletId = entity.UserWalletId,
            UserId = entity.UserId,
            UserName = entity.UserName,
            ProgramId = entity.ProgramId,
            PersonalPoint = entity.PersonalPoint,
            GivingBudget = entity.GivingBudget,
            Program = entity.Program != null ? RewardProgramResponse.FromEntity(entity.Program) : null
        };
    }
}

/// <summary>
/// Simplified response DTO for wallet balance only.
/// </summary>
public record WalletBalanceResponse
{
    public int PersonalPoint { get; init; }
    public int GivingBudget { get; init; }

    /// <summary>
    /// Creates a balance response from a UserWallet entity.
    /// </summary>
    public static WalletBalanceResponse FromEntity(UserWallet entity)
    {
        return new WalletBalanceResponse
        {
            PersonalPoint = entity.PersonalPoint,
            GivingBudget = entity.GivingBudget
        };
    }
}
