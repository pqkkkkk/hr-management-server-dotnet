using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Commands;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;

/// <summary>
/// Implementation of IPointTransactionCommandService.
/// Handles point gift, exchange, and policy reward operations.
/// </summary>
public class PointTransactionCommandServiceImpl : IPointTransactionCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IPointTransactionDao _pointTransactionDao;
    private readonly IUserWalletDao _userWalletDao;
    private readonly IRewardItemDao _rewardItemDao;
    private readonly IRewardProgramDao _rewardProgramDao;

    public PointTransactionCommandServiceImpl(
        AppDbContext dbContext,
        IPointTransactionDao pointTransactionDao,
        IUserWalletDao userWalletDao,
        IRewardItemDao rewardItemDao,
        IRewardProgramDao rewardProgramDao)
    {
        _dbContext = dbContext;
        _pointTransactionDao = pointTransactionDao;
        _userWalletDao = userWalletDao;
        _rewardItemDao = rewardItemDao;
        _rewardProgramDao = rewardProgramDao;
    }

    #region GiftPointsToEmployees

    /// <summary>
    /// Manager gifts points to employees.
    /// Deducts from sender's giving_budget, adds to recipients' personal_point.
    /// </summary>
    public async Task<List<PointTransaction>> GiftPointsToEmployeesAsync(GiftPointsCommand command)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            ValidateGiftPointsCommand(command);

            var program = await ValidateAndGetActiveProgram(command.ProgramId);
            var senderWallet = await ValidateAndGetSenderWallet(command);
            var recipientWallets = await ValidateAndGetRecipientWallets(command);

            var transactions = CreateGiftTransactions(command, senderWallet, recipientWallets);
            UpdateWalletsForGift(command, senderWallet, recipientWallets);

            await PersistGiftPointsChanges(senderWallet, recipientWallets.Values.ToList(), transactions);

            await transaction.CommitAsync();
            return transactions;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private void ValidateGiftPointsCommand(GiftPointsCommand command)
    {
        if (command.Recipients == null || command.Recipients.Count == 0)
        {
            throw new ArgumentException("At least one recipient is required.");
        }

        foreach (var recipient in command.Recipients)
        {
            if (recipient.Points <= 0)
            {
                throw new ArgumentException($"Points to gift must be positive. Got {recipient.Points} for user {recipient.UserId}.");
            }
        }
    }

    private async Task<RewardProgram> ValidateAndGetActiveProgram(string programId)
    {
        var program = await _rewardProgramDao.GetByIdAsync(programId);
        if (program == null)
        {
            throw new ArgumentException($"Reward program with ID {programId} not found.");
        }

        if (program.Status != ProgramStatus.ACTIVE)
        {
            throw new InvalidOperationException($"Cannot perform operation. Program '{program.Name}' is not active.");
        }

        return program;
    }

    private async Task<UserWallet> ValidateAndGetSenderWallet(GiftPointsCommand command)
    {
        var senderWallet = await _userWalletDao.GetByUserIdAndProgramIdAsync(command.SenderUserId, command.ProgramId);
        if (senderWallet == null)
        {
            throw new ArgumentException($"Sender wallet not found for user {command.SenderUserId} in program {command.ProgramId}.");
        }

        int totalPointsToGift = command.Recipients.Sum(r => r.Points);
        if (senderWallet.GivingBudget < totalPointsToGift)
        {
            throw new InvalidOperationException(
                $"Insufficient giving budget. Available: {senderWallet.GivingBudget}, Required: {totalPointsToGift}.");
        }

        return senderWallet;
    }

    private async Task<Dictionary<string, UserWallet>> ValidateAndGetRecipientWallets(GiftPointsCommand command)
    {
        var recipientWallets = new Dictionary<string, UserWallet>();

        foreach (var recipient in command.Recipients)
        {
            var recipientWallet = await _userWalletDao.GetByUserIdAndProgramIdAsync(recipient.UserId, command.ProgramId);
            if (recipientWallet == null)
            {
                throw new ArgumentException($"Recipient wallet not found for user {recipient.UserId} in program {command.ProgramId}.");
            }

            recipientWallets[recipient.UserId] = recipientWallet;
        }

        return recipientWallets;
    }

    private List<PointTransaction> CreateGiftTransactions(
        GiftPointsCommand command,
        UserWallet senderWallet,
        Dictionary<string, UserWallet> recipientWallets)
    {
        var transactions = new List<PointTransaction>();

        foreach (var recipient in command.Recipients)
        {
            var recipientWallet = recipientWallets[recipient.UserId];

            // Note: PointTransactionId will be set by DAO layer
            var transaction = new PointTransaction
            {
                Type = TransactionType.GIFT,
                Amount = recipient.Points,
                SourceWalletId = senderWallet.UserWalletId,
                DestinationWalletId = recipientWallet.UserWalletId,
                CreatedAt = DateTime.UtcNow
            };

            transactions.Add(transaction);
        }

        return transactions;
    }

    private void UpdateWalletsForGift(
        GiftPointsCommand command,
        UserWallet senderWallet,
        Dictionary<string, UserWallet> recipientWallets)
    {
        int totalPointsToGift = command.Recipients.Sum(r => r.Points);
        senderWallet.GivingBudget -= totalPointsToGift;

        foreach (var recipient in command.Recipients)
        {
            recipientWallets[recipient.UserId].PersonalPoint += recipient.Points;
        }
    }

    private async Task PersistGiftPointsChanges(
        UserWallet senderWallet,
        List<UserWallet> recipientWallets,
        List<PointTransaction> transactions)
    {
        await _userWalletDao.UpdateAsync(senderWallet);

        foreach (var recipientWallet in recipientWallets)
        {
            await _userWalletDao.UpdateAsync(recipientWallet);
        }

        await _pointTransactionDao.CreateBatchAsync(transactions);
    }

    #endregion

    #region ExchangePoints

    /// <summary>
    /// Employee exchanges points for reward items.
    /// Validates balance and stock, then creates transaction with items.
    /// </summary>
    public async Task<PointTransaction> ExchangePointsAsync(PointTransaction transaction)
    {
        using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            ValidateExchangeTransactionInfo(transaction);

            var userWallet = await GetAndValidateUserWallet(transaction.DestinationWalletId!);
            await ValidateAndGetActiveProgram(userWallet.ProgramId);

            var rewardItems = await ValidateAndGetRewardItems(transaction, userWallet.ProgramId);
            float totalPointsRequired = CalculateAndSetItemPoints(transaction, rewardItems);

            ValidateUserBalance(userWallet, totalPointsRequired);
            PrepareTransaction(transaction, totalPointsRequired);
            UpdateEntitiesForExchange(userWallet, rewardItems, transaction, totalPointsRequired);

            await PersistExchangeChanges(userWallet, rewardItems, transaction);

            await dbTransaction.CommitAsync();
            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private void ValidateExchangeTransactionInfo(PointTransaction transaction)
    {
        if (transaction.Items == null || transaction.Items.Count == 0)
        {
            throw new ArgumentException("At least one item is required for exchange.");
        }

        if (string.IsNullOrEmpty(transaction.DestinationWalletId))
        {
            throw new ArgumentException("Destination wallet ID is required.");
        }
    }

    private async Task<UserWallet> GetAndValidateUserWallet(string walletId)
    {
        var userWallet = await _userWalletDao.GetByIdAsync(walletId);
        if (userWallet == null)
        {
            throw new ArgumentException($"Wallet with ID {walletId} not found.");
        }
        return userWallet;
    }

    private async Task<Dictionary<string, RewardItem>> ValidateAndGetRewardItems(
        PointTransaction transaction,
        string programId)
    {
        var itemIds = transaction.Items.Select(i => i.RewardItemId).ToList();
        var rewardItems = await _rewardItemDao.GetByIdsAsync(itemIds);

        if (rewardItems.Count != itemIds.Count)
        {
            var foundIds = rewardItems.Select(r => r.RewardItemId).ToHashSet();
            var missingIds = itemIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new ArgumentException($"Reward items not found: {string.Join(", ", missingIds)}");
        }

        var rewardItemLookup = rewardItems.ToDictionary(r => r.RewardItemId);

        // Validate each item belongs to program and has sufficient quantity
        foreach (var itemInTx in transaction.Items)
        {
            var rewardItem = rewardItemLookup[itemInTx.RewardItemId];

            if (rewardItem.ProgramId != programId)
            {
                throw new ArgumentException($"Item '{rewardItem.Name}' does not belong to the program.");
            }

            if (rewardItem.Quantity < itemInTx.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for item '{rewardItem.Name}'. Available: {rewardItem.Quantity}, Requested: {itemInTx.Quantity}.");
            }
        }

        return rewardItemLookup;
    }

    private float CalculateAndSetItemPoints(
        PointTransaction transaction,
        Dictionary<string, RewardItem> rewardItemLookup)
    {
        float totalPointsRequired = 0;

        foreach (var itemInTx in transaction.Items)
        {
            var rewardItem = rewardItemLookup[itemInTx.RewardItemId];
            float itemTotalPoints = rewardItem.RequiredPoints * itemInTx.Quantity;
            itemInTx.TotalPoints = (int)itemTotalPoints;
            totalPointsRequired += itemTotalPoints;
        }

        return totalPointsRequired;
    }

    private void ValidateUserBalance(UserWallet userWallet, float totalPointsRequired)
    {
        if (userWallet.PersonalPoint < totalPointsRequired)
        {
            throw new InvalidOperationException(
                $"Insufficient points. Available: {userWallet.PersonalPoint}, Required: {totalPointsRequired}.");
        }
    }

    private void PrepareTransaction(PointTransaction transaction, float totalPointsRequired)
    {
        // Note: PointTransactionId will be set by DAO layer
        transaction.Type = TransactionType.EXCHANGE;
        transaction.Amount = totalPointsRequired;
        transaction.SourceWalletId = null;
        transaction.CreatedAt = DateTime.UtcNow;

        // Note: ItemInTransactionId will be set by DAO layer
        foreach (var itemInTx in transaction.Items)
        {
            itemInTx.TransactionId = transaction.PointTransactionId;
        }
    }

    private void UpdateEntitiesForExchange(
        UserWallet userWallet,
        Dictionary<string, RewardItem> rewardItemLookup,
        PointTransaction transaction,
        float totalPointsRequired)
    {
        // Deduct user's personal points
        userWallet.PersonalPoint -= (int)totalPointsRequired;

        // Deduct quantities from reward items
        foreach (var itemInTx in transaction.Items)
        {
            rewardItemLookup[itemInTx.RewardItemId].Quantity -= itemInTx.Quantity;
        }
    }

    private async Task PersistExchangeChanges(
        UserWallet userWallet,
        Dictionary<string, RewardItem> rewardItemLookup,
        PointTransaction transaction)
    {
        await _userWalletDao.UpdateAsync(userWallet);
        await _rewardItemDao.UpdateBatchAsync(rewardItemLookup.Values.ToList());
        await _pointTransactionDao.CreateAsync(transaction);
    }

    #endregion

    #region CronJobAutoDistributePoints

    /// <summary>
    /// Cron job: auto distribute points based on reward program policies (WEEKLY).
    /// </summary>
    public async Task ProcessRewardProgramPolicyAsync()
    {
        // TODO: Implement policy-based point distribution
        // This will be called by a cron job weekly
        // 
        // Logic outline:
        // 1. Get all active reward programs with active policies
        // 2. For each policy, based on PolicyType:
        //    - NOT_LATE: Query attendance data from Spring Boot, count days with lateMinutes == 0
        //    - OVERTIME: Query attendance data, sum overtimeMinutes
        //    - FULL_ATTENDANCE: Query attendance data, count days with morning + afternoon = PRESENT
        // 3. Calculate points: (AchievedValue / UnitValue) × PointsPerUnit
        // 4. Create POLICY_REWARD transactions for each user
        // 5. Update user personal_point balances
        //
        // This requires:
        // - Integration with Spring Boot attendance API
        // - Scheduled job configuration (e.g., Hangfire, Quartz)

        Console.WriteLine("[PLACEHOLDER] ProcessRewardProgramPolicyAsync - Not implemented yet.");
        Console.WriteLine("To implement: Query attendance data from Spring Boot and distribute points based on policies.");

        await Task.CompletedTask;
    }

    #endregion
}
