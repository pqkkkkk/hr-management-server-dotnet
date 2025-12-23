using Xunit;
using HrManagement.Api.Modules.Reward.Domain.Commands;
using HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;
using Microsoft.EntityFrameworkCore;

namespace HrManagement.Api.Tests.Integration.Reward;

/// <summary>
/// Integration tests for PointTransactionCommandServiceImpl.
/// Uses shared database from RewardTestFixture (via Collection).
/// 
/// IMPORTANT: These tests are for critical business logic and run AFTER Query tests.
/// Uses seed data wallets:
/// - wallet-001: manager-001, program-001, givingBudget=100
/// - wallet-002: employee-001, program-001, personalPoint=500
/// - wallet-003: employee-002, program-001, personalPoint=200
/// - wallet-004: employee-003, program-001, personalPoint=150
/// </summary>
[Collection("2_RewardCommandTests")]
public class PointTransactionCommandServiceTests
{
    private readonly RewardTestFixture _fixture;

    public PointTransactionCommandServiceTests(RewardTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GiftPointsToEmployeesAsync Tests

    [Fact]
    public async Task GiftPoints_ValidCommand_CreatesTransactionsAndUpdatesWallets()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var dbContext = _fixture.CreateDbContext();

        // Get initial balances
        var senderWalletBefore = await dbContext.UserWallets.FindAsync("wallet-001");
        var recipient1Before = await dbContext.UserWallets.FindAsync("wallet-002");
        var recipient2Before = await dbContext.UserWallets.FindAsync("wallet-003");

        var initialSenderBudget = senderWalletBefore!.GivingBudget;
        var initialRecipient1Points = recipient1Before!.PersonalPoint;
        var initialRecipient2Points = recipient2Before!.PersonalPoint;

        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 10),
                new GiftRecipient("employee-002", 15)
            }
        );

        // Act
        var transactions = await service.GiftPointsToEmployeesAsync(command);

        // Assert - Verify transactions created
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count);

        // Verify wallet balances updated (use fresh DbContext to avoid caching)
        var freshContext = _fixture.CreateDbContext();
        var senderAfter = await freshContext.UserWallets.FindAsync("wallet-001");
        var recipient1After = await freshContext.UserWallets.FindAsync("wallet-002");
        var recipient2After = await freshContext.UserWallets.FindAsync("wallet-003");

        Assert.Equal(initialSenderBudget - 25, senderAfter!.GivingBudget); // 10 + 15 = 25
        Assert.Equal(initialRecipient1Points + 10, recipient1After!.PersonalPoint);
        Assert.Equal(initialRecipient2Points + 15, recipient2After!.PersonalPoint);
    }

    [Fact]
    public async Task GiftPoints_EmptyRecipients_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>() // Empty
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_ZeroPoints_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 0) // Invalid: 0 points
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_NegativePoints_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", -10) // Invalid: negative
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_InactiveProgram_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        // program-002 is INACTIVE
        var command = new GiftPointsCommand(
            ProgramId: "program-002",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 10)
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_NonExistingProgram_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "non-existing-program",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 10)
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_SenderNoWallet_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "non-existing-user", // No wallet
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 10)
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_InsufficientBudget_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        // manager-001 has giving_budget ~100 (minus previous test deductions)
        // Request more than available
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("employee-001", 99999) // Way too much
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    [Fact]
    public async Task GiftPoints_RecipientNoWallet_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var command = new GiftPointsCommand(
            ProgramId: "program-001",
            SenderUserId: "manager-001",
            Recipients: new List<GiftRecipient>
            {
                new GiftRecipient("non-existing-employee", 10) // No wallet
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GiftPointsToEmployeesAsync(command));
    }

    #endregion

    #region ExchangePointsAsync Tests

    [Fact]
    public async Task ExchangePoints_ValidExchange_CreatesTransactionAndUpdatesBalances()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var dbContext = _fixture.CreateDbContext();

        // wallet-002 (employee-001) has personalPoint ~500+ (after gift tests)
        // item-002 (Coffee Voucher) requires 100 points, quantity ~50
        var walletBefore = await dbContext.UserWallets.FindAsync("wallet-002");
        var itemBefore = await dbContext.RewardItems.FindAsync("item-002");

        var initialPoints = walletBefore!.PersonalPoint;
        var initialQuantity = itemBefore!.Quantity;

        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-002",
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-002",
                    Quantity = 2  // 2 x 100 = 200 points
                }
            }
        };

        // Act
        var result = await service.ExchangePointsAsync(transaction);

        // Assert - Transaction created
        Assert.NotNull(result);
        Assert.NotNull(result.PointTransactionId);
        Assert.Equal(HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums.TransactionType.EXCHANGE, result.Type);
        Assert.Equal(200, result.Amount); // 2 items x 100 points

        // Verify balances updated
        var freshContext = _fixture.CreateDbContext();
        var walletAfter = await freshContext.UserWallets.FindAsync("wallet-002");
        var itemAfter = await freshContext.RewardItems.FindAsync("item-002");

        Assert.Equal(initialPoints - 200, walletAfter!.PersonalPoint);
        Assert.Equal(initialQuantity - 2, itemAfter!.Quantity);
    }

    [Fact]
    public async Task ExchangePoints_EmptyItems_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-002",
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>() // Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_NoDestinationWallet_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = null, // Missing
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-002",
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_WalletNotFound_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "non-existing-wallet",
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-002",
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_InsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        // item-003 (Extra Leave Day) requires 1000 points
        // wallet-004 (employee-003) only has ~150 points
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-004",
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-003", // Requires 1000 points
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_InsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        // item-003 (Extra Leave Day) has quantity = 5
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-002", // Has enough points
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-003",
                    Quantity = 999 // Way more than available (5)
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_ItemNotFound_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-002",
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "non-existing-item",
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExchangePointsAsync(transaction));
    }

    [Fact]
    public async Task ExchangePoints_ItemFromDifferentProgram_ThrowsArgumentException()
    {
        // Arrange
        var service = _fixture.GetService<IPointTransactionCommandService>();
        // wallet-002 is for program-001
        // item-004 is for program-002
        var transaction = new HrManagement.Api.Modules.Reward.Domain.Entities.PointTransaction
        {
            DestinationWalletId = "wallet-002", // program-001
            Items = new List<HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction>
            {
                new HrManagement.Api.Modules.Reward.Domain.Entities.ItemInTransaction
                {
                    RewardItemId = "item-004", // program-002
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExchangePointsAsync(transaction));
    }

    #endregion
}
