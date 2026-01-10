using Microsoft.EntityFrameworkCore;
using HrManagement.Api.Data;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Entities;
using static HrManagement.Api.Modules.Reward.Domain.Entities.RewardEnums;

using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices;

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
    private readonly ISpringBootApiClient _springBootApiClient;

    public RewardProgramCommandServiceImpl(
        AppDbContext dbContext,
        IRewardProgramDao rewardProgramDao,
        IUserWalletDao userWalletDao,
        ISpringBootApiClient springBootApiClient)
    {
        _dbContext = dbContext;
        _rewardProgramDao = rewardProgramDao;
        _userWalletDao = userWalletDao;
        _springBootApiClient = springBootApiClient;
    }

    #region CreateRewardProgram

    /// <summary>
    /// Creates a new reward program with its items and policies.
    /// Also auto-creates wallets for all users in the system.
    /// Business rule: Only one reward program can be active at a time.
    /// </summary>
    public async Task<RewardProgram> CreateRewardProgramAsync(RewardProgram program)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            ValidateRewardProgram(program);

            // Deactivate all existing active programs before creating new one
            await DeactivateAllActiveProgramsAsync();

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

    /// <summary>
    /// Deactivates all currently active reward programs.
    /// Business rule: Only one reward program can be active at a time.
    /// </summary>
    private async Task DeactivateAllActiveProgramsAsync()
    {
        var activePrograms = await _dbContext.RewardPrograms
            .Where(p => p.Status == ProgramStatus.ACTIVE)
            .Include(p => p.Policies)
            .ToListAsync();

        foreach (var program in activePrograms)
        {
            program.Status = ProgramStatus.INACTIVE;
            foreach (var policy in program.Policies)
            {
                policy.IsActive = false;
            }
        }

        // Changes will be saved as part of the transaction in CreateRewardProgramAsync
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
    /// Fetches users from internal API and creates a wallet for each.
    /// </summary>
    private async Task CreateWalletsForAllUsersAsync(RewardProgram program)
    {
        // Fetch all managers and employees from internal API
        // We only care about these roles for the reward program
        var roles = new List<string> { "MANAGER", "EMPLOYEE" }; // Including all active roles
        var users = await _springBootApiClient.GetAllUsersAsync(roles);

        if (users == null || !users.Any())
        {
            // Log warning?
            return;
        }

        var wallets = users.Select(user => new UserWallet
        {
            UserWalletId = Guid.NewGuid().ToString(), // Auto-generate ID or let DAO handle it
            UserId = user.UserId,
            UserName = user.FullName,
            ProgramId = program.RewardProgramId,
            PersonalPoint = 0,
            // Only MANAGERS get a giving budget
            GivingBudget = user.Role == "MANAGER" ? program.DefaultGivingBudget : 0
        }).ToList();

        await _userWalletDao.CreateBatchAsync(wallets);
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

            // Handle items update - smart update (Add/Update/Remove)
            UpdateRewardItems(existingProgram, program.RewardItems);

            // Handle policies update - smart update (Add/Update/Remove)
            UpdatePolicies(existingProgram, program.Policies);

            // Update the program properties
            _dbContext.Entry(existingProgram).CurrentValues.SetValues(program);

            // Prevent navigation properties from being reset by SetValues if it touches them (it verifies scalar only usually but good to be safe)
            // The helper methods above already modified the collections in the tracked entity 'existingProgram'

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
    #region Smart Update Helpers

    private void UpdateRewardItems(RewardProgram existingProgram, ICollection<RewardItem> incomingItems)
    {
        var existingItems = existingProgram.RewardItems.ToList();

        // 1. Identify items to DELETE (in DB but not in Request)
        // Check by ID. Incoming items always have an ID (generated by DTO if new).
        // However, we don't know if the generated ID is "real" or "new" unless we check existence.
        // Actually, for "Update" DTO, if the user didn't provide an ID, the DTO generated a NEW one. 
        // So we match by ID. If no match -> New Item.
        // If an existing item ID is NOT in the incoming list -> Delete it.

        var incomingIds = incomingItems.Select(i => i.RewardItemId).ToHashSet();
        var itemsToDelete = existingItems.Where(i => !incomingIds.Contains(i.RewardItemId)).ToList();

        foreach (var item in itemsToDelete)
        {
            // We can check for usage here if we want to be nice, but EF will throw if FK violation occurs.
            // Let's rely on EF for hard constraints, but we could wrap it later.
            _dbContext.Set<RewardItem>().Remove(item);
        }

        // 2. Identify items to UPDATE and ADD
        foreach (var incomingItem in incomingItems)
        {
            var existingItem = existingItems.FirstOrDefault(e => e.RewardItemId == incomingItem.RewardItemId);

            if (existingItem != null)
            {
                // UPDATE existing
                existingItem.Name = incomingItem.Name;
                existingItem.RequiredPoints = incomingItem.RequiredPoints;
                existingItem.Quantity = incomingItem.Quantity;
                existingItem.ImageUrl = incomingItem.ImageUrl;
                // ProgramId should already be correct, but ensure it
                existingItem.ProgramId = existingProgram.RewardProgramId;
            }
            else
            {
                // ADD new
                // Reset ProgramId just in case
                incomingItem.ProgramId = existingProgram.RewardProgramId;
                existingProgram.RewardItems.Add(incomingItem);
            }
        }
    }

    private void UpdatePolicies(RewardProgram existingProgram, ICollection<RewardProgramPolicy> incomingPolicies)
    {
        var existingPolicies = existingProgram.Policies.ToList();
        var incomingIds = incomingPolicies.Select(p => p.PolicyId).ToHashSet();

        // 1. Delete missing
        var policiesToDelete = existingPolicies.Where(p => !incomingIds.Contains(p.PolicyId)).ToList();
        foreach (var policy in policiesToDelete)
        {
            _dbContext.Set<RewardProgramPolicy>().Remove(policy);
        }

        // 2. Update/Add
        foreach (var incomingPolicy in incomingPolicies)
        {
            var existingPolicy = existingPolicies.FirstOrDefault(e => e.PolicyId == incomingPolicy.PolicyId);
            if (existingPolicy != null)
            {
                // Update
                existingPolicy.PolicyType = incomingPolicy.PolicyType;
                existingPolicy.UnitValue = incomingPolicy.UnitValue;
                existingPolicy.PointsPerUnit = incomingPolicy.PointsPerUnit;
                existingPolicy.ProgramId = existingProgram.RewardProgramId;
            }
            else
            {
                // Add
                incomingPolicy.ProgramId = existingProgram.RewardProgramId;
                // New polices on an active program should probably be active by default? 
                // Using the same logic as Create:
                incomingPolicy.IsActive = existingProgram.Status == ProgramStatus.ACTIVE;
                existingProgram.Policies.Add(incomingPolicy);
            }
        }
    }

    #endregion
}
