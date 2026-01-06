using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Represents a log entry for an activity (e.g., running distance for a day).
/// </summary>
[Table("activity_log")]
public class ActivityLog
{
    [Key]
    [Column("activity_log_id")]
    [StringLength(255)]
    public string ActivityLogId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("participant_id")]
    [StringLength(255)]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// Employee name for display purposes.
    /// </summary>
    [Column("employee_name")]
    [StringLength(255)]
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Distance in kilometers (for running activities)
    /// </summary>
    [Column("distance")]
    public decimal Distance { get; set; } = 0;

    /// <summary>
    /// Duration in minutes
    /// </summary>
    [Column("duration_minutes")]
    public int DurationMinutes { get; set; } = 0;

    /// <summary>
    /// URL to proof screenshot/image
    /// </summary>
    [Column("proof_url")]
    [StringLength(500)]
    public string? ProofUrl { get; set; }

    /// <summary>
    /// Date of the activity log
    /// </summary>
    [Column("log_date")]
    public DateTime LogDate { get; set; }

    [Column("status")]
    public ActivityLogStatus Status { get; set; } = ActivityLogStatus.PENDING;

    /// <summary>
    /// Reason for rejection (if rejected)
    /// </summary>
    [Column("reject_reason")]
    [StringLength(500)]
    public string? RejectReason { get; set; }

    /// <summary>
    /// Manager who reviewed this log
    /// </summary>
    [Column("reviewer_id")]
    [StringLength(255)]
    public string? ReviewerId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Participant Participant { get; set; } = null!;
}
