using Xunit;
using System.Text.Json;
using HrManagement.Api.Modules.Activity.Domain.Entities;
using HrManagement.Api.Modules.Activity.Domain.Services.Template;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Tests.Unit.Activity;

/// <summary>
/// Unit tests for RunningSimpleConfigProvider.
/// Tests validation logic, point calculation, and config schema.
/// </summary>
public class RunningSimpleConfigProviderTests
{
    private readonly RunningSimpleConfigProvider _provider;

    public RunningSimpleConfigProviderTests()
    {
        _provider = new RunningSimpleConfigProvider();
    }

    #region Properties Tests

    [Fact]
    public void TemplateId_ReturnsCorrectValue()
    {
        Assert.Equal("RUNNING_SIMPLE", _provider.TemplateId);
    }

    [Fact]
    public void TemplateName_ReturnsCorrectValue()
    {
        Assert.Equal("Running - Simple Points", _provider.TemplateName);
    }

    [Fact]
    public void Type_ReturnsRunning()
    {
        Assert.Equal(ActivityType.RUNNING, _provider.Type);
    }

    [Fact]
    public void GetConfigSchema_ReturnsAllRequiredFields()
    {
        // Act
        var schema = _provider.GetConfigSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal(6, schema.Fields.Count);

        var fieldNames = schema.Fields.Select(f => f.Name).ToList();
        Assert.Contains("min_pace", fieldNames);
        Assert.Contains("max_pace", fieldNames);
        Assert.Contains("min_distance_per_log", fieldNames);
        Assert.Contains("max_distance_per_day", fieldNames);
        Assert.Contains("points_per_km", fieldNames);
        Assert.Contains("bonus_weekend_multiplier", fieldNames);
    }

    #endregion

    #region ValidateConfig Tests

