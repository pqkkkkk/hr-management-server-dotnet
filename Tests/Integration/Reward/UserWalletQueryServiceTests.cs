using Xunit;
using HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for UserWalletQueryServiceImpl.
/// Uses shared database from RewardTestFixture (via Collection).
/// 
/// Sample data (from M202512_009_SeedSampleData):
/// - wallet-001: manager-001, program-001, personalPoint=0, givingBudget=100
/// - wallet-002: employee-001, program-001, personalPoint=500, givingBudget=0
/// - wallet-003: employee-002, program-001, personalPoint=200, givingBudget=0
/// - wallet-004: employee-003, program-001, personalPoint=150, givingBudget=0
/// - wallet-005: manager-001, program-002, personalPoint=0, givingBudget=200
/// - wallet-006: employee-001, program-002, personalPoint=100, givingBudget=0
/// 
/// These tests are READ-ONLY.
/// </summary>
[Collection("1_RewardQueryTests")]
public class UserWalletQueryServiceTests
{
    private readonly RewardTestFixture _fixture;

    public UserWalletQueryServiceTests(RewardTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetWalletByUserAndProgramAsync Tests

    [Fact]
    public async Task GetWalletByUserAndProgram_ExistingWallet_ReturnsWalletWithProgram()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();
        var userId = "employee-001";
        var programId = "program-001";

        // Act
        var result = await service.GetWalletByUserAndProgramAsync(userId, programId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("wallet-002", result.UserWalletId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(programId, result.ProgramId);
        Assert.Equal(500, result.PersonalPoint);
        Assert.Equal(0, result.GivingBudget);

        // Verify Program is loaded
        Assert.NotNull(result.Program);
        Assert.Equal("Q4 2024 Employee Recognition", result.Program.Name);
    }

    [Fact]
    public async Task GetWalletByUserAndProgram_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act
        var result = await service.GetWalletByUserAndProgramAsync("non-existing-user", "program-001");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWalletByUserAndProgram_NonExistingProgram_ReturnsNull()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act
        var result = await service.GetWalletByUserAndProgramAsync("employee-001", "non-existing-program");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWalletByUserAndProgram_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetWalletByUserAndProgramAsync("", "program-001"));
    }

    [Fact]
    public async Task GetWalletByUserAndProgram_EmptyProgramId_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetWalletByUserAndProgramAsync("employee-001", ""));
    }

    #endregion

    #region GetWalletsByUserIdAsync Tests

    [Fact]
    public async Task GetWalletsByUserId_UserWithMultipleWallets_ReturnsAllWallets()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();
        // employee-001 has 2 wallets: wallet-002 (program-001), wallet-006 (program-002)
        var userId = "employee-001";

        // Act
        var result = await service.GetWalletsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, wallet => Assert.Equal(userId, wallet.UserId));

        // Verify both programs are represented
        var programIds = result.Select(w => w.ProgramId).ToList();
        Assert.Contains("program-001", programIds);
        Assert.Contains("program-002", programIds);
    }

    [Fact]
    public async Task GetWalletsByUserId_UserWithSingleWallet_ReturnsSingleWallet()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();
        // employee-002 has only 1 wallet: wallet-003 (program-001)
        var userId = "employee-002";

        // Act
        var result = await service.GetWalletsByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("wallet-003", result[0].UserWalletId);
        Assert.Equal(userId, result[0].UserId);
    }

    [Fact]
    public async Task GetWalletsByUserId_NonExistingUser_ReturnsEmptyList()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act
        var result = await service.GetWalletsByUserIdAsync("non-existing-user");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWalletsByUserId_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IUserWalletQueryService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetWalletsByUserIdAsync(""));
    }

    #endregion
}
