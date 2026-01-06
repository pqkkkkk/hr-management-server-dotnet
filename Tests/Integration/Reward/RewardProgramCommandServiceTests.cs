using Xunit;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for RewardProgramCommandServiceImpl.
/// Uses shared database from SharedTestFixture (via Collection).
/// 
/// IMPORTANT: These tests CREATE NEW data to avoid modifying seed data.
/// Test-created programs use prefix "test-" to distinguish from seed data.
/// </summary>
[Collection("2_RewardCommandTests")]
public class RewardProgramCommandServiceTests
{
    private readonly SharedTestFixture _fixture;

    public RewardProgramCommandServiceTests(SharedTestFixture fixture)
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

    [Fact]
    public async Task CreateRewardProgram_DeactivatesExistingActivePrograms()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();
        var dbContext = _fixture.CreateDbContext();

        // First create an active program
        var firstProgram = new RewardProgram
        {
            Name = "First Active Program",
            Description = "This should be deactivated when second is created",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "First Item", RequiredPoints = 100, Quantity = 10 }
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
        var createdFirst = await commandService.CreateRewardProgramAsync(firstProgram);
        Assert.Equal(ProgramStatus.ACTIVE, createdFirst.Status);

        // Act - Create second program (should deactivate first)
        var secondProgram = new RewardProgram
        {
            Name = "Second Active Program",
            Description = "This should become the only active program",
            StartDate = new DateTime(2025, 7, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 75,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Second Item", RequiredPoints = 150, Quantity = 5 }
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
        var createdSecond = await commandService.CreateRewardProgramAsync(secondProgram);

        // Assert - Second program should be active
        Assert.Equal(ProgramStatus.ACTIVE, createdSecond.Status);

        // First program should now be inactive
        var freshContext = _fixture.CreateDbContext();
        var updatedFirst = await freshContext.RewardPrograms
            .FindAsync(createdFirst.RewardProgramId);
        Assert.NotNull(updatedFirst);
        Assert.Equal(ProgramStatus.INACTIVE, updatedFirst.Status);
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

    #region UpdateRewardProgramAsync Tests

    [Fact]
    public async Task UpdateRewardProgram_ValidUpdate_UpdatesSuccessfully()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();

        // Use program-003 from seed data (see M202512_010_SeedUpdateDeleteTestData)
        // Original: Name="Update Test Program", 2 items, 1 policy
        var updatedProgram = new RewardProgram
        {
            RewardProgramId = "program-003",
            Name = "Update Test Program - Updated",
            Description = "Updated description",
            StartDate = new DateTime(2025, 2, 1),
            EndDate = new DateTime(2025, 12, 31),
            DefaultGivingBudget = 100,
            BannerUrl = "https://example.com/updated-banner.jpg",
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "New Item 1", RequiredPoints = 150, Quantity = 20 },
                new RewardItem { Name = "New Item 2", RequiredPoints = 300, Quantity = 10 },
                new RewardItem { Name = "New Item 3", RequiredPoints = 500, Quantity = 3 }
            },
            Policies = new List<RewardProgramPolicy>
            {
                new RewardProgramPolicy
                {
                    PolicyType = PolicyType.NOT_LATE,
                    CalculationPeriod = CalculationPeriod.WEEKLY,
                    UnitValue = 1,
                    PointsPerUnit = 20
                }
            }
        };

        // Act
        var result = await commandService.UpdateRewardProgramAsync(updatedProgram);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Update Test Program - Updated", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(new DateTime(2025, 2, 1), result.StartDate);
        Assert.Equal(new DateTime(2025, 12, 31), result.EndDate);
        Assert.Equal(100, result.DefaultGivingBudget);
        Assert.Equal("https://example.com/updated-banner.jpg", result.BannerUrl);

        // Items should be replaced (old 2 items removed, new 3 items added)
        Assert.Equal(3, result.RewardItems.Count);
        Assert.Contains(result.RewardItems, i => i.Name == "New Item 1");
        Assert.Contains(result.RewardItems, i => i.Name == "New Item 2");
        Assert.Contains(result.RewardItems, i => i.Name == "New Item 3");
        Assert.DoesNotContain(result.RewardItems, i => i.Name == "Update Test Item 1");

