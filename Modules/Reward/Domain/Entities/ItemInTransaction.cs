using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Junction table for items exchanged in a transaction.
/// Used when a user exchanges points for reward items.
/// </summary>
[Table("item_in_transaction")]
public class ItemInTransaction
{
    [Key]
    [Column("item_in_transaction_id")]
    [StringLength(255)]
    public string ItemInTransactionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("transaction_id")]
    [StringLength(255)]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    [Column("reward_item_id")]
    [StringLength(255)]
    public string RewardItemId { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("total_points")]
    public int TotalPoints { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TransactionId))]
    public virtual PointTransaction? Transaction { get; set; }

    [ForeignKey(nameof(RewardItemId))]
    public virtual RewardItem? RewardItem { get; set; }
}
