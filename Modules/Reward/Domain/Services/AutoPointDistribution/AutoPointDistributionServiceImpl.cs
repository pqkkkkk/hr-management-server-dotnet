using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Services.AutoPointDistribution;

/// <summary>
/// Implementation of auto point distribution service.
/// Calculates and distributes points based on reward program policies and timesheet data.
/// </summary>
public class AutoPointDistributionServiceImpl : IAutoPointDistributionService
{
    private const int MAX_USER_IDS_PER_BATCH = 100;

    private readonly AppDbContext _dbContext;
    private readonly IRewardProgramDao _rewardProgramDao;
    private readonly IUserWalletDao _userWalletDao;
    private readonly IPointTransactionDao _pointTransactionDao;
    private readonly ISpringBootApiClient _springBootApiClient;

    public AutoPointDistributionServiceImpl(
        AppDbContext dbContext,
        IRewardProgramDao rewardProgramDao,
        IUserWalletDao userWalletDao,
        IPointTransactionDao pointTransactionDao,
        ISpringBootApiClient springBootApiClient)
    {
        _dbContext = dbContext;
        _rewardProgramDao = rewardProgramDao;
        _userWalletDao = userWalletDao;
        _pointTransactionDao = pointTransactionDao;
        _springBootApiClient = springBootApiClient;
    }

    public async Task<DistributionResult> DistributePointsAsync(
        string programId,
        DateTime startDate,
        DateTime endDate)
    {
        // 1. Validate and get program with policies
        var program = await GetAndValidateProgramAsync(programId);
        var activePolicies = program.Policies.Where(p => p.IsActive).ToList();

        if (!activePolicies.Any())
        {
            return new DistributionResult(0, 0, 0, new List<UserDistributionSummary>());
        }

        // 2. Get all user wallets for this program
        var wallets = await GetWalletsForProgramAsync(programId);
        if (!wallets.Any())
        {
            return new DistributionResult(0, 0, 0, new List<UserDistributionSummary>());
        }

        // 3. Fetch timesheet statistics in batches (max 100 userIds per call)
        var userIds = wallets.Select(w => w.UserId).ToList();
        var allStatistics = await FetchTimesheetStatisticsInBatchesAsync(userIds, startDate, endDate);

        // 4. Create a lookup for quick access
        var statsLookup = allStatistics.ToDictionary(s => s.UserId);
        var walletLookup = wallets.ToDictionary(w => w.UserId);

        // 5. Calculate points and create transactions
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var userSummaries = new List<UserDistributionSummary>();
            var allTransactions = new List<PointTransaction>();
            int totalPointsDistributed = 0;

            foreach (var wallet in wallets)
            {
                if (!statsLookup.TryGetValue(wallet.UserId, out var stats))
                {
                    // User has no stats (should have zeros from API, but just in case)
                    continue;
                }

                var (pointsEarned, pointsByPolicy, transactions) = CalculatePointsForUser(
                    wallet, stats, activePolicies);

                if (pointsEarned > 0)
                {
                    // Update wallet points
                    wallet.PersonalPoint += pointsEarned;
                    totalPointsDistributed += pointsEarned;
                    allTransactions.AddRange(transactions);

                    userSummaries.Add(new UserDistributionSummary(
                        wallet.UserId,
                        wallet.UserName,
                        pointsEarned,
                        pointsByPolicy
                    ));
                }
            }

            // Save all changes
            if (allTransactions.Any())
            {
                await _pointTransactionDao.CreateBatchAsync(allTransactions);
            }
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return new DistributionResult(
                wallets.Count,
                totalPointsDistributed,
                allTransactions.Count,
                userSummaries
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<RewardProgram> GetAndValidateProgramAsync(string programId)
    {
        var program = await _rewardProgramDao.GetByIdAsync(programId);
        
        if (program == null)
        {
            throw new KeyNotFoundException($"Reward program with ID {programId} not found.");
        }

        if (program.Status != ProgramStatus.ACTIVE)
        {
            throw new InvalidOperationException($"Reward program {programId} is not active.");
        }

        return program;
    }

    private async Task<List<UserWallet>> GetWalletsForProgramAsync(string programId)
    {
        return await _dbContext.UserWallets
            .Where(w => w.ProgramId == programId)
            .ToListAsync();
    }

    private async Task<List<TimesheetStatisticsDto>> FetchTimesheetStatisticsInBatchesAsync(
        List<string> userIds,
        DateTime startDate,
        DateTime endDate)
    {
        var allStatistics = new List<TimesheetStatisticsDto>();

        // Chunk userIds into batches of MAX_USER_IDS_PER_BATCH
        var batches = userIds
            .Select((userId, index) => new { userId, index })
            .GroupBy(x => x.index / MAX_USER_IDS_PER_BATCH)
            .Select(g => g.Select(x => x.userId).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            var batchStats = await _springBootApiClient.GetBatchTimesheetStatisticsAsync(
                batch, startDate, endDate);
            
            if (batchStats != null)
            {
                allStatistics.AddRange(batchStats);
            }
        }

        return allStatistics;
    }

    private (int PointsEarned, Dictionary<string, int> PointsByPolicy, List<PointTransaction> Transactions)
        CalculatePointsForUser(
            UserWallet wallet,
            TimesheetStatisticsDto stats,
            List<RewardProgramPolicy> policies)
    {
        var pointsByPolicy = new Dictionary<string, int>();
        var transactions = new List<PointTransaction>();
        int totalPoints = 0;

        foreach (var policy in policies)
        {
            int achievedValue = GetAchievedValue(stats, policy.PolicyType);
            int units = achievedValue / policy.UnitValue;
            int points = units * policy.PointsPerUnit;

            if (points > 0)
            {
                pointsByPolicy[policy.PolicyType.ToString()] = points;
                totalPoints += points;

                // Create transaction for this policy reward
                transactions.Add(new PointTransaction
                {
                    PointTransactionId = Guid.NewGuid().ToString(),
                    Type = TransactionType.POLICY_REWARD,
                    Amount = points,
                    SourceWalletId = null, // System distribution, no source
                    DestinationWalletId = wallet.UserWalletId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return (totalPoints, pointsByPolicy, transactions);
    }

    /// <summary>
    /// Gets the achieved value from timesheet statistics based on policy type.
    /// </summary>
    private int GetAchievedValue(TimesheetStatisticsDto stats, PolicyType policyType)
    {
        return policyType switch
        {
            // NOT_LATE: Days where user was not late = TotalDays - LateDays
            PolicyType.NOT_LATE => (int)(stats.TotalDays - stats.LateDays),

            // OVERTIME: Total overtime minutes
            PolicyType.OVERTIME => (int)stats.TotalOvertimeMinutes,

            // FULL_ATTENDANCE: Days with both morning and afternoon present
            // Since we have separate counts, take the minimum (both slots must be present)
            PolicyType.FULL_ATTENDANCE => (int)Math.Min(stats.MorningPresent, stats.AfternoonPresent),

            _ => 0
        };
    }
}
