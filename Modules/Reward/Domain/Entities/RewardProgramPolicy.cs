using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Defines a policy for automatic point distribution to employees.
/// Uses "Point Per Unit" model: Points = (AchievedValue / UnitValue) × PointsPerUnit
/// </summary>
[Table("reward_program_policy")]
public class RewardProgramPolicy
{
    [Key]
    [Column("policy_id")]
    [StringLength(255)]
    public string PolicyId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("program_id")]
    [StringLength(255)]
    public string ProgramId { get; set; } = string.Empty;

    /// <summary>
    /// Type of policy: NOT_LATE, OVERTIME, FULL_ATTENDANCE
    /// </summary>
    [Required]
    [Column("policy_type")]
    public PolicyType PolicyType { get; set; }

    /// <summary>
    /// Fixed to WEEKLY for simplicity in cron job logic
    /// </summary>
    [Required]
    [Column("calculation_period")]
    public CalculationPeriod CalculationPeriod { get; set; } = CalculationPeriod.WEEKLY;

    /// <summary>
    /// The unit value for calculation.
    /// Examples:
    /// - OVERTIME: 30 (minutes per unit)
    /// - NOT_LATE: 1 (day per unit)
    /// - FULL_ATTENDANCE: 1 (day per unit)
    /// </summary>
    [Column("unit_value")]
    public int UnitValue { get; set; } = 1;

    /// <summary>
    /// Points awarded per unit achieved.
    /// Example: 5 points per 30 minutes of overtime
    /// </summary>
    [Column("points_per_unit")]
    public int PointsPerUnit { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey(nameof(ProgramId))]
    public virtual RewardProgram? Program { get; set; }
}
