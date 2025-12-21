using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Represents a user's wallet for a specific reward program.
/// Each user can have multiple wallets (one per program they participate in).
/// </summary>
[Table("user_wallet")]
public class UserWallet
{
    [Key]
    [Column("user_wallet_id")]
    [StringLength(255)]
    public string UserWalletId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Reference to User in the main Spring Boot application.
    /// Stored as string without FK constraint since User table is in a different service.
    /// </summary>
    [Required]
    [Column("user_id")]
    [StringLength(255)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("program_id")]
    [StringLength(255)]
    public string ProgramId { get; set; } = string.Empty;

    [Column("personal_point")]
    public int PersonalPoint { get; set; }

    [Column("giving_budget")]
    public int GivingBudget { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProgramId))]
    public virtual RewardProgram? Program { get; set; }

    public virtual ICollection<PointTransaction> SourceTransactions { get; set; } = new List<PointTransaction>();
    public virtual ICollection<PointTransaction> DestinationTransactions { get; set; } = new List<PointTransaction>();
}
