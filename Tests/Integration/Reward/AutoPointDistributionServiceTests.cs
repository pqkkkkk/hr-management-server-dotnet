using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Services.AutoPointDistribution;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for AutoPointDistributionServiceImpl.
/// Tests the point distribution logic with mocked Spring Boot API.
/// </summary>
[Collection("2_RewardCommandTests")]
[TestCaseOrderer("HrManagement.Api.Tests.Integration.PriorityOrderer", "hr-management-dotnet")]
public class AutoPointDistributionServiceTests
{
    private readonly SharedTestFixture _fixture;
    private readonly IAutoPointDistributionService _service;
    private readonly AppDbContext _dbContext;

    // Test data IDs
    private const string TEST_PROGRAM_ID = "test-program-distribution";
    private const string USER_1_ID = "user-for-distribution-1";
    private const string USER_2_ID = "user-for-distribution-2";
    private const string USER_3_ID = "user-for-distribution-3";

    public AutoPointDistributionServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
        _service = _fixture.GetService<IAutoPointDistributionService>();
        _dbContext = _fixture.CreateDbContext();
    }

    #region Test Setup

    [Fact, TestPriority(1)]
    public async Task Setup_CreateTestProgramAndWallets()
    {
        // Create a test program with policies
        var program = new RewardProgram
        {
            RewardProgramId = TEST_PROGRAM_ID,
            Name = "Distribution Test Program",
            Description = "Program for testing auto point distribution",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            DefaultGivingBudget = 100,
            Status = ProgramStatus.ACTIVE,
            Policies = new List<RewardProgramPolicy>
            {
                new()
                {
                    PolicyId = "policy-not-late",
                    ProgramId = TEST_PROGRAM_ID,
                    PolicyType = PolicyType.NOT_LATE,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 1, // 1 day not late = 1 unit
                    PointsPerUnit = 5, // 5 points per day not late
                    IsActive = true
                },
                new()
                {
                    PolicyId = "policy-overtime",
                    ProgramId = TEST_PROGRAM_ID,
                    PolicyType = PolicyType.OVERTIME,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 30, // 30 minutes = 1 unit
                    PointsPerUnit = 10, // 10 points per 30 mins overtime
                    IsActive = true
                },
                new()
                {
                    PolicyId = "policy-full-attendance",
                    ProgramId = TEST_PROGRAM_ID,
                    PolicyType = PolicyType.FULL_ATTENDANCE,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 1, // 1 full day = 1 unit
                    PointsPerUnit = 3, // 3 points per full attendance day
                    IsActive = true
                }
            },
            RewardItems = new List<RewardItem>
            {
                new()
                {
                    RewardItemId = "item-test-dist",
                    ProgramId = TEST_PROGRAM_ID,
                    Name = "Test Item",
                    RequiredPoints = 100,
                    Quantity = 10
                }
            }
        };

        _dbContext.RewardPrograms.Add(program);

        // Create wallets for test users
        var wallets = new List<UserWallet>
        {
            new()
            {
                UserWalletId = "wallet-dist-1",
                UserId = USER_1_ID,
                UserName = "User One",
                ProgramId = TEST_PROGRAM_ID,
                PersonalPoint = 0,
                GivingBudget = 0
            },
            new()
            {
                UserWalletId = "wallet-dist-2",
                UserId = USER_2_ID,
                UserName = "User Two",
                ProgramId = TEST_PROGRAM_ID,
                PersonalPoint = 0,
                GivingBudget = 0
            },
            new()
            {
                UserWalletId = "wallet-dist-3",
                UserId = USER_3_ID,
                UserName = "User Three",
                ProgramId = TEST_PROGRAM_ID,
                PersonalPoint = 50, // Already has some points
                GivingBudget = 0
            }
        };

        _dbContext.UserWallets.AddRange(wallets);
        await _dbContext.SaveChangesAsync();

        // Verify setup
        var savedProgram = await _dbContext.RewardPrograms
            .Include(p => p.Policies)
            .FirstOrDefaultAsync(p => p.RewardProgramId == TEST_PROGRAM_ID);

        Assert.NotNull(savedProgram);
        Assert.Equal(3, savedProgram.Policies.Count);
    }

    #endregion

    #region Happy Path Tests

    [Fact, TestPriority(10)]
    public async Task DistributePointsAsync_WithValidData_CalculatesPointsCorrectly()
    {
        // Arrange - Setup mock timesheet data
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var mockStats = new List<TimesheetStatisticsDto>
        {
            new()
            {
                UserId = USER_1_ID,
                TotalDays = 5,
                MorningPresent = 5,
                AfternoonPresent = 5,
                LateDays = 1,
                TotalLateMinutes = 30,
                TotalOvertimeMinutes = 60, // 2 units of 30 mins
                TotalWorkCredit = 5.0
            },
            new()
            {
                UserId = USER_2_ID,
                TotalDays = 5,
                MorningPresent = 5,
                AfternoonPresent = 4, // Missing one afternoon
                LateDays = 0,
                TotalLateMinutes = 0,
                TotalOvertimeMinutes = 90, // 3 units of 30 mins
                TotalWorkCredit = 4.5
            },
            new()
            {
                UserId = USER_3_ID,
                TotalDays = 5,
                MorningPresent = 5,
                AfternoonPresent = 5,
                LateDays = 2,
                TotalLateMinutes = 45,
                TotalOvertimeMinutes = 0,
                TotalWorkCredit = 5.0
            }
        };

        _fixture.SetupMockTimesheetStatistics(mockStats);

        // Act
        var result = await _service.DistributePointsAsync(TEST_PROGRAM_ID, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalUsersProcessed);
        Assert.True(result.TotalPointsDistributed > 0);
        Assert.True(result.TotalTransactionsCreated > 0);

        // User 1: NOT_LATE = (5-1)*5 = 20, OVERTIME = 2*10 = 20, FULL_ATTENDANCE = 5*3 = 15 => Total = 55
        var user1Summary = result.UserSummaries.FirstOrDefault(u => u.UserId == USER_1_ID);
        Assert.NotNull(user1Summary);
        Assert.Equal(55, user1Summary.PointsEarned);

        // User 2: NOT_LATE = 5*5 = 25, OVERTIME = 3*10 = 30, FULL_ATTENDANCE = 4*3 = 12 => Total = 67
        var user2Summary = result.UserSummaries.FirstOrDefault(u => u.UserId == USER_2_ID);
        Assert.NotNull(user2Summary);
        Assert.Equal(67, user2Summary.PointsEarned);

        // User 3: NOT_LATE = (5-2)*5 = 15, OVERTIME = 0, FULL_ATTENDANCE = 5*3 = 15 => Total = 30
        var user3Summary = result.UserSummaries.FirstOrDefault(u => u.UserId == USER_3_ID);
        Assert.NotNull(user3Summary);
        Assert.Equal(30, user3Summary.PointsEarned);
    }

    [Fact, TestPriority(11)]
    public async Task DistributePointsAsync_UpdatesWalletPoints()
    {
        // Verify wallet points were updated from previous test
        var wallet1 = await _dbContext.UserWallets.FirstOrDefaultAsync(w => w.UserId == USER_1_ID);
        var wallet2 = await _dbContext.UserWallets.FirstOrDefaultAsync(w => w.UserId == USER_2_ID);
        var wallet3 = await _dbContext.UserWallets.FirstOrDefaultAsync(w => w.UserId == USER_3_ID);

        // Note: wallet3 had 50 initial points
        Assert.Equal(55, wallet1!.PersonalPoint);
        Assert.Equal(67, wallet2!.PersonalPoint);
        Assert.Equal(80, wallet3!.PersonalPoint); // 50 + 30
    }

    [Fact, TestPriority(12)]
    public async Task DistributePointsAsync_CreatesTransactions()
    {
        // Verify transactions were created
        var transactions = await _dbContext.PointTransactions
            .Where(t => t.Type == TransactionType.POLICY_REWARD)
            .Where(t => t.DestinationWallet != null && t.DestinationWallet.ProgramId == TEST_PROGRAM_ID)
            .ToListAsync();

        Assert.NotEmpty(transactions);
        Assert.All(transactions, t =>
        {
            Assert.Equal(TransactionType.POLICY_REWARD, t.Type);
            Assert.Null(t.SourceWalletId); // System distribution has no source
            Assert.NotNull(t.DestinationWalletId);
        });
    }

    #endregion

    #region Edge Cases

    [Fact, TestPriority(20)]
    public async Task DistributePointsAsync_WithZeroStats_ReturnsNoPointsForUser()
    {
        // Arrange - Create a new program for this test
        var programId = "test-zero-stats-program";
        var userId = "user-zero-stats";

        await CreateTestProgram(programId, userId);

        var mockStats = new List<TimesheetStatisticsDto>
        {
            new()
            {
                UserId = userId,
                TotalDays = 0,
                MorningPresent = 0,
                AfternoonPresent = 0,
                LateDays = 0,
                TotalLateMinutes = 0,
                TotalOvertimeMinutes = 0,
                TotalWorkCredit = 0
            }
        };

        _fixture.SetupMockTimesheetStatistics(mockStats);

        // Act
        var result = await _service.DistributePointsAsync(
            programId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        Assert.Equal(1, result.TotalUsersProcessed);
        Assert.Equal(0, result.TotalPointsDistributed);
        Assert.Empty(result.UserSummaries); // No summaries for 0 points
    }

    [Fact, TestPriority(21)]
    public async Task DistributePointsAsync_WithNoWallets_ReturnsEmptyResult()
    {
        // Arrange - Create program without wallets
        var programId = "test-no-wallets-program";
        var program = new RewardProgram
        {
            RewardProgramId = programId,
            Name = "No Wallets Program",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            DefaultGivingBudget = 100,
            Status = ProgramStatus.ACTIVE,
            Policies = new List<RewardProgramPolicy>
            {
                new()
                {
                    PolicyId = $"{programId}-policy",
                    ProgramId = programId,
                    PolicyType = PolicyType.NOT_LATE,
                    UnitValue = 1,
                    PointsPerUnit = 5,
                    IsActive = true
                }
            },
            RewardItems = new List<RewardItem>
            {
                new()
                {
                    RewardItemId = $"{programId}-item",
                    ProgramId = programId,
                    Name = "Item",
                    RequiredPoints = 10,
                    Quantity = 5
                }
            }
        };
        _dbContext.RewardPrograms.Add(program);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DistributePointsAsync(
            programId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        Assert.Equal(0, result.TotalUsersProcessed);
        Assert.Equal(0, result.TotalPointsDistributed);
    }

    #endregion

    #region Validation Tests

    [Fact, TestPriority(30)]
    public async Task DistributePointsAsync_WithNonExistentProgram_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DistributePointsAsync("non-existent", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow));
    }

    [Fact, TestPriority(31)]
    public async Task DistributePointsAsync_WithInactiveProgram_ThrowsInvalidOperationException()
    {
        // Arrange
        var programId = "test-inactive-program";
        var program = new RewardProgram
        {
            RewardProgramId = programId,
            Name = "Inactive Program",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            DefaultGivingBudget = 100,
            Status = ProgramStatus.INACTIVE,
            RewardItems = new List<RewardItem>
            {
                new()
                {
                    RewardItemId = $"{programId}-item",
                    ProgramId = programId,
                    Name = "Item",
                    RequiredPoints = 10,
                    Quantity = 5
                }
            }
        };
        _dbContext.RewardPrograms.Add(program);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DistributePointsAsync(programId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow));
    }

    #endregion

    #region Helper Methods

    private async Task CreateTestProgram(string programId, string userId)
    {
        var program = new RewardProgram
        {
            RewardProgramId = programId,
            Name = $"Test Program {programId}",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            DefaultGivingBudget = 100,
            Status = ProgramStatus.ACTIVE,
            Policies = new List<RewardProgramPolicy>
            {
                new()
                {
                    PolicyId = $"{programId}-policy",
                    ProgramId = programId,
                    PolicyType = PolicyType.NOT_LATE,
                    UnitValue = 1,
                    PointsPerUnit = 5,
                    IsActive = true
                }
            },
            RewardItems = new List<RewardItem>
            {
                new()
                {
                    RewardItemId = $"{programId}-item",
                    ProgramId = programId,
                    Name = "Item",
                    RequiredPoints = 10,
                    Quantity = 5
                }
            }
        };

        var wallet = new UserWallet
        {
            UserWalletId = $"wallet-{userId}",
            UserId = userId,
            UserName = "Test User",
            ProgramId = programId,
            PersonalPoint = 0,
            GivingBudget = 0
        };

        _dbContext.RewardPrograms.Add(program);
        _dbContext.UserWallets.Add(wallet);
        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
