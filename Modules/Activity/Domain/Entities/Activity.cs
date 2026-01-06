using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Represents an activity that employees can participate in.
/// </summary>
[Table("activity")]
public class Activity
{
    [Key]
    [Column("activity_id")]
    [StringLength(255)]
    public string ActivityId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("activity_type")]
    public ActivityType Type { get; set; } = ActivityType.RUNNING;

    /// <summary>
    /// Template identifier that maps to Strategy class (e.g., "RUNNING_SIMPLE")
    /// </summary>
    [Required]
    [Column("template_id")]
    [StringLength(100)]
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// HTML content for activity description
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    [Column("banner_url")]
    [StringLength(500)]
    public string? BannerUrl { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("status")]
    public ActivityStatus Status { get; set; } = ActivityStatus.DRAFT;

    /// <summary>
    /// JSONB configuration with values entered by admin when creating activity
    /// </summary>
    [Column("config", TypeName = "jsonb")]
    public JsonDocument? Config { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    #region Computed Properties (Not Mapped to DB)

    /// <summary>
    /// Number of participants in this activity.
    /// Computed from Participants navigation property.
    /// </summary>
    [NotMapped]
    public int ParticipantsCount => Participants?.Count ?? 0;

    #endregion
}
