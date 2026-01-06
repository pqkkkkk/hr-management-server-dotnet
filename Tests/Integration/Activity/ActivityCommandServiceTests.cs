using System.Text.Json;
using Xunit;
using HrManagement.Api.Modules.Activity.Domain.Services.Activity;
using HrManagement.Api.Modules.Activity.Domain.Services.Participant;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Tests.Integration.Activity;

/// <summary>
/// Integration tests for ActivityCommandServiceImpl.
/// Uses shared database from SharedTestFixture.
/// </summary>
[Collection("4_ActivityCommandTests")]
public class ActivityCommandServiceTests
{
    private readonly SharedTestFixture _fixture;

    public ActivityCommandServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region CreateActivityAsync Tests

    [Fact]
    public async Task CreateActivityAsync_ValidActivity_CreatesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Create Valid",
            Type = ActivityType.RUNNING,
            TemplateId = "RUNNING_SIMPLE",
            Description = "Test activity for integration testing",
            BannerUrl = "https://example.com/test-banner.jpg",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Config = config
        };

        // Act
        var result = await service.CreateActivityAsync(activity);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ActivityId);
        Assert.Equal("Test Activity - Create Valid", result.Name);
        Assert.Equal(ActivityStatus.DRAFT, result.Status);
        Assert.Equal("RUNNING_SIMPLE", result.TemplateId);
    }

    [Fact]
    public async Task CreateActivityAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateActivityAsync(activity));
    }

    [Fact]
    public async Task CreateActivityAsync_InvalidTemplate_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Invalid Template",
            TemplateId = "NON_EXISTING_TEMPLATE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateActivityAsync(activity));
    }

    [Fact]
    public async Task CreateActivityAsync_InvalidDates_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Invalid Dates",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 12, 31),
            EndDate = new DateTime(2025, 1, 1) // Before start
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateActivityAsync(activity));
    }

    [Fact]
    public async Task CreateActivityAsync_MissingConfig_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Missing Config",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Config = null // Should fail - RUNNING_SIMPLE requires config
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateActivityAsync(activity));
    }

    #endregion

    #region UpdateActivityAsync Tests

    [Fact]
    public async Task UpdateActivityAsync_ValidUpdate_UpdatesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 3.0,
            ""max_pace"": 12.0,
            ""min_distance_per_log"": 2.0,
            ""max_distance_per_day"": 50.0,
            ""points_per_km"": 20,
            ""bonus_weekend_multiplier"": 2.0
        }");

        var updatedActivity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            ActivityId = "activity-002", // OPEN status
            Name = "New Year Running 2025 - Updated",
            Description = "Updated description",
            StartDate = new DateTime(2025, 2, 1),
            EndDate = new DateTime(2025, 6, 30),
            BannerUrl = "https://example.com/updated-banner.jpg",
            Config = config
        };

        // Act
        var result = await service.UpdateActivityAsync(updatedActivity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Year Running 2025 - Updated", result.Name);
        Assert.Equal("Updated description", result.Description);
    }

    [Fact]
    public async Task UpdateActivityAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();
        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            ActivityId = "non-existing-id",
            Name = "Non Existing",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateActivityAsync(activity));
    }

    #endregion

    #region DeleteActivityAsync Tests (Soft Delete)

    [Fact]
    public async Task DeleteActivityAsync_ExistingActivity_SoftDeletesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();

        // Create a new activity to test delete
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        var activity = await service.CreateActivityAsync(new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Soft Delete Test",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Config = config
        });

        // Act
        await service.DeleteActivityAsync(activity.ActivityId);

        // Assert - Activity should still exist but with DELETED status
        var dbContext = _fixture.CreateDbContext();
        var deleted = await dbContext.Activities.FindAsync(activity.ActivityId);
        Assert.NotNull(deleted); // Record still exists
        Assert.Equal(ActivityStatus.CLOSED, deleted.Status); // But status is CLOSED
    }

    [Fact]
    public async Task DeleteActivityAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteActivityAsync("non-existing-id"));
    }

    [Fact]
    public async Task DeleteActivityAsync_AlreadyDeleted_ChangesStatusAgain()
    {
        // Arrange
        var service = _fixture.GetService<IActivityCommandService>();

        // Create and delete an activity
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        var activity = await service.CreateActivityAsync(new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Double Delete Test",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Config = config
        });

        await service.DeleteActivityAsync(activity.ActivityId);

        // Act - Delete again (should not throw)
        await service.DeleteActivityAsync(activity.ActivityId);

        // Assert - Status is still DELETED
        var dbContext = _fixture.CreateDbContext();
        var deleted = await dbContext.Activities.FindAsync(activity.ActivityId);
        Assert.NotNull(deleted);
        Assert.Equal(ActivityStatus.CLOSED, deleted.Status);
    }

    #endregion

    #region UpdateActivityStatusAsync Tests

    [Fact]
    public async Task UpdateActivityStatusAsync_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var commandService = _fixture.GetService<IActivityCommandService>();

        // Create a new activity to test status update
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        var activity = await commandService.CreateActivityAsync(new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            Name = "Test Activity - Status Test",
            TemplateId = "RUNNING_SIMPLE",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 12, 31),
            Config = config
        });

        // Act - DRAFT -> OPEN
        var result = await commandService.UpdateActivityStatusAsync(activity.ActivityId, ActivityStatus.OPEN);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActivityStatus.OPEN, result.Status);
    }

    #endregion
}