        // Policies should be replaced
        Assert.Single(result.Policies);
        Assert.Equal(PolicyType.NOT_LATE, result.Policies.First().PolicyType);
    }

    [Fact]
    public async Task UpdateRewardProgram_NonExistingProgram_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        var program = new RewardProgram
        {
            RewardProgramId = "non-existing-program-id",
            Name = "Non-existing Program",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Item", RequiredPoints = 100, Quantity = 5 }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateRewardProgramAsync(program));
    }

    [Fact]
    public async Task UpdateRewardProgram_EmptyItems_ThrowsArgumentException()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();

        // Create a temporary program for this test (to avoid affecting other tests)
        var tempProgram = new RewardProgram
        {
            Name = "Temp Program - Empty Items Test",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Temp Item", RequiredPoints = 100, Quantity = 10 }
            },
            Policies = new List<RewardProgramPolicy>()
        };
        var created = await commandService.CreateRewardProgramAsync(tempProgram);

        // Try to update with empty items
        var updateWithEmptyItems = new RewardProgram
        {
            RewardProgramId = created.RewardProgramId,
            Name = "Updated Name",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            RewardItems = new List<RewardItem>(), // Empty - should fail
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => commandService.UpdateRewardProgramAsync(updateWithEmptyItems));
    }

    [Fact]
    public async Task UpdateRewardProgram_InvalidDates_ThrowsArgumentException()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();

        // Create a temporary program for this test
        var tempProgram = new RewardProgram
        {
            Name = "Temp Program - Invalid Dates Test",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 30),
            DefaultGivingBudget = 50,
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Temp Item", RequiredPoints = 100, Quantity = 10 }
            },
            Policies = new List<RewardProgramPolicy>()
        };
        var created = await commandService.CreateRewardProgramAsync(tempProgram);

        // Try to update with invalid dates
        var updateWithInvalidDates = new RewardProgram
        {
            RewardProgramId = created.RewardProgramId,
            Name = "Updated Name",
            StartDate = new DateTime(2025, 12, 31), // End before start
            EndDate = new DateTime(2025, 1, 1),
            RewardItems = new List<RewardItem>
            {
                new RewardItem { Name = "Item", RequiredPoints = 100, Quantity = 10 }
            },
            Policies = new List<RewardProgramPolicy>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => commandService.UpdateRewardProgramAsync(updateWithInvalidDates));
    }

    #endregion

    #region DeleteRewardProgramAsync Tests

    [Fact]
    public async Task DeleteRewardProgram_NoTransactions_DeletesSuccessfully()
    {
        // Arrange
        var commandService = _fixture.GetService<IRewardProgramCommandService>();

        // Use program-004 from seed data (see M202512_010_SeedUpdateDeleteTestData)
        // program-004 has no transactions, no wallets - safe to delete
        var programId = "program-004";

        // Act
        await commandService.DeleteRewardProgramAsync(programId);

        // Assert - Verify program is deleted
        var dbContext = _fixture.CreateDbContext();
        var deletedProgram = await dbContext.RewardPrograms.FindAsync(programId);
        Assert.Null(deletedProgram);

        // Verify related entities are also deleted
        var remainingItems = dbContext.RewardItems.Where(i => i.ProgramId == programId).ToList();
        var remainingPolicies = dbContext.RewardProgramPolicies.Where(p => p.ProgramId == programId).ToList();
        Assert.Empty(remainingItems);
        Assert.Empty(remainingPolicies);
    }

    [Fact]
    public async Task DeleteRewardProgram_WithTransactions_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();
        // program-001 has transactions in seed data

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteRewardProgramAsync("program-001"));
    }

    [Fact]
    public async Task DeleteRewardProgram_NonExistingProgram_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteRewardProgramAsync("non-existing-program-id"));
    }

    #endregion
}
