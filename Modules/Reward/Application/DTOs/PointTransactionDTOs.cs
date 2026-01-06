using System.ComponentModel.DataAnnotations;
using HrManagement.Api.Modules.Reward.Domain.Commands;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request DTO for gifting points from manager to employees.
/// </summary>
public record GiftPointsRequest
{
    /// <summary>
    /// ID of the reward program.
    /// </summary>
    /// <example>program-123</example>
    [Required]
    public string ProgramId { get; init; } = string.Empty;

    /// <summary>
    /// User ID of the manager sending points.
    /// </summary>
    /// <example>manager-456</example>
    [Required]
    public string SenderUserId { get; init; } = string.Empty;

    /// <summary>
    /// List of recipients with their respective point amounts.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<GiftRecipientRequest> Recipients { get; init; } = new();

    /// <summary>
    /// Converts this request to a GiftPointsCommand.
    /// </summary>
    public GiftPointsCommand ToCommand()
    {
        return new GiftPointsCommand(
            ProgramId,
            SenderUserId,
            Recipients.Select(r => r.ToGiftRecipient()).ToList()
        );
    }
}

/// <summary>
/// DTO for a gift recipient with point amount.
/// </summary>
public record GiftRecipientRequest
{
    /// <summary>
    /// User ID of the employee receiving points.
    /// </summary>
    /// <example>employee-789</example>
    [Required]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Number of points to gift.
    /// </summary>
    /// <example>10</example>
    [Required]
    [Range(1, int.MaxValue)]
    public int Points { get; init; }

    /// <summary>
    /// Converts this request to a GiftRecipient record.
    /// </summary>
    public GiftRecipient ToGiftRecipient()
    {
        return new GiftRecipient(UserId, Points);
    }
}

/// <summary>
/// Request DTO for exchanging points for reward items.
/// </summary>
public record ExchangePointsRequest
{
    /// <summary>
    /// ID of the reward program.
    /// </summary>
    /// <example>program-123</example>
    [Required]
    public string ProgramId { get; init; } = string.Empty;

    /// <summary>
    /// User wallet ID of the employee exchanging points.
    /// </summary>
    /// <example>wallet-456</example>
    [Required]
    public string UserWalletId { get; init; } = string.Empty;

    /// <summary>
    /// List of items to exchange with quantities.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<ExchangeItemRequest> Items { get; init; } = new();

    /// <summary>
    /// Converts this request to a PointTransaction entity.
    /// Note: Requires walletId to be looked up separately.
    /// </summary>
    public PointTransaction ToEntity()
    {
        return new PointTransaction
        {
            Type = TransactionType.EXCHANGE,
            DestinationWalletId = UserWalletId,
            SourceWalletId = UserWalletId,
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }
}

/// <summary>
/// DTO for an item to exchange with quantity.
/// </summary>
public record ExchangeItemRequest
{
    /// <summary>
    /// ID of the reward item to exchange.
    /// </summary>
    /// <example>item-123</example>
    [Required]
    public string RewardItemId { get; init; } = string.Empty;

    /// <summary>
    /// Quantity of the item to exchange.
    /// </summary>
    /// <example>2</example>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    /// <summary>
    /// Converts this request to an ItemInTransaction entity.
    /// </summary>
    public ItemInTransaction ToEntity()
    {
        return new ItemInTransaction
        {
            RewardItemId = RewardItemId,
            Quantity = Quantity
        };
    }
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for a point transaction.
/// </summary>
public record PointTransactionResponse
{
    public string PointTransactionId { get; init; } = string.Empty;
    public TransactionType Type { get; init; }
    public float Amount { get; init; }
    public string? SourceWalletId { get; init; }
    public string? DestinationWalletId { get; init; }
    public string? SourceUsername { get; init; }
    public string? DestinationUsername { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Creates a response from a PointTransaction entity.
    /// </summary>
    public static PointTransactionResponse FromEntity(PointTransaction entity)
    {
        return new PointTransactionResponse
        {
            PointTransactionId = entity.PointTransactionId,
            Type = entity.Type,
            Amount = entity.Amount,
            SourceWalletId = entity.SourceWalletId,
            DestinationWalletId = entity.DestinationWalletId,
            SourceUsername = entity.SourceWallet?.UserName,
            DestinationUsername = entity.DestinationWallet?.UserName,
            CreatedAt = entity.CreatedAt
        };
    }
}

/// <summary>
/// Response DTO for point transaction with item details (for EXCHANGE transactions).
/// </summary>
public record PointTransactionDetailResponse : PointTransactionResponse
{
    public List<ItemInTransactionResponse> Items { get; init; } = new();

    /// <summary>
    /// Creates a detail response from a PointTransaction entity with items.
    /// </summary>
    public static new PointTransactionDetailResponse FromEntity(PointTransaction entity)
    {
        return new PointTransactionDetailResponse
        {
            PointTransactionId = entity.PointTransactionId,
            Type = entity.Type,
            Amount = entity.Amount,
            SourceWalletId = entity.SourceWalletId,
            DestinationWalletId = entity.DestinationWalletId,
            SourceUsername = entity.SourceWallet?.UserName,
            DestinationUsername = entity.DestinationWallet?.UserName,
            CreatedAt = entity.CreatedAt,
            Items = entity.Items.Select(ItemInTransactionResponse.FromEntity).ToList()
        };
    }
}

/// <summary>
/// Response DTO for an item in a transaction.
/// </summary>
public record ItemInTransactionResponse
{
    public string RewardItemId { get; init; } = string.Empty;
    public string RewardItemName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int TotalPoints { get; init; }

    /// <summary>
    /// Creates a response from an ItemInTransaction entity.
    /// </summary>
    public static ItemInTransactionResponse FromEntity(ItemInTransaction entity)
    {
        return new ItemInTransactionResponse
        {
            RewardItemId = entity.RewardItemId,
            RewardItemName = entity.RewardItem?.Name ?? string.Empty,
            Quantity = entity.Quantity,
            TotalPoints = entity.TotalPoints
        };
    }
}

#endregion
