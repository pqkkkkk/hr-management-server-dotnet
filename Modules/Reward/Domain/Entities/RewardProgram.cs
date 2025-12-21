using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Represents a reward program that employees can participate in.
/// </summary>
[Table("reward_program")]
public class RewardProgram
{
    [Key]
    [Column("reward_program_id")]
    [StringLength(255)]
    public string RewardProgramId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("status")]
    public ProgramStatus Status { get; set; } = ProgramStatus.ACTIVE;

    /// <summary>
    /// Default giving budget allocated to manager wallets when program is created
    /// </summary>
    [Column("default_giving_budget")]
    public int DefaultGivingBudget { get; set; } = 100;

    [Column("banner_url")]
    [StringLength(500)]
    public string? BannerUrl { get; set; }

    // Navigation properties
    public virtual ICollection<RewardItem> RewardItems { get; set; } = new List<RewardItem>();
    public virtual ICollection<UserWallet> UserWallets { get; set; } = new List<UserWallet>();
    public virtual ICollection<RewardProgramPolicy> Policies { get; set; } = new List<RewardProgramPolicy>();
}
