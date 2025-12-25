using Xunit;
using HrManagement.Api.Modules.Activity.Domain.Services.Participant;
using HrManagement.Api.Tests.Integration;
using static HrManagement.Api.Modules.Activity.Domain.Entities.ActivityEnums;

namespace HrManagement.Api.Tests.Integration.Activity;

/// <summary>
/// Integration tests for ParticipantCommandServiceImpl.
/// Uses shared database from SharedTestFixture.
/// </summary>
[Collection("4_ActivityCommandTests")]
public class ParticipantCommandServiceTests
{
    private readonly SharedTestFixture _fixture;

    public ParticipantCommandServiceTests(SharedTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region RegisterParticipantAsync Tests

    [Fact]
    public async Task RegisterParticipantAsync_ValidRegistration_CreatesParticipant()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // activity-002 is OPEN for registration
        // Act
        var result = await service.RegisterParticipantAsync("activity-002", "new-employee-001");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ParticipantId);
        Assert.Equal("activity-002", result.ActivityId);
        Assert.Equal("new-employee-001", result.EmployeeId);
        Assert.Equal(ParticipantStatus.ACTIVE, result.Status);
        Assert.Equal(0, result.TotalScore);
    }

    [Fact]
    public async Task RegisterParticipantAsync_AlreadyRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // employee-001 is already in activity-001
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterParticipantAsync("activity-001", "employee-001"));
    }

    [Fact]
    public async Task RegisterParticipantAsync_ActivityNotOpen_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // activity-003 is COMPLETED
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterParticipantAsync("activity-003", "new-employee-002"));
    }

    [Fact]
    public async Task RegisterParticipantAsync_NonExistingActivity_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RegisterParticipantAsync("non-existing-activity", "employee-001"));
    }

    #endregion

    #region RemoveParticipantAsync Tests

    [Fact]
    public async Task RemoveParticipantAsync_ExistingParticipant_RemovesSuccessfully()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // First register a participant to remove
        var participant = await service.RegisterParticipantAsync("activity-002", "temp-employee-remove");

        // Act
        await service.RemoveParticipantAsync("activity-002", "temp-employee-remove");

        // Assert
        var dbContext = _fixture.CreateDbContext();
        var deleted = await dbContext.Participants.FindAsync(participant.ParticipantId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task RemoveParticipantAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RemoveParticipantAsync("activity-001", "non-existing-employee"));
    }

    #endregion

    #region UpdateParticipantStatusAsync Tests

    [Fact]
    public async Task UpdateParticipantStatusAsync_ValidChange_UpdatesStatus()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // Register a new participant to test status update
        var participant = await service.RegisterParticipantAsync("activity-002", "status-test-employee");

        // Act
        var result = await service.UpdateParticipantStatusAsync(participant.ParticipantId, ParticipantStatus.DISQUALIFIED);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ParticipantStatus.DISQUALIFIED, result.Status);
    }

    [Fact]
    public async Task UpdateParticipantStatusAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        // Arrange
        var service = _fixture.GetService<IParticipantCommandService>();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateParticipantStatusAsync("non-existing-participant", ParticipantStatus.DISQUALIFIED));
    }

    #endregion
}
