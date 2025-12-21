namespace HrManagement.Api.Modules.Reward.Domain.Commands;

/// <summary>
/// Represents a recipient with specified points to gift.
/// </summary>
public record GiftRecipient(
    string UserId,
    int Points
);

/// <summary>
/// Command for gifting points from manager to employees.
/// Each recipient can receive different point amounts.
/// </summary>
public record GiftPointsCommand(
    string ProgramId,
    string SenderUserId,
    List<GiftRecipient> Recipients
);
