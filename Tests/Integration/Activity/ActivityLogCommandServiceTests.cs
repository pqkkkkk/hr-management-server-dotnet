using Xunit;
using HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

// Type alias
using ActivityLogEntity = HrManagement.Api.Modules.Activity.Domain.Entities.ActivityLog;

namespace HrManagement.Api.Tests.Integration.Activity;

/// <summary>
/// Integration tests for ActivityLogCommandServiceImpl.
/// Uses shared database from SharedTestFixture.
/// </summary>
[Collection("4_ActivityCommandTests")]
public class ActivityLogCommandServiceTests
{
    private readonly SharedTestFixture _fixture;

    public ActivityLogCommandServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region SubmitLogAsync Tests

    [Fact]
    public async Task SubmitLogAsync_ValidLog_CreatesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // participant-001 is in activity-001 (IN_PROGRESS)
        var log = new ActivityLogEntity
        {
            ParticipantId = "participant-001",
            Distance = 5.0m,
            DurationMinutes = 30,
            ProofUrl = "https://example.com/proof-new.jpg",
            LogDate = DateTime.UtcNow
        };

        // Act
        var result = await service.SubmitLogAsync(log);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ActivityLogId);
        Assert.Equal(ActivityLogStatus.PENDING, result.Status);
        Assert.Equal(5.0m, result.Distance);
    }

    [Fact]
    public async Task SubmitLogAsync_NonExistingParticipant_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();
        var log = new ActivityLogEntity
        {
            ParticipantId = "non-existing-participant",
            Distance = 5.0m,
            DurationMinutes = 30,
            LogDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SubmitLogAsync(log));
    }

    #endregion

    #region ApproveLogAsync Tests

    [Fact]
    public async Task ApproveLogAsync_PendingLog_ApprovesAndUpdatesScore()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();
        var dbContext = _fixture.CreateDbContext();

        // Get initial participant-001 score (log-002 belongs to participant-001)
        // Seed data: participant-001 has total_score = 150
        var participantBefore = await dbContext.Participants.FindAsync("participant-001");
        var initialScore = participantBefore!.TotalScore;

        // log-002 is PENDING, distance = 5km, points_per_km = 10 → expected 50 points
        // Act
        var result = await service.ApproveLogAsync("log-002", "reviewer-001");

        // Assert - Log status updated
        Assert.NotNull(result);
        Assert.Equal(ActivityLogStatus.APPROVED, result.Status);
        Assert.Equal("reviewer-001", result.ReviewerId);

        // Assert - Participant score updated
        var freshContext = _fixture.CreateDbContext();
        var participantAfter = await freshContext.Participants.FindAsync("participant-001");
        Assert.True(participantAfter!.TotalScore > initialScore,
            $"Expected score to increase from {initialScore} but got {participantAfter.TotalScore}");
    }

    [Fact]
    public async Task ApproveLogAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // log-001 is already APPROVED
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveLogAsync("log-001", "reviewer-001"));
    }

    [Fact]
    public async Task ApproveLogAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ApproveLogAsync("non-existing-log", "reviewer-001"));
    }

    #endregion

    #region RejectLogAsync Tests

    [Fact]
    public async Task RejectLogAsync_PendingLog_RejectsWithReason()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // log-005 is PENDING
        // Act
        var result = await service.RejectLogAsync("log-005", "reviewer-001", "Invalid proof");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActivityLogStatus.REJECTED, result.Status);
        Assert.Equal("Invalid proof", result.RejectReason);
        Assert.Equal("reviewer-001", result.ReviewerId);
    }

    [Fact]
    public async Task RejectLogAsync_AlreadyRejected_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // log-003 is already REJECTED
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RejectLogAsync("log-003", "reviewer-001", "Duplicate rejection"));
    }

    #endregion

    #region DeleteLogAsync Tests

    [Fact]
    public async Task DeleteLogAsync_ExistingLog_DeletesSuccessfully()
    {
        // Arrange
        var submitService = _fixture.GetService<IActivityLogCommandService>();

        // First create a log to delete
        var newLog = await submitService.SubmitLogAsync(new ActivityLogEntity
        {
            ParticipantId = "participant-002",
            Distance = 2.0m,
            DurationMinutes = 15,
            LogDate = DateTime.UtcNow
        });

        // Act
        await submitService.DeleteLogAsync(newLog.ActivityLogId);

        // Assert
        var dbContext = _fixture.CreateDbContext();
        var deleted = await dbContext.ActivityLogs.FindAsync(newLog.ActivityLogId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteLogAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IActivityLogCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteLogAsync("non-existing-log"));
    }

    #endregion
}
