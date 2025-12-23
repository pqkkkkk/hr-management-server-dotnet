using Xunit;
using HrManagement.Api.Modules.Reward.Domain.Filter;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for RewardProgramQueryServiceImpl.
/// Uses shared database from RewardTestFixture (via Collection).
/// 
/// These tests are READ-ONLY and should run before Command tests.
/// </summary>
[Collection("1_RewardQueryTests")]
public class RewardProgramQueryServiceTests
{
    private readonly RewardTestFixture _fixture;

    public RewardProgramQueryServiceTests(RewardTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllPrograms()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetAllAsync_FilterByName_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 10,
            NameContains = "Q4"
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);

        var items = result.Items.ToList();
        Assert.Equal("Q4 2024 Employee Recognition", items[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_FilterByActiveStatus_ReturnsActiveOnly()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IsActive = true
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);

        var items = result.Items.ToList();
        Assert.Equal(ProgramStatus.ACTIVE, items[0].Status);
        Assert.Equal("program-001", items[0].RewardProgramId);
    }

    [Fact]
    public async Task GetAllAsync_FilterByInactiveStatus_ReturnsInactiveOnly()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IsActive = false
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);

        var items = result.Items.ToList();
        Assert.Equal(ProgramStatus.INACTIVE, items[0].Status);
        Assert.Equal("program-002", items[0].RewardProgramId);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_Page1_ReturnsFirstPage()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 1
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_Page2_ReturnsSecondPage()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 2,
            PageSize = 1
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Single(result.Items);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetAllAsync_WithItemsAndPolicies_IncludesRelatedData()
    {
        // Arrange
        var service = _fixture.GetService<IRewardProgramQueryService>();
        var filter = new RewardProgramFilter
        {
            PageNumber = 1,
            PageSize = 10,
            IsActive = true
        };

        // Act
        var result = await service.GetRewardProgramsAsync(filter);

        // Assert
        Assert.NotNull(result);
        var program = result.Items.First();

        // Verify items are loaded (program-001 has 3 items: item-001, item-002, item-003)
        Assert.NotEmpty(program.RewardItems);
        Assert.Equal(3, program.RewardItems.Count);

        // Verify policies are loaded (program-001 has 2 policies: policy-001, policy-002)
        Assert.NotEmpty(program.Policies);
        Assert.Equal(2, program.Policies.Count);
    }

    #endregion
}
