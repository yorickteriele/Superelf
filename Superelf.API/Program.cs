using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Superelf.Application.Authentication;
using Superelf.Application.Pool;
using Superelf.Application.Selection;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;
using Superelf.Infrastructure.Repositories;
using Superelf.API.Hubs;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Check if this is a migration-only run
bool isMigrationOnly = args.Contains("--migrate-only");

if (isMigrationOnly)
{
    Console.WriteLine("=== MIGRATION ONLY MODE ===");
    Console.WriteLine("Running in migration-only mode. Application will exit after applying migrations.");
    
    // In migration-only mode, add a short initial delay to ensure the database is ready
    Console.WriteLine("Waiting for database to be ready...");
    Thread.Sleep(5000);
}

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration for migration debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsProduction())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var corsOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        
        if (corsOrigins != null && corsOrigins.Any())
        {
            policy.WithOrigins(corsOrigins);
        }
        else
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://superelf.yorickteriele.nl",
                "http://superelf.yorickteriele.nl",
                // Support both with and without www
                "https://www.superelf.yorickteriele.nl",
                "http://www.superelf.yorickteriele.nl"
            );
        }
        
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains(); // Allow wildcard subdomains
        
        // Log the configured CORS origins for troubleshooting
        var origins = corsOrigins ?? new[] {
            "http://localhost:3000",
            "http://localhost:3001",
            "https://superelf.yorickteriele.nl",
            "http://superelf.yorickteriele.nl",
            "https://www.superelf.yorickteriele.nl",
            "http://www.superelf.yorickteriele.nl"
        };
        
        Console.WriteLine("CORS configured with these origins:");
        foreach (var origin in origins)
        {
            Console.WriteLine($"  - {origin}");
        }
    });
});

