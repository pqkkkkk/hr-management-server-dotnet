using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

namespace HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;

/// <summary>
/// Implementation of IRewardProgramCommandService.
/// Handles create/update operations for reward programs.
/// </summary>
public class RewardProgramCommandServiceImpl : IRewardProgramCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IRewardProgramDao _rewardProgramDao;
    private readonly IUserWalletDao _userWalletDao;

    public RewardProgramCommandServiceImpl(
        AppDbContext dbContext,
        IRewardProgramDao rewardProgramDao,
        IUserWalletDao userWalletDao)
    {
        _dbContext = dbContext;
        _rewardProgramDao = rewardProgramDao;
        _userWalletDao = userWalletDao;
    }

    #region CreateRewardProgram

    /// <summary>
    /// Creates a new reward program with its items and policies.
    /// Also auto-creates wallets for all users in the system.
    /// </summary>
    public async Task<RewardProgram> CreateRewardProgramAsync(RewardProgram program)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            ValidateRewardProgram(program);
            PrepareRewardProgram(program);

            // Save the reward program (cascade saves items and policies)
            var createdProgram = await _rewardProgramDao.CreateAsync(program);

            // Auto-create wallets for all users
            await CreateWalletsForAllUsersAsync(createdProgram);

            await transaction.CommitAsync();
            return createdProgram;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private void ValidateRewardProgram(RewardProgram program)
    {
        if (string.IsNullOrWhiteSpace(program.Name))
        {
            throw new ArgumentException("Program name is required.");
        }

        if (program.EndDate <= program.StartDate)
        {
            throw new ArgumentException("End date must be after start date.");
        }

        if (program.DefaultGivingBudget < 0)
        {
            throw new ArgumentException("Default giving budget cannot be negative.");
        }

        ValidateRewardItems(program.RewardItems);
        ValidatePolicies(program.Policies);
    }

    private void ValidateRewardItems(ICollection<RewardItem> items)
    {
        // Items are required - program must have at least one reward to exchange
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("Reward program must have at least one reward item.");
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentException("Reward item name is required.");
            }

            if (item.RequiredPoints <= 0)
            {
                throw new ArgumentException($"Required points for item '{item.Name}' must be positive.");
            }

            if (item.Quantity < 0)
            {
                throw new ArgumentException($"Quantity for item '{item.Name}' cannot be negative.");
            }
        }
    }

    private void ValidatePolicies(ICollection<RewardProgramPolicy> policies)
    {
        foreach (var policy in policies)
        {
            if (policy.UnitValue <= 0)
            {
                throw new ArgumentException("Unit value for policy must be positive.");
            }

            if (policy.PointsPerUnit <= 0)
            {
                throw new ArgumentException("Points per unit for policy must be positive.");
            }
        }
    }

    private void PrepareRewardProgram(RewardProgram program)
    {
        // Note: RewardProgramId will be set by DAO layer
        program.Status = ProgramStatus.ACTIVE;

        // Link items to program (IDs will be set by DAO layer)
        foreach (var item in program.RewardItems)
        {
            item.ProgramId = program.RewardProgramId;
        }

        // Link policies to program (IDs will be set by DAO layer)
        foreach (var policy in program.Policies)
        {
            policy.ProgramId = program.RewardProgramId;
            policy.IsActive = true;
        }
    }

    /// <summary>
    /// Creates wallets for all users in the system.
    /// TODO: Implement external API call to get user list from Spring Boot.
    /// </summary>
    private async Task CreateWalletsForAllUsersAsync(RewardProgram program)
    {
        // TODO: Replace this placeholder with actual API call to Spring Boot service
        // 
        // Implementation outline:
        // 1. Inject HttpClient or create UserApiClient service
        // 2. Call Spring Boot API: GET /api/users?role=EMPLOYEE,MANAGER
        // 3. For each user:
        //    - If role is MANAGER: create wallet with giving_budget = program.DefaultGivingBudget
        //    - If role is EMPLOYEE: create wallet with giving_budget = 0
        //    - All wallets start with personal_point = 0
        //
        // Example implementation:
        // var users = await _userApiClient.GetAllUsersAsync();
        // var wallets = users.Select(user => new UserWallet
        // {
        //     UserId = user.Id,
        //     ProgramId = program.RewardProgramId,
        //     PersonalPoint = 0,
        //     GivingBudget = user.Role == "MANAGER" ? program.DefaultGivingBudget : 0
        // }).ToList();
        // await _userWalletDao.CreateBatchAsync(wallets);

        Console.WriteLine($"[PLACEHOLDER] Auto-create wallets for program {program.RewardProgramId} - Not implemented yet.");
        Console.WriteLine("To implement: Call Spring Boot API to get all users and create wallets.");

        await Task.CompletedTask;
    }

    #endregion

    #region DeactivateRewardProgram

    /// <summary>
    /// Deactivates a reward program. Points become frozen (no gift/exchange).
    /// </summary>
    public async Task DeactivateRewardProgramAsync(string programId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var program = await GetAndValidateProgramForDeactivation(programId);
            DeactivateProgram(program);

            await _rewardProgramDao.UpdateAsync(program);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<RewardProgram> GetAndValidateProgramForDeactivation(string programId)
    {
        var program = await _rewardProgramDao.GetByIdAsync(programId);

        if (program == null)
        {
            throw new ArgumentException($"Reward program with ID {programId} not found.");
        }

        if (program.Status == ProgramStatus.INACTIVE)
        {
            throw new InvalidOperationException($"Reward program {programId} is already inactive.");
        }

        return program;
    }

    private void DeactivateProgram(RewardProgram program)
    {
        program.Status = ProgramStatus.INACTIVE;

        // Deactivate all policies as well
        foreach (var policy in program.Policies)
        {
            policy.IsActive = false;
        }
    }

    #endregion

    #region UpdateRewardProgram

    /// <summary>
    /// Updates an existing reward program.
    /// The entity should already contain updated values from controller mapping.
    /// </summary>
    public async Task<RewardProgram> UpdateRewardProgramAsync(RewardProgram program)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Validate program exists
            var existingProgram = await _rewardProgramDao.GetByIdAsync(program.RewardProgramId)
                ?? throw new KeyNotFoundException($"Reward program with ID {program.RewardProgramId} not found.");

            // Validate dates
            if (program.EndDate <= program.StartDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            // Validate items
            if (program.RewardItems.Count == 0)
            {
                throw new ArgumentException("Reward program must have at least one reward item.");
            }

            // Handle items update - remove old, add new
            var existingItems = existingProgram.RewardItems.ToList();
            foreach (var item in existingItems)
            {
                _dbContext.Set<RewardItem>().Remove(item);
            }
            foreach (var item in program.RewardItems)
            {
                item.ProgramId = program.RewardProgramId;
                if (string.IsNullOrEmpty(item.RewardItemId))
                {
                    item.RewardItemId = Guid.NewGuid().ToString();
                }
            }

            // Handle policies update - remove old, add new
            var existingPolicies = existingProgram.Policies.ToList();
            foreach (var policy in existingPolicies)
            {
                _dbContext.Set<RewardProgramPolicy>().Remove(policy);
            }
            foreach (var policy in program.Policies)
            {
                policy.ProgramId = program.RewardProgramId;
                if (string.IsNullOrEmpty(policy.PolicyId))
                {
                    policy.PolicyId = Guid.NewGuid().ToString();
                }
            }

            // Update the program
            _dbContext.Entry(existingProgram).CurrentValues.SetValues(program);
            existingProgram.RewardItems = program.RewardItems;
            existingProgram.Policies = program.Policies;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return existingProgram;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    #endregion

    #region DeleteRewardProgram

    /// <summary>
    /// Deletes a reward program by ID.
    /// </summary>
    public async Task DeleteRewardProgramAsync(string programId)
    {
        var program = await _rewardProgramDao.GetByIdAsync(programId)
            ?? throw new KeyNotFoundException($"Reward program with ID {programId} not found.");

        // Check if program has any transactions (business rule: can't delete if has transactions)
        // Transactions are linked to program via wallets
        var hasTransactions = await _dbContext.PointTransactions
            .AnyAsync(t =>
                (t.SourceWallet != null && t.SourceWallet.ProgramId == programId) ||
                (t.DestinationWallet != null && t.DestinationWallet.ProgramId == programId));

        if (hasTransactions)
        {
            throw new InvalidOperationException("Cannot delete a reward program that has transactions. Consider deactivating instead.");
        }

        await _rewardProgramDao.DeleteAsync(programId);
    }

    #endregion
}
