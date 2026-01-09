using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentMigrator.Runner;
using HrManagement.Api.Data;
using HrManagement.Api.Data.Migrations;
using Moq;

// Reward module
using HrManagement.Api.Modules.Reward.Domain.Dao;
using HrManagement.Api.Modules.Reward.Domain.Services.RewardProgramServices;
using HrManagement.Api.Modules.Reward.Domain.Services.PointTransactionServices;
using HrManagement.Api.Modules.Reward.Domain.Services.UserWalletServices;
using HrManagement.Api.Modules.Reward.Infrastructure.Dao;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;

// Activity module
using HrManagement.Api.Modules.Activity.Domain.Dao;
using HrManagement.Api.Modules.Activity.Domain.Services.Activity;
using HrManagement.Api.Modules.Activity.Domain.Services.Participant;
using HrManagement.Api.Modules.Activity.Domain.Services.ActivityLog;
using HrManagement.Api.Modules.Activity.Domain.Services.Template;
using HrManagement.Api.Modules.Activity.Infrastructure.Dao;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestCollectionOrderer(
    "HrManagement.Api.Tests.Integration.CollectionPriorityOrderer",
    "hr-management-dotnet")]

namespace HrManagement.Api.Tests.Integration;

// =============================================================================
// COLLECTION DEFINITIONS (ordered alphabetically by prefix)
// =============================================================================

// Reward Module Collections
[CollectionDefinition("1_RewardQueryTests")]
public class RewardQueryTestCollection : ICollectionFixture<SharedTestFixture> { }

[CollectionDefinition("2_RewardCommandTests")]
public class RewardCommandTestCollection : ICollectionFixture<SharedTestFixture> { }

// Activity Module Collections
[CollectionDefinition("3_ActivityQueryTests")]
public class ActivityQueryTestCollection : ICollectionFixture<SharedTestFixture> { }

[CollectionDefinition("4_ActivityCommandTests")]
public class ActivityCommandTestCollection : ICollectionFixture<SharedTestFixture> { }

// =============================================================================
// SHARED TEST FIXTURE
// =============================================================================

/// <summary>
/// Shared fixture for ALL integration tests.
/// Uses SQLite file-based database with FluentMigrator.
/// Registers all modules' DAOs and Services.
/// 
/// Lifecycle:
/// - Created ONCE before all tests
/// - Shared by all test classes via collections
/// - Disposed ONCE after all tests complete
/// </summary>
public class SharedTestFixture : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Mock for ISpringBootApiClient - exposed so tests can configure behavior
    /// </summary>
    public Mock<ISpringBootApiClient> MockSpringBootApiClient { get; }

    public SharedTestFixture()
    {
        // Set environment to Testing so migrations can skip seeding prod data
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Create unique temp file for this test run
        _dbPath = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.db");
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

        // =============================================================================
        // MOCK EXTERNAL SERVICES
        // =============================================================================
        MockSpringBootApiClient = new Mock<ISpringBootApiClient>();

        // Default: return empty list (tests can override this via SetupMockUsers)
        MockSpringBootApiClient
            .Setup(x => x.GetAllUsersAsync(It.IsAny<List<string>?>()))
            .ReturnsAsync(new List<UserBasicDto>());

        services.AddSingleton<ISpringBootApiClient>(MockSpringBootApiClient.Object);

        // =============================================================================
        // REWARD MODULE REGISTRATION
        // =============================================================================
        services.AddScoped<IRewardProgramDao, RewardProgramDao>();
        services.AddScoped<IPointTransactionDao, PointTransactionDao>();
        services.AddScoped<IUserWalletDao, UserWalletDao>();
        services.AddScoped<IRewardItemDao, RewardItemDao>();

        services.AddScoped<IRewardProgramQueryService, RewardProgramQueryServiceImpl>();
        services.AddScoped<IRewardProgramCommandService, RewardProgramCommandServiceImpl>();
        services.AddScoped<IPointTransactionQueryService, PointTransactionQueryServiceImpl>();
        services.AddScoped<IPointTransactionCommandService, PointTransactionCommandServiceImpl>();
        services.AddScoped<IUserWalletQueryService, UserWalletQueryServiceImpl>();

        // =============================================================================
        // ACTIVITY MODULE REGISTRATION
        // =============================================================================
        // Template Registry (Singleton - holds all config providers)
        services.AddSingleton<IActivityConfigProvider, RunningSimpleConfigProvider>();
        services.AddSingleton<ActivityTemplateRegistry>();

        // DAOs
        services.AddScoped<IActivityDao, ActivityDao>();
        services.AddScoped<IParticipantDao, ParticipantDao>();
        services.AddScoped<IActivityLogDao, ActivityLogDao>();

        // Services
        services.AddScoped<IActivityQueryService, ActivityQueryServiceImpl>();
        services.AddScoped<IActivityCommandService, ActivityCommandServiceImpl>();
        services.AddScoped<IParticipantCommandService, ParticipantCommandServiceImpl>();
        services.AddScoped<IActivityLogQueryService, ActivityLogQueryServiceImpl>();
        services.AddScoped<IActivityLogCommandService, ActivityLogCommandServiceImpl>();

        ServiceProvider = services.BuildServiceProvider();

        // Run ALL FluentMigrator migrations
        RunMigrations();
    }

    private void RunMigrations()
    {
        using var scope = ServiceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <summary>
    /// Gets a scoped service from the DI container.
    /// Creates a new scope for each call to avoid DbContext tracking issues.
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

    /// <summary>
    /// Helper method to configure mock users for ISpringBootApiClient.
    /// Call this at the beginning of tests that need specific user data.
    /// </summary>
    public void SetupMockUsers(List<UserBasicDto> users)
    {
        MockSpringBootApiClient
            .Setup(x => x.GetAllUsersAsync(It.IsAny<List<string>?>()))
            .ReturnsAsync(users);
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

// =============================================================================
// TEST ORDERERS
// =============================================================================

/// <summary>
/// Custom attribute to specify test priority within a class.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute : Attribute
{
    public int Priority { get; }
    public TestPriorityAttribute(int priority) => Priority = priority;
}

/// <summary>
/// Orders tests by priority attribute.
/// </summary>
public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

        foreach (var testCase in testCases)
        {
            var priority = 100;
            var priorityAttribute = testCase.TestMethod.Method
                .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                .FirstOrDefault();

            if (priorityAttribute != null)
            {
                priority = priorityAttribute.GetNamedArgument<int>("Priority");
            }

            if (!sortedMethods.ContainsKey(priority))
            {
                sortedMethods[priority] = new List<TTestCase>();
            }
            sortedMethods[priority].Add(testCase);
        }

        foreach (var group in sortedMethods.Keys)
        {
            foreach (var testCase in sortedMethods[group])
            {
                yield return testCase;
            }
        }
    }
}

/// <summary>
/// Orders test collections alphabetically (1_Query before 2_Command, etc.)
/// </summary>
public class CollectionPriorityOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        return testCollections.OrderBy(c => c.DisplayName);
    }
}
