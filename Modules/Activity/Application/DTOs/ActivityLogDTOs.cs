using System.ComponentModel.DataAnnotations;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request DTO for submitting an activity log.
/// </summary>
public record SubmitActivityLogRequest
{
    /// <summary>
    /// ID of the activity this log belongs to.
    /// </summary>
    [Required]
    public string ActivityId { get; init; } = string.Empty;

    /// <summary>
    /// Employee ID of the participant.
    /// </summary>
    [Required]
    public string EmployeeId { get; init; } = string.Empty;

    /// <summary>
    /// Distance covered in kilometers.
    /// </summary>
    /// <example>5.5</example>
    [Required]
    [Range(0.01, 1000)]
    public decimal Distance { get; init; }

    /// <summary>
    /// Duration in minutes.
    /// </summary>
    /// <example>30</example>
    [Required]
    [Range(1, 1440)]
    public int DurationMinutes { get; init; }

    /// <summary>
    /// URL to proof screenshot/image.
    /// </summary>
    [StringLength(500)]
    public string? ProofUrl { get; init; }

    /// <summary>
    /// Date of the activity.
    /// </summary>
    /// <example>2024-01-15</example>
    [Required]
    public DateTime LogDate { get; init; }

    /// <summary>
    /// Converts this request to an ActivityLog entity.
    /// Note: ParticipantId must be set separately after looking up the participant.
    /// </summary>
    public ActivityLog ToEntity(string participantId)
    {
        return new ActivityLog
        {
            ParticipantId = participantId,
            Distance = Distance,
            DurationMinutes = DurationMinutes,
            ProofUrl = ProofUrl,
            LogDate = LogDate,
            Status = ActivityLogStatus.PENDING
        };
    }
}

/// <summary>
/// Request DTO for rejecting an activity log.
/// </summary>
public record RejectActivityLogRequest
{
    /// <summary>
    /// Reason for rejection.
    /// </summary>
    /// <example>Invalid proof image - screenshot shows different date</example>
    [Required]
    [StringLength(500)]
    public string Reason { get; init; } = string.Empty;
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for an activity log.
/// </summary>
public record ActivityLogResponse
{
    public string ActivityLogId { get; init; } = string.Empty;
    public string ParticipantId { get; init; } = string.Empty;
    public string? EmployeeId { get; init; }
    public string? ActivityId { get; init; }
    public decimal Distance { get; init; }
    public int DurationMinutes { get; init; }
    public string? ProofUrl { get; init; }
    public DateTime LogDate { get; init; }
    public ActivityLogStatus Status { get; init; }
    public string? RejectReason { get; init; }
    public string? ReviewerId { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Calculated pace in minutes per kilometer.
    /// </summary>
    public decimal? PaceMinPerKm => Distance > 0 ? Math.Round(DurationMinutes / Distance, 2) : null;

    /// <summary>
    /// Creates a response from an ActivityLog entity.
    /// </summary>
    public static ActivityLogResponse FromEntity(ActivityLog entity)
    {
        return new ActivityLogResponse
        {
            ActivityLogId = entity.ActivityLogId,
            ParticipantId = entity.ParticipantId,
            EmployeeId = entity.Participant?.EmployeeId,
            ActivityId = entity.Participant?.ActivityId,
            Distance = entity.Distance,
            DurationMinutes = entity.DurationMinutes,
            ProofUrl = entity.ProofUrl,
            LogDate = entity.LogDate,
            Status = entity.Status,
            RejectReason = entity.RejectReason,
            ReviewerId = entity.ReviewerId,
            CreatedAt = entity.CreatedAt
        };
    }
}

#endregion
