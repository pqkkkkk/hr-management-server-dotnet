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
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

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

        // ======================================================================
        // ENUM TO STRING CONVERSIONS
        // Required for FluentMigrator compatibility (migrations store as strings)
        // ======================================================================
        modelBuilder.Entity<RewardProgram>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<PointTransaction>()
            .Property(e => e.Type)
            .HasConversion<string>();

        modelBuilder.Entity<RewardProgramPolicy>()
            .Property(e => e.PolicyType)
            .HasConversion<string>();

        modelBuilder.Entity<RewardProgramPolicy>()
            .Property(e => e.CalculationPeriod)
            .HasConversion<string>();

        // ======================================================================
        // ACTIVITY MODULE CONFIGURATIONS
        // ======================================================================

        // Activity -> Participants (1:N)
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.Activity)
            .WithMany(a => a.Participants)
            .HasForeignKey(p => p.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Participant -> ActivityLogs (1:N)
        modelBuilder.Entity<ActivityLog>()
            .HasOne(al => al.Participant)
            .WithMany(p => p.ActivityLogs)
            .HasForeignKey(al => al.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Activity enum conversions
        modelBuilder.Entity<HrManagement.Api.Modules.Activity.Domain.Entities.Activity>()
            .Property(e => e.Type)
            .HasConversion<string>();

        modelBuilder.Entity<HrManagement.Api.Modules.Activity.Domain.Entities.Activity>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Participant>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ActivityLog>()
            .Property(e => e.Status)
            .HasConversion<string>();

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