    [Fact]
    public void ValidateConfig_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        // Act
        var result = _provider.ValidateConfig(config);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateConfig_NullConfig_ReturnsFail()
    {
        // Act
        var result = _provider.ValidateConfig(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("required", result.ErrorMessage?.ToLower());
    }

    [Fact]
    public void ValidateConfig_MissingRequiredField_ReturnsFail()
    {
        // Arrange - missing points_per_km
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""bonus_weekend_multiplier"": 1.5
        }");

        // Act
        var result = _provider.ValidateConfig(config);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("points_per_km", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConfig_MinPaceGreaterThanMaxPace_ReturnsFail()
    {
        // Arrange
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 20.0,
            ""max_pace"": 5.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 10,
            ""bonus_weekend_multiplier"": 1.5
        }");

        // Act
        var result = _provider.ValidateConfig(config);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("pace", result.ErrorMessage?.ToLower());
    }

    [Fact]
    public void ValidateConfig_PointsPerKmZero_ReturnsFail()
    {
        // Arrange
        var config = JsonDocument.Parse(@"{
            ""min_pace"": 4.0,
            ""max_pace"": 15.0,
            ""min_distance_per_log"": 1.0,
            ""max_distance_per_day"": 42.0,
            ""points_per_km"": 0,
            ""bonus_weekend_multiplier"": 1.5
        }");

        // Act
        var result = _provider.ValidateConfig(config);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Điểm/km", result.ErrorMessage);
    }

    #endregion

    #region ValidateLog Tests

    [Fact]
    public void ValidateLog_ValidData_ReturnsSuccess()
    {
        // Arrange
        var log = CreateActivityLog(distance: 5.0m, durationMinutes: 30);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateLog_DistanceTooShort_ReturnsFail()
    {
        // Arrange - min_distance_per_log default is 1.0 km
        var log = CreateActivityLog(distance: 0.5m, durationMinutes: 5);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Khoảng cách tối thiểu", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLog_DistanceTooLong_ReturnsFail()
    {
        // Arrange - max_distance_per_day default is 42 km
        var log = CreateActivityLog(distance: 50.0m, durationMinutes: 300);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Khoảng cách tối đa", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLog_PaceTooFast_ReturnsFail()
    {
        // Arrange - min_pace default is 4.0 min/km, 10km in 20min = 2 min/km (too fast)
        var log = CreateActivityLog(distance: 10.0m, durationMinutes: 20);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pace quá nhanh", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLog_PaceTooSlow_ReturnsFail()
    {
        // Arrange - max_pace default is 15.0 min/km, 5km in 100min = 20 min/km (too slow)
        var log = CreateActivityLog(distance: 5.0m, durationMinutes: 100);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pace quá chậm", result.ErrorMessage);
    }

    [Fact]
    public void ValidateLog_NullConfig_ReturnsValidationError()
    {
        // Arrange
        var log = CreateActivityLog(distance: 5.0m, durationMinutes: 30, useNullConfig: true);

        // Act
        var result = _provider.ValidateLog(log);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("config không được cấu hình", result.ErrorMessage);
    }

    #endregion

    #region CalculatePoints Tests

    [Fact]
    public void CalculatePoints_WeekdayRun_ReturnsBasePoints()
    {
        // Arrange - 5km * 10 points/km = 50 points on weekday
        var log = CreateActivityLog(
            distance: 5.0m,
            durationMinutes: 30,
            logDate: new DateTime(2024, 12, 23)); // Monday

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        Assert.Equal(50, points);
    }

    [Fact]
    public void CalculatePoints_WeekendRun_AppliesBonus()
    {
        // Arrange - 5km * 10 points/km * 1.5 bonus = 75 points on Saturday
        var log = CreateActivityLog(
            distance: 5.0m,
            durationMinutes: 30,
            logDate: new DateTime(2024, 12, 21)); // Saturday

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        Assert.Equal(75, points);
    }

    [Fact]
    public void CalculatePoints_SundayRun_AppliesBonus()
    {
        // Arrange - 10km * 10 points/km * 1.5 bonus = 150 points on Sunday
        var log = CreateActivityLog(
            distance: 10.0m,
            durationMinutes: 60,
            logDate: new DateTime(2024, 12, 22)); // Sunday

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        Assert.Equal(150, points);
    }

    [Fact]
    public void CalculatePoints_DistanceExceedsMax_CapsAtMax()
    {
        // Arrange - 50km but max is 42km, so 42 * 10 = 420 points
        var log = CreateActivityLog(
            distance: 50.0m,
            durationMinutes: 300,
            logDate: new DateTime(2024, 12, 23)); // Monday

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        Assert.Equal(420, points);
    }

    [Fact]
    public void CalculatePoints_NullConfig_UsesDefaultFormula()
    {
        // Arrange - fallback is distance * 10
        var log = CreateActivityLog(distance: 5.0m, durationMinutes: 30, useNullConfig: true);

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        Assert.Equal(50, points);
    }

    [Fact]
    public void CalculatePoints_CustomConfig_UsesCustomValues()
    {
        // Arrange - custom config: 20 points/km, 2.0 weekend multiplier
        var customConfig = JsonDocument.Parse(@"{
            ""points_per_km"": 20,
            ""bonus_weekend_multiplier"": 2.0,
            ""max_distance_per_day"": 42
        }");

        var log = CreateActivityLog(
            distance: 5.0m,
            durationMinutes: 30,
            logDate: new DateTime(2024, 12, 21), // Saturday
            config: customConfig);

        // Act
        var points = _provider.CalculatePoints(log);

        // Assert
        // 5km * 20 points/km * 2.0 weekend = 200 points
        Assert.Equal(200, points);
    }

    #endregion

    #region Helper Methods

    private static ActivityLog CreateActivityLog(
        decimal distance,
        int durationMinutes,
        DateTime? logDate = null,
        JsonDocument? config = null,
        bool useNullConfig = false)
    {
        // Create default config if not provided and not explicitly requesting null
        JsonDocument? activityConfig = null;
        if (!useNullConfig)
        {
            activityConfig = config ?? JsonDocument.Parse(@"{
                ""min_pace"": 4.0,
                ""max_pace"": 15.0,
                ""min_distance_per_log"": 1.0,
                ""max_distance_per_day"": 42.0,
                ""points_per_km"": 10,
                ""bonus_weekend_multiplier"": 1.5
            }");
        }

        var activity = new HrManagement.Api.Modules.Activity.Domain.Entities.Activity
        {
            ActivityId = "test-activity",
            Name = "Test Running Activity",
            TemplateId = "RUNNING_SIMPLE",
            Config = activityConfig
        };

        var participant = new Participant
        {
            ParticipantId = "test-participant",
            ActivityId = activity.ActivityId,
            EmployeeId = "test-employee",
            Activity = activity
        };

        return new ActivityLog
        {
            ActivityLogId = "test-log",
            ParticipantId = participant.ParticipantId,
            Distance = distance,
            DurationMinutes = durationMinutes,
            LogDate = logDate ?? DateTime.UtcNow,
            Participant = participant
        };
    }

    #endregion
}
