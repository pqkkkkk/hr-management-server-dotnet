using Xunit;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for RewardProgramCommandServiceImpl.
/// Uses shared database from RewardTestFixture (via Collection).
/// 
/// IMPORTANT: These tests CREATE NEW data to avoid modifying seed data.
/// Test-created programs use prefix "test-" to distinguish from seed data.
/// </summary>
[Collection("2_RewardCommandTests")]
public class RewardProgramCommandServiceTests
{
    private readonly RewardTestFixture _fixture;

    public RewardProgramCommandServiceTests(RewardTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region CreateRewardProgramAsync Tests

    [Fact]
    public async Task CreateRewardProgram_ValidProgram_CreatesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "Test Program - Create Valid",
            Description = "Test program for integration testing",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 50,
            BannerUrl = "https://example.com/test-banner.jpg",
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Test Item 1", RequiredPoints = 100, Quantity = 10 },
                new RewardItem { Name = "Test Item 2", RequiredPoints = 200, Quantity = 5 }
            },
            Policies = new List<RewardProgramPolicy>
            {
                new RewardProgramPolicy
                {
                    PolicyType = PolicyType.OVERTIME,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 10,
                    PointsPerUnit = 5
                }
            }
        };

        // Act
        var result = await service.CreateRewardProgramAsync(program);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RewardProgramId);
        Assert.Equal("Test Program - Create Valid", result.Name);
        Assert.Equal(ProgramStatus.ACTIVE, result.Status);
        Assert.Equal(2, result.RewardItems.Count);
        Assert.Single(result.Policies);

        // Verify IDs are generated for items and policies
        Assert.All(result.RewardItems, item => Assert.NotNull(item.RewardItemId));
        Assert.All(result.Policies, policy => Assert.NotNull(policy.PolicyId));
    }

    [Fact]
    public async Task CreateRewardProgram_EmptyItems_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "Test Program - No Items",
            Description = "Program without items",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            DefaultGivingBudget = 100,
            RewardItems = new List<RewardItem>(), // Empty - should fail
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateRewardProgramAsync(program));
    }

    [Fact]
    public async Task CreateRewardProgram_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>(),
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateRewardProgramAsync(program));
    }

    [Fact]
    public async Task CreateRewardProgram_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "Test Program - Invalid Dates",
            StartDate = new DateTime(2025, 12, 31),
            EndDate = new DateTime(2025, 1, 1), // Before start
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>(),
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateRewardProgramAsync(program));
    }

    [Fact]
    public async Task CreateRewardProgram_NegativeBudget_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "Test Program - Negative Budget",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = -100, // Invalid
            RewardItems = new List<RewardItem>(),
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateRewardProgramAsync(program));
    }

    [Fact]
    public async Task CreateRewardProgram_ItemWithZeroPoints_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            Name = "Test Program - Invalid Item",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Invalid Item", RequiredPoints = 0, Quantity = 10 }
            },
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateRewardProgramAsync(program));
    }

    #endregion

    #region DeactivateRewardProgramAsync Tests

    [Fact]
    public async Task DeactivateRewardProgram_ActiveProgram_DeactivatesSuccessfully()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();
        var queryService = _fixture.GetService<IRewardProgramQueryService>();

        // First create a new program to deactivate
        var program = new RewardProgram
        {
            Name = "Test Program - To Deactivate",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Test Item", RequiredPoints = 100, Quantity = 5 }
            },
            Policies = new List<RewardProgramPolicy>
            {
                new RewardProgramPolicy
                {
                    PolicyType = PolicyType.NOT_LATE,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 1,
                    PointsPerUnit = 10
                }
            }
        };
        var createdProgram = await commandService.CreateRewardProgramAsync(program);
        Assert.Equal(ProgramStatus.ACTIVE, createdProgram.Status);

        // Act
        await commandService.DeactivateRewardProgramAsync(createdProgram.RewardProgramId);

        // Assert - Verify program is now inactive
        var dbContext = _fixture.CreateDbContext();
        var updatedProgram = await dbContext.RewardPrograms
            .FindAsync(createdProgram.RewardProgramId);

        Assert.NotNull(updatedProgram);
        Assert.Equal(ProgramStatus.INACTIVE, updatedProgram.Status);
    }

    [Fact]
    public async Task DeactivateRewardProgram_NonExistingProgram_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.DeactivateRewardProgramAsync("non-existing-program-id"));
    }

    [Fact]
    public async Task DeactivateRewardProgram_AlreadyInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        // program-002 is already INACTIVE in seed data

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeactivateRewardProgramAsync("program-002"));
    }

    #endregion
}
