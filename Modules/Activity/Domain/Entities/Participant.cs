using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Represents a participant in an activity.
/// </summary>
[Table("participant")]
public class Participant
{
    [Key]
    [Column("participant_id")]
    [StringLength(255)]
    public string ParticipantId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("activity_id")]
    [StringLength(255)]
    public string ActivityId { get; set; } = string.Empty;

    /// <summary>
    /// Employee ID from external system (stored as string)
    /// </summary>
    [Required]
    [Column("employee_id")]
    [StringLength(255)]
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Employee name from external system (stored for display purposes)
    /// </summary>
    [Column("employee_name")]
    [StringLength(255)]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [Column("status")]
    public ParticipantStatus Status { get; set; } = ParticipantStatus.ACTIVE;

    /// <summary>
    /// Cached total score from approved activity logs
    /// </summary>
    [Column("total_score")]
    public decimal TotalScore { get; set; } = 0;

    // Navigation properties
    public virtual Activity Activity { get; set; } = null!;
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
