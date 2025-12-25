using Xunit;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Modules.Activity.Domain.Services.Activity;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Tests.Integration.Activity;

/// <summary>
/// Integration tests for ActivityQueryServiceImpl.
/// Uses shared database from SharedTestFixture.
/// 
/// These tests are READ-ONLY and run before Command tests.
/// </summary>
[Collection("3_ActivityQueryTests")]
public class ActivityQueryServiceTests
{
    private readonly SharedTestFixture _fixture;

    public ActivityQueryServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetActivityByIdAsync Tests

    [Fact]
    public async Task GetActivityByIdAsync_ValidId_ReturnsActivity()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();

        // Act
        var result = await service.GetActivityByIdAsync("activity-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("activity-001", result.ActivityId);
        Assert.Equal("Q4 2024 Running Challenge", result.Name);
        Assert.Equal(ActivityStatus.IN_PROGRESS, result.Status);
    }

    [Fact]
    public async Task GetActivityByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();

        // Act
        var result = await service.GetActivityByIdAsync("non-existing-id");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetActivitiesAsync Tests

    [Fact]
    public async Task GetActivitiesAsync_NoFilter_ReturnsAllActivities()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await service.GetActivitiesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalItems); // activity-001, 002, 003, 004
    }

    [Fact]
    public async Task GetActivitiesAsync_FilterByStatus_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter
        {
            PageNumber = 1,
            PageSize = 10,
            Status = ActivityStatus.IN_PROGRESS
        };

        // Act
        var result = await service.GetActivitiesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("activity-001", result.Items.First().ActivityId);
    }

    [Fact]
    public async Task GetActivitiesAsync_FilterByName_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter
        {
            PageNumber = 1,
            PageSize = 10,
            NameContains = "New Year"
        };

        // Act
        var result = await service.GetActivitiesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("activity-002", result.Items.First().ActivityId);
    }

    [Fact]
    public async Task GetActivitiesAsync_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await service.GetActivitiesAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
    }

    #endregion

    #region GetMyActivitiesAsync Tests

    [Fact]
    public async Task GetMyActivitiesAsync_ParticipatingEmployee_ReturnsActivities()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter { PageNumber = 1, PageSize = 10 };

        // Act - employee-001 participates in activity-001 and activity-003
        var result = await service.GetMyActivitiesAsync("employee-001", filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetMyActivitiesAsync_NonParticipant_ReturnsEmpty()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();
        var filter = new ActivityFilter { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await service.GetMyActivitiesAsync("non-participant", filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region GetLeaderboardAsync Tests

    [Fact]
    public async Task GetLeaderboardAsync_ValidActivity_ReturnsSortedByScore()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();

        // Act - activity-001 has 3 participants
        var result = await service.GetLeaderboardAsync("activity-001", 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());

        var leaderboard = result.ToList();
        Assert.Equal("participant-001", leaderboard[0].ParticipantId); // 150 points
        Assert.Equal("participant-002", leaderboard[1].ParticipantId); // 100 points
        Assert.Equal("participant-003", leaderboard[2].ParticipantId); // 50 points
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithTopLimit_ReturnsLimitedResults()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();

        // Act
        var result = await service.GetLeaderboardAsync("activity-001", 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetActivityStatisticsAsync Tests

    [Fact]
    public async Task GetActivityStatisticsAsync_ValidActivity_ReturnsStats()
    {
        // Arrange
        var service = _fixture.GetService<IActivityQueryService>();

        // Act
        var result = await service.GetActivityStatisticsAsync("activity-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("activity-001", result.ActivityId);
        Assert.Equal(3, result.TotalParticipants);
        Assert.Equal(5, result.TotalLogs); // 5 logs in activity-001
        Assert.Equal(2, result.PendingLogs); // log-002, log-005
        Assert.Equal(2, result.ApprovedLogs); // log-001, log-004
        Assert.Equal(1, result.RejectedLogs); // log-003
    }

    #endregion
}
