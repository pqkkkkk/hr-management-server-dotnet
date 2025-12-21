var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// SERVICES CONFIGURATION
// =============================================================================

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HR Management API - Rewards & Activities", Version = "v1" });
});

// Determine database provider from configuration
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "PostgreSQL";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// Add Entity Framework Core with appropriate provider
if (databaseProvider == "Sqlite")
{
    builder.Services.AddDbContext<HrManagement.Api.Data.AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // Add FluentMigrator for SQLite
    builder.Services.AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSQLite()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(Program).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole());
}
else
{
    builder.Services.AddDbContext<HrManagement.Api.Data.AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Add FluentMigrator for PostgreSQL
    builder.Services.AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddPostgres()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(Program).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole());

    // Add Health Checks for PostgreSQL only
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString);
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// =============================================================================
// MIDDLEWARE PIPELINE
// =============================================================================

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    // For SQLite in-memory, we need to ensure the connection stays open
    if (databaseProvider == "Sqlite")
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HrManagement.Api.Data.AppDbContext>();
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();
    }

    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Management API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
