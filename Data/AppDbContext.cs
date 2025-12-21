using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Modules.Reward.Domain.Entities;

namespace HrManagement.Api.Data;

/// <summary>
/// Application database context for HR Management system.
/// Handles entity configurations for Reward and Activity modules.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ==========================================================================
    // REWARD MODULE ENTITIES
    // ==========================================================================
    public DbSet<RewardProgram> RewardPrograms { get; set; }
    public DbSet<RewardItem> RewardItems { get; set; }
    public DbSet<UserWallet> UserWallets { get; set; }
    public DbSet<PointTransaction> PointTransactions { get; set; }
    public DbSet<ItemInTransaction> ItemInTransactions { get; set; }
    public DbSet<RewardProgramPolicy> RewardProgramPolicies { get; set; }

    // ==========================================================================
    // ACTIVITY MODULE ENTITIES
    // ==========================================================================
    // DbSets will be added when entities are created

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================================================================
        // REWARD MODULE CONFIGURATIONS
        // ======================================================================

        // RewardProgram -> RewardItems (1:N)
        modelBuilder.Entity<RewardItem>()
            .HasOne(ri => ri.Program)
            .WithMany(rp => rp.RewardItems)
            .HasForeignKey(ri => ri.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // RewardProgram -> UserWallets (1:N)
        modelBuilder.Entity<UserWallet>()
            .HasOne(uw => uw.Program)
            .WithMany(rp => rp.UserWallets)
            .HasForeignKey(uw => uw.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // PointTransaction -> SourceWallet (N:1)
        modelBuilder.Entity<PointTransaction>()
            .HasOne(pt => pt.SourceWallet)
            .WithMany(uw => uw.SourceTransactions)
            .HasForeignKey(pt => pt.SourceWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // PointTransaction -> DestinationWallet (N:1)
        modelBuilder.Entity<PointTransaction>()
            .HasOne(pt => pt.DestinationWallet)
            .WithMany(uw => uw.DestinationTransactions)
            .HasForeignKey(pt => pt.DestinationWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // ItemInTransaction -> PointTransaction (N:1)
        modelBuilder.Entity<ItemInTransaction>()
            .HasOne(iit => iit.Transaction)
            .WithMany(pt => pt.Items)
            .HasForeignKey(iit => iit.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ItemInTransaction -> RewardItem (N:1)
        modelBuilder.Entity<ItemInTransaction>()
            .HasOne(iit => iit.RewardItem)
            .WithMany(ri => ri.ItemInTransactions)
            .HasForeignKey(iit => iit.RewardItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
