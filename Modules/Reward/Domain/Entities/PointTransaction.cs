using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Represents a point transaction between wallets.
/// Types: GIVE (gift points), RECEIVE (receive points), EXCHANGE (redeem for items)
/// </summary>
[Table("point_transaction")]
public class PointTransaction
{
    [Key]
    [Column("point_transaction_id")]
    [StringLength(255)]
    public string PointTransactionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Transaction type: GIFT, EXCHANGE, POLICY_REWARD
    /// </summary>
    [Required]
    [Column("type")]
    public TransactionType Type { get; set; }

    [Column("amount")]
    public float Amount { get; set; }

    [Column("source_wallet_id")]
    [StringLength(255)]
    public string? SourceWalletId { get; set; }

    [Column("destination_wallet_id")]
    [StringLength(255)]
    public string? DestinationWalletId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SourceWalletId))]
    public virtual UserWallet? SourceWallet { get; set; }

    [ForeignKey(nameof(DestinationWalletId))]
    public virtual UserWallet? DestinationWallet { get; set; }

    public virtual ICollection<ItemInTransaction> Items { get; set; } = new List<ItemInTransaction>();
}
