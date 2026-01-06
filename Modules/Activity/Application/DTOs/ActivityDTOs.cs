using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request DTO for creating a new activity.
/// </summary>
public record CreateActivityRequest
{
    /// <summary>
    /// Name of the activity.
    /// </summary>
    /// <example>Marathon Challenge 2024</example>
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Template ID for the activity (e.g., "RUNNING_SIMPLE").
    /// </summary>
    /// <example>RUNNING_SIMPLE</example>
    [Required]
    [StringLength(100)]
    public string TemplateId { get; init; } = string.Empty;

    /// <summary>
    /// Activity type.
    /// </summary>
    /// <example>RUNNING</example>
    public ActivityType Type { get; init; } = ActivityType.RUNNING;

    /// <summary>
    /// HTML content for activity description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// URL to the activity banner image.
    /// </summary>
    [StringLength(500)]
    public string? BannerUrl { get; init; }

    /// <summary>
    /// Start date of the activity.
    /// </summary>
    /// <example>2024-01-01</example>
    [Required]
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the activity.
    /// </summary>
    /// <example>2024-03-31</example>
    [Required]
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Configuration values for the template (e.g., min_pace, points_per_km).
    /// </summary>
    public Dictionary<string, object>? Config { get; init; }

    /// <summary>
    /// Converts this request to an Activity entity.
    /// </summary>
    public Domain.Entities.Activity ToEntity()
    {
        return new Domain.Entities.Activity
        {
            Name = Name,
            TemplateId = TemplateId,
            Type = Type,
            Description = Description,
            BannerUrl = BannerUrl,
            StartDate = StartDate,
            EndDate = EndDate,
            Config = Config != null ? JsonDocument.Parse(JsonSerializer.Serialize(Config)) : null,
            Status = ActivityStatus.DRAFT
        };
    }
}

/// <summary>
/// Request DTO for updating an existing activity.
/// </summary>
public record UpdateActivityRequest
{
    [StringLength(255)]
    public string? Name { get; init; }

    [StringLength(100)]
    public string? TemplateId { get; init; }

    public ActivityType? Type { get; init; }

    public string? Description { get; init; }

    [StringLength(500)]
    public string? BannerUrl { get; init; }

    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public Dictionary<string, object>? Config { get; init; }

    /// <summary>
    /// Converts this update request to an Activity entity.
    /// </summary>
    public Domain.Entities.Activity ToEntity(string activityId)
    {
        return new Domain.Entities.Activity
        {
            ActivityId = activityId,
            Name = Name ?? string.Empty,
            TemplateId = TemplateId ?? string.Empty,
            Type = Type ?? ActivityType.RUNNING,
            Description = Description,
            BannerUrl = BannerUrl,
            StartDate = StartDate ?? DateTime.UtcNow,
            EndDate = EndDate ?? DateTime.UtcNow.AddMonths(3),
            Config = Config != null ? JsonDocument.Parse(JsonSerializer.Serialize(Config)) : null
        };
    }
}

/// <summary>
/// Request DTO for updating activity status.
/// </summary>
public record UpdateActivityStatusRequest
{
    /// <summary>
    /// New status for the activity.
    /// </summary>
    [Required]
    public ActivityStatus Status { get; init; }
}

/// <summary>
/// Request DTO for registering a participant.
/// </summary>
public record RegisterParticipantRequest
{
    /// <summary>
    /// Employee ID to register.
    /// </summary>
    [Required]
    public string EmployeeId { get; init; } = string.Empty;

    /// <summary>
    /// Employee name for display purposes.
    /// </summary>
    [StringLength(255)]
    public string EmployeeName { get; init; } = string.Empty;
}

#endregion

#region Response DTOs

