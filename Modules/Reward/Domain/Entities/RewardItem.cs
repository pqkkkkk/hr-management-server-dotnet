using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrManagement.Api.Modules.Reward.Domain.Entities;

/// <summary>
/// Represents an item that can be exchanged for points in a reward program.
/// </summary>
[Table("reward_item")]
public class RewardItem
{
    [Key]
    [Column("reward_item_id")]
    [StringLength(255)]
    public string RewardItemId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("program_id")]
    [StringLength(255)]
    public string ProgramId { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("required_points")]
    public float RequiredPoints { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("image_url")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProgramId))]
    public virtual RewardProgram? Program { get; set; }

    public virtual ICollection<ItemInTransaction> ItemInTransactions { get; set; } = new List<ItemInTransaction>();
}
