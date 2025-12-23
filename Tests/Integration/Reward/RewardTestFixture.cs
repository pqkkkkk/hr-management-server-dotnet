using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentMigrator.Runner;
using HrManagement.Api.Data;
using HrManagement.Api.Data.Migrations;
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;
using HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;
using HrManagement.Api.Modules.Reward.Infrastructure.Dao;
using Xunit;

[assembly: TestCollectionOrderer(
    "HrManagement.Api.Tests.Integration.Reward.CollectionPriorityOrderer",
    "hr-management-dotnet")]

namespace HrManagement.Api.Tests.Integration.Reward;

// =============================================================================
// COLLECTION DEFINITIONS (ordered by name: 1_Query runs before 2_Command)
// =============================================================================

/// <summary>
/// Query tests collection - runs FIRST (prefix "1_")
/// </summary>
[CollectionDefinition("1_RewardQueryTests")]
public class RewardQueryTestCollection : ICollectionFixture<RewardTestFixture>
{
}

/// <summary>
/// Command tests collection - runs SECOND (prefix "2_")
/// </summary>
[CollectionDefinition("2_RewardCommandTests")]
public class RewardCommandTestCollection : ICollectionFixture<RewardTestFixture>
{
}

// =============================================================================
// SHARED TEST FIXTURE
// =============================================================================

/// <summary>
/// Shared fixture for Reward module integration tests.
/// Uses SQLite file-based database with FluentMigrator.
/// 
/// Sample data comes from M202512_009_SeedSampleData.cs → Single source of truth!
/// 
/// Lifecycle:
/// - Created ONCE before all tests in collection
/// - Shared by all test classes in the collection
/// - Disposed ONCE after all tests complete
/// </summary>
public class RewardTestFixture : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    public IServiceProvider ServiceProvider { get; }

    public RewardTestFixture()
    {
        // Create unique temp file for this test run
        _dbPath = Path.Combine(Path.GetTempPath(), $"reward_test_{Guid.NewGuid()}.db");
        _connectionString = $"DataSource={_dbPath}";

        var services = new ServiceCollection();

        // Configure EF Core with SQLite file
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connectionString));

        // Configure FluentMigrator with SQLite
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(_connectionString)
                .ScanIn(typeof(M202512_002_CreateRewardProgramTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        // Register DAOs
        services.AddScoped<IRewardProgramDao, RewardProgramDao>();
        services.AddScoped<IPointTransactionDao, PointTransactionDao>();
        services.AddScoped<IUserWalletDao, UserWalletDao>();
        services.AddScoped<IRewardItemDao, RewardItemDao>();

        // Register Services
        services.AddScoped<IRewardProgramQueryService, RewardProgramQueryServiceImpl>();
        services.AddScoped<IRewardProgramCommandService, RewardProgramCommandServiceImpl>();
        services.AddScoped<IPointTransactionQueryService, PointTransactionQueryServiceImpl>();
        services.AddScoped<IPointTransactionCommandService, PointTransactionCommandServiceImpl>();
        services.AddScoped<IUserWalletQueryService, UserWalletQueryServiceImpl>();

        ServiceProvider = services.BuildServiceProvider();

        // Run ALL FluentMigrator migrations (including M202512_009_SeedSampleData)
        RunMigrations();
    }

    private void RunMigrations()
    {
        using var scope = ServiceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        
        // Run all migrations from 001 to 009 (creates tables + inserts seed data)
        runner.MigrateUp();
    }

    /// <summary>
    /// Gets a scoped service from the DI container.
    /// Note: Creates a new scope for each call to avoid DbContext tracking issues.
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        var scope = ServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a fresh DbContext for direct database access in tests.
    /// </summary>
    public AppDbContext CreateDbContext()
    {
        var scope = ServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public void Dispose()
    {
        // Clean up temp database file
        if (File.Exists(_dbPath))
        {
            // Wait a bit for connections to close
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors - temp file will be cleaned up by OS
            }
        }
    }
}