/// <summary>
/// Response DTO for activity list view.
/// </summary>
public record ActivityResponse
{
    public string ActivityId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ActivityType Type { get; init; }
    public string TemplateId { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? BannerUrl { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public ActivityStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Number of participants in this activity.
    /// </summary>
    public int ParticipantsCount { get; init; }

    /// <summary>
    /// Whether the current user is registered for this activity.
    /// </summary>
    public bool IsRegistered { get; init; }

    /// <summary>
    /// Creates a response from an Activity entity.
    /// </summary>
    public static ActivityResponse FromEntity(Domain.Entities.Activity entity, string? employeeId = null)
    {
        return new ActivityResponse
        {
            ActivityId = entity.ActivityId,
            Name = entity.Name,
            Type = entity.Type,
            TemplateId = entity.TemplateId,
            Description = entity.Description,
            BannerUrl = entity.BannerUrl,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ParticipantsCount = entity.ParticipantsCount,
            IsRegistered = !string.IsNullOrEmpty(employeeId) &&
                           entity.Participants?.Any(p => p.EmployeeId == employeeId) == true
        };
    }
}

/// <summary>
/// Response DTO for activity detail view.
/// </summary>
public record ActivityDetailResponse : ActivityResponse
{
    public Dictionary<string, object>? Config { get; init; }

    /// <summary>
    /// Creates a detail response from an Activity entity.
    /// </summary>
    public static new ActivityDetailResponse FromEntity(Domain.Entities.Activity entity, string? employeeId = null)
    {
        Dictionary<string, object>? configDict = null;
        if (entity.Config != null)
        {
            configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Config.RootElement.GetRawText());
        }

        return new ActivityDetailResponse
        {
            ActivityId = entity.ActivityId,
            Name = entity.Name,
            Type = entity.Type,
            TemplateId = entity.TemplateId,
            Description = entity.Description,
            BannerUrl = entity.BannerUrl,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            Config = configDict,
            ParticipantsCount = entity.ParticipantsCount,
            IsRegistered = !string.IsNullOrEmpty(employeeId) &&
                           entity.Participants?.Any(p => p.EmployeeId == employeeId) == true
        };
    }
}

/// <summary>
/// Response DTO for participant information.
/// </summary>
public record ParticipantResponse
{
    public string ParticipantId { get; init; } = string.Empty;
    public string ActivityId { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public ParticipantStatus Status { get; init; }
    public decimal TotalScore { get; init; }

    /// <summary>
    /// Creates a response from a Participant entity.
    /// </summary>
    public static ParticipantResponse FromEntity(Participant entity)
    {
        return new ParticipantResponse
        {
            ParticipantId = entity.ParticipantId,
            ActivityId = entity.ActivityId,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.EmployeeName,
            JoinedAt = entity.JoinedAt,
            Status = entity.Status,
            TotalScore = entity.TotalScore
        };
    }
}

/// <summary>
/// Response DTO for participant statistics.
/// </summary>
public record ParticipantStatsResponse
{
    public string ParticipantId { get; init; } = string.Empty;
    public string ActivityId { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    public ParticipantStatus Status { get; init; }
    public decimal TotalScore { get; init; }

    /// <summary>
    /// Total distance in kilometers from approved activity logs.
    /// </summary>
    public decimal TotalDistanceKm { get; init; }

    /// <summary>
    /// Total number of activity log submissions (all statuses).
    /// </summary>
    public int TotalSubmissions { get; init; }

    /// <summary>
    /// Number of approved activity log submissions.
    /// </summary>
    public int ApprovedSubmissions { get; init; }

    /// <summary>
    /// Number of pending activity log submissions.
    /// </summary>
    public int PendingSubmissions { get; init; }

    /// <summary>
    /// Number of rejected activity log submissions.
    /// </summary>
    public int RejectedSubmissions { get; init; }

    /// <summary>
    /// Creates a response from a Participant entity with activity logs.
    /// </summary>
    public static ParticipantStatsResponse FromEntity(Participant entity)
    {
        var logs = entity.ActivityLogs?.ToList() ?? new List<ActivityLog>();

        return new ParticipantStatsResponse
        {
            ParticipantId = entity.ParticipantId,
            ActivityId = entity.ActivityId,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.EmployeeName,
            JoinedAt = entity.JoinedAt,
            Status = entity.Status,
            TotalScore = entity.TotalScore,
            TotalDistanceKm = logs.Where(l => l.Status == ActivityLogStatus.APPROVED).Sum(l => l.Distance),
            TotalSubmissions = logs.Count,
            ApprovedSubmissions = logs.Count(l => l.Status == ActivityLogStatus.APPROVED),
            PendingSubmissions = logs.Count(l => l.Status == ActivityLogStatus.PENDING),
            RejectedSubmissions = logs.Count(l => l.Status == ActivityLogStatus.REJECTED)
        };
    }
}

/// <summary>
/// Response DTO for leaderboard entry.
/// </summary>
public record LeaderboardEntryResponse
{
    public int Rank { get; init; }
    public string EmployeeId { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public decimal TotalScore { get; init; }
    public string ParticipantId { get; init; } = string.Empty;

    /// <summary>
    /// Creates a leaderboard entry from a Participant entity.
    /// </summary>
    public static LeaderboardEntryResponse FromEntity(Participant entity, int rank)
    {
        return new LeaderboardEntryResponse
        {
            Rank = rank,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.EmployeeName,
            TotalScore = entity.TotalScore,
            ParticipantId = entity.ParticipantId
        };
    }
}

/// <summary>
/// Response DTO for activity statistics.
/// </summary>
public record ActivityStatisticsResponse
{
    public string ActivityId { get; init; } = string.Empty;
    public int TotalParticipants { get; init; }
    public int ActiveParticipants { get; init; }
    public decimal TotalDistance { get; init; }
    public int TotalLogs { get; init; }
    public int PendingLogs { get; init; }
    public int ApprovedLogs { get; init; }
    public int RejectedLogs { get; init; }
    public decimal AverageDistancePerParticipant { get; init; }

    /// <summary>
    /// Creates a response from ActivityStatistics entity.
    /// </summary>
    public static ActivityStatisticsResponse FromEntity(ActivityStatistics entity)
    {
        return new ActivityStatisticsResponse
        {
            ActivityId = entity.ActivityId,
            TotalParticipants = entity.TotalParticipants,
            ActiveParticipants = entity.ActiveParticipants,
            TotalDistance = entity.TotalDistance,
            TotalLogs = entity.TotalLogs,
            PendingLogs = entity.PendingLogs,
            ApprovedLogs = entity.ApprovedLogs,
            RejectedLogs = entity.RejectedLogs,
            AverageDistancePerParticipant = entity.AverageDistancePerParticipant
        };
    }
}

#endregion