// Get connection string and log it
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("Configuring PostgreSQL database connection");

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    // Use PostgreSQL only
    option.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
    
    // Enable sensitive data logging in non-production environments
    if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
    {
        option.EnableSensitiveDataLogging();
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // For SignalR authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/poolhub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IPoolRepository, PoolRepository>();
builder.Services.AddScoped<PoolService>();

builder.Services.AddScoped<ISelectionRepository, SelectionRepository>();
builder.Services.AddScoped<SelectionService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure logging for startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up. Environment: {Environment}", app.Environment.EnvironmentName);

// Log DB connection on startup and apply pending migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Testing database connection...");

        // Test connection with more detailed error handling
        bool canConnect = false;
        int retryCount = 0;
        int maxRetries = isMigrationOnly ? 3 : 10; // Fewer retries in migration-only mode
        int retryDelay = isMigrationOnly ? 3000 : 10000; // Shorter delay in migration-only mode

        logger.LogInformation("Starting database connection attempts (max {MaxRetries} attempts)...", maxRetries);

        while (!canConnect && retryCount < maxRetries)
        {
            try
            {
                logger.LogInformation("Connection attempt {Attempt}/{MaxRetries}...", retryCount + 1, maxRetries);

                // Test the connection
                logger.LogInformation("Testing database connection...");
                canConnect = dbContext.Database.CanConnect();
                logger.LogInformation("Database connection test result: {CanConnect}", canConnect ? "SUCCESS" : "FAILED");

                if (canConnect)
                {
                    // If we can connect, ensure database exists
                    logger.LogInformation("Ensuring database exists...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database.EnsureCreated() completed successfully.");
                    logger.LogInformation("=== DATABASE CONNECTION SUCCESSFUL ===");
                }
                else if (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    logger.LogWarning("Connection failed, retrying in {Delay}ms... (attempt {Retry}/{MaxRetries})", 
                                     retryDelay, retryCount, maxRetries);
                    await Task.Delay(retryDelay);
                }
            }
            catch (Exception dbConnEx)
            {
                retryCount++;
                logger.LogError(dbConnEx, "Database connection attempt {Retry}/{MaxRetries} failed with exception: {Message}", 
                               retryCount, maxRetries, dbConnEx.Message);
                
                if (dbConnEx.InnerException != null)
                {
                    logger.LogError("Inner exception: {InnerException}", dbConnEx.InnerException.Message);
                }

                if (retryCount < maxRetries)
                {
                    logger.LogInformation("Retrying database connection in {Delay}ms...", retryDelay);
                    await Task.Delay(retryDelay);
                }
            }
        }

        if (!canConnect)
        {
            logger.LogError("Cannot connect to database after {MaxRetries} attempts. Check connection string and database availability.", maxRetries);
            logger.LogError("Connection string format: {ConnectionString}",
                connectionString?.Substring(0, Math.Min(100, connectionString?.Length ?? 0)) + "...");

            if (!app.Environment.IsProduction())
            {
                throw new InvalidOperationException("Database connection failed");
            }
        }
        else
        {
// Check for pending migrations
logger.LogInformation("Successfully connected to database. Checking for pending migrations...");

// Check if JWT settings are properly configured
if (string.IsNullOrEmpty(secretKey) && (app.Environment.IsDevelopment() || isMigrationOnly))
{
    logger.LogWarning("JWT Secret Key is not configured in the environment or appsettings.json. Using a default key for development/migration...");
    secretKey = "DefaultSecretKey1234567890DefaultSecretKey1234567890";
}

// In migration-only mode, always run migrations
// In normal mode, check if migrations should be skipped
bool skipMigrations = !isMigrationOnly && Environment.GetEnvironmentVariable("SKIP_DB_MIGRATIONS")?.ToLower() == "true";

if (skipMigrations)
{
    logger.LogInformation("Skipping database migrations as SKIP_DB_MIGRATIONS is set to true.");
}
else
{
    logger.LogInformation("=== STARTING MIGRATION PROCESS ===");
    
    // Print migrations info for debugging
    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
        
        logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count);
        foreach (var migration in appliedMigrations)
        {
            logger.LogInformation("  - {Migration}", migration);
        }
        
        logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count);
        foreach (var migration in pendingMigrations)
        {
            logger.LogInformation("  - {Migration}", migration);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning("Could not retrieve migration info: {Message}", ex.Message);
    }
    
    // Multiple attempts for migrations
    int maxAttempts = 3;
    int attempt = 0;
    bool migrationSuccess = false;
    
    while (!migrationSuccess && attempt < maxAttempts)
    {
        attempt++;
        logger.LogInformation("Migration attempt {Attempt} of {MaxAttempts}...", attempt, maxAttempts);
        
        try
        {
            if (isMigrationOnly)
            {
                // For migration-only mode, ensure the database exists first
                logger.LogInformation("Migration-only mode detected, ensuring database schema exists...");
                dbContext.Database.EnsureCreated();
                logger.LogInformation("Database schema created successfully.");
                
                // Force migrations to run even in migration-only mode
                logger.LogInformation("Applying migrations in migration-only mode...");
                dbContext.Database.Migrate();
                logger.LogInformation("Migrations applied successfully in migration-only mode.");
            }
            else
            {
                // Apply standard migrations
                logger.LogInformation("Applying database migrations...");
                dbContext.Database.Migrate();
                logger.LogInformation("=== MIGRATIONS SUCCESSFULLY APPLIED ===");
            }
            
            migrationSuccess = true;
        }
        catch (Exception migrationEx)
        {
            logger.LogError(migrationEx, "Migration attempt {Attempt} failed with error: {Message}", attempt, migrationEx.Message);
            
            if (migrationEx.InnerException != null)
            {
                logger.LogError("Inner exception: {InnerException}", migrationEx.InnerException.Message);
            }

            // If this is a table already exists error, we can continue
            bool isTableExistsError = migrationEx.Message.Contains("already an object named") || 
                                     (migrationEx.Message.Contains("relation") && migrationEx.Message.Contains("already exists"));
            
            if (isTableExistsError)
            {
                logger.LogWarning("Tables already exist. This might be expected during setup.");
                migrationSuccess = true;
                break;
            }
            
            // If we still have attempts left, wait before trying again
            if (attempt < maxAttempts)
            {
                int delay = attempt * 5;
                logger.LogInformation("Waiting {Delay} seconds before next attempt...", delay);
                Thread.Sleep(delay * 1000);
            }
            else if (isMigrationOnly || !app.Environment.IsProduction())
            {
                throw; // Re-throw to prevent app from starting with broken DB on the last attempt
            }
        }
    }                // Verify tables exist
                try
                {
                    // Get PostgreSQL table count
                    var tableCount = dbContext.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'").First();
                    logger.LogInformation("Database contains {TableCount} tables", tableCount);
                }
                catch (Exception tableEx)
                {
                    logger.LogWarning(tableEx, "Could not verify table count: {Message}", tableEx.Message);
                }
            }

            // If this is migration-only mode, exit after successful migration
            if (isMigrationOnly)
            {
                logger.LogInformation("=== MIGRATION-ONLY MODE COMPLETED SUCCESSFULLY ===");
                logger.LogInformation("Database migration process completed. Exiting application.");
                Environment.Exit(0);
            }
        }
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error occurred while testing database connection or applying migrations: {Message}", ex.Message);
    logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);

    if (isMigrationOnly)
    {
        logger.LogError("=== MIGRATION-ONLY MODE FAILED ===");
        logger.LogError("Migration process failed. See error details above.");
        Environment.Exit(1);
    }

    // Don't throw in production to allow app to start (but log the error)
    if (!app.Environment.IsProduction())
    {
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comment out for local development to allow HTTP connections
// app.UseHttpsRedirection();

// Configure and apply CORS early in the pipeline - MUST be before UseRouting
logger.LogInformation("Configuring CORS policy");
app.UseCors("AllowFrontend");
logger.LogInformation("CORS policy applied");

// Print out CORS policy for debugging
try 
{
    var corsPolicy = app.Services.GetRequiredService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsPolicyProvider>()
        .GetPolicyAsync(app.Services.GetRequiredService<Microsoft.AspNetCore.Http.HttpContext>(), "AllowFrontend")
        .GetAwaiter().GetResult();
    if (corsPolicy != null)
    {
        logger.LogInformation("CORS policy origins: {Origins}", 
            string.Join(", ", corsPolicy.Origins));
    }
}
catch (Exception ex)
{
    logger.LogWarning("Could not log CORS policy: {Message}", ex.Message);
}

// Add middleware to log CORS issues
app.Use(async (context, next) =>
{
    // Log all incoming requests
    logger.LogInformation("Request received: {Method} {Path} from Origin: {Origin}",
        context.Request.Method,
        context.Request.Path,
        context.Request.Headers.Origin);
    
    await next();
    
    // Log response status for preflight requests
    if (context.Request.Method == "OPTIONS")
    {
        logger.LogInformation("CORS preflight response status: {StatusCode}", context.Response.StatusCode);
        foreach (var header in context.Response.Headers)
        {
            logger.LogInformation("Response Header: {Key}: {Value}", header.Key, header.Value);
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PoolHub>("/api/poolhub");

app.Run();
