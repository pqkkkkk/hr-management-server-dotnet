using Xunit;
using HrManagement.Api.Modules.Activity.Domain.Filter;
using HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Tests.Integration.Activity;

/// <summary>
/// Integration tests for ActivityLogQueryServiceImpl.
/// Uses shared database from SharedTestFixture.
/// </summary>
[Collection("3_ActivityQueryTests")]
public class ActivityLogQueryServiceTests
{
    private readonly SharedTestFixture _fixture;

    public ActivityLogQueryServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetActivityLogsAsync Tests

    [Fact]
    public async Task GetActivityLogsAsync_NoFilter_ReturnsAllLogs()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogQueryService>();
        var filter = new ActivityLogFilter { PageNumber = 1, PageSize = 20 };

        // Act
        var result = await service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalItems); // 5 logs in seed data
    }

    [Fact]
    public async Task GetActivityLogsAsync_FilterByStatus_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogQueryService>();
        var filter = new ActivityLogFilter
        {
            PageNumber = 1,
            PageSize = 20,
            Status = ActivityLogStatus.PENDING
        };

        // Act
        var result = await service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems); // log-002, log-005
        Assert.All(result.Items, l => Assert.Equal(ActivityLogStatus.PENDING, l.Status));
    }

    [Fact]
    public async Task GetActivityLogsAsync_FilterByActivityId_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogQueryService>();
        var filter = new ActivityLogFilter
        {
            PageNumber = 1,
            PageSize = 20,
            ActivityId = "activity-001"
        };

        // Act
        var result = await service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalItems); // All logs are from activity-001
    }

    [Fact]
    public async Task GetActivityLogsAsync_FilterByEmployeeId_ReturnsMatching()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogQueryService>();
        var filter = new ActivityLogFilter
        {
            PageNumber = 1,
            PageSize = 20,
            EmployeeId = "employee-001"
        };

        // Act
        var result = await service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems); // log-001, log-002 from participant-001
    }

    [Fact]
    public async Task GetActivityLogsAsync_Pagination_Works()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogQueryService>();
        var filter = new ActivityLogFilter
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await service.GetActivityLogsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
    }

    #endregion
}
