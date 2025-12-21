using System.ComponentModel.DataAnnotations;
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
    /// User ID of the employee exchanging points.
    /// </summary>
    /// <example>employee-456</example>
    [Required]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// List of items to exchange with quantities.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<ExchangeItemRequest> Items { get; init; } = new();
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
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for a point transaction.
/// </summary>
public record PointTransactionResponse
{
    /// <summary>
    /// Unique identifier of the transaction.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public string PointTransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Type of transaction: GIFT, EXCHANGE, or POLICY_REWARD.
    /// </summary>
    public TransactionType Type { get; init; }

    /// <summary>
    /// Amount of points transferred.
    /// </summary>
    /// <example>50</example>
    public float Amount { get; init; }

    /// <summary>
    /// ID of the source wallet (for GIFT transactions).
    /// </summary>
    public string? SourceWalletId { get; init; }

    /// <summary>
    /// ID of the destination wallet.
    /// </summary>
    public string? DestinationWalletId { get; init; }

    /// <summary>
    /// Timestamp when the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Response DTO for point transaction with item details (for EXCHANGE transactions).
/// </summary>
public record PointTransactionDetailResponse : PointTransactionResponse
{
    /// <summary>
    /// List of items involved in this transaction (for EXCHANGE type).
    /// </summary>
    public List<ItemInTransactionResponse> Items { get; init; } = new();
}

/// <summary>
/// Response DTO for an item in a transaction.
/// </summary>
public record ItemInTransactionResponse
{
    /// <summary>
    /// ID of the reward item.
    /// </summary>
    public string RewardItemId { get; init; } = string.Empty;

    /// <summary>
    /// Name of the reward item.
    /// </summary>
    public string RewardItemName { get; init; } = string.Empty;

    /// <summary>
    /// Quantity of items exchanged.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Total points spent for these items.
    /// </summary>
    public int TotalPoints { get; init; }
}

#endregion
