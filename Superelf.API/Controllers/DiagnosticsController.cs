using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using Superelf.Infrastructure.Data;

namespace Superelf.API.Controllers
{
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ILogger<DiagnosticsController> _logger;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(ILogger<DiagnosticsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("ping")]
        [EnableCors("AllowFrontend")]
        public IActionResult Ping()
        {
            try
            {
                _logger.LogInformation("Diagnostics ping received from {Origin}", 
                    Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : "unknown");
            
                // Log all request headers for debugging
                _logger.LogInformation("---- Request Headers ----");
                foreach (var header in Request.Headers)
                {
                    _logger.LogInformation("{Key}: {Value}", header.Key, header.Value);
                }
            
                // Add CORS headers manually as a backup
                Response.Headers["Access-Control-Allow-Origin"] = Request.Headers.ContainsKey("Origin") 
                    ? Request.Headers["Origin"].ToString() 
                    : "*";
                Response.Headers["Access-Control-Allow-Credentials"] = "true";
                Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Accept, Authorization";
                Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";

                // Get allowed origins from config
                var allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>();
                var originsString = allowedOrigins != null ? string.Join(", ", allowedOrigins) : "none configured";
                
                return Ok(new { 
                    message = "API is alive!", 
                    timestamp = DateTime.UtcNow,
                    origin = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : "unknown",
                    host = Request.Host.ToString(),
                    remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    allowedOrigins = originsString,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ping endpoint");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        
        [HttpGet("cors-test")]
        public IActionResult CorsTest()
        {
            // Return different response based on whether this is a CORS or same-origin request
            var origin = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : null;
            var host = $"{Request.Scheme}://{Request.Host}";
            var isCorsRequest = !string.IsNullOrEmpty(origin) && origin != host;
            
            return Ok(new {
                isCorsRequest,
                origin,
                host,
                headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        }
        
        [HttpGet("db-health")]
        public async Task<IActionResult> CheckDatabaseHealth([FromServices] ApplicationDbContext dbContext)
        {
            try
            {
                // Check if the database connection is working
                bool canConnect = await dbContext.Database.CanConnectAsync();
                
                // Get database provider
                string provider = dbContext.Database.ProviderName ?? "Unknown";
                
                // Get connection string (masked for security)
                var connectionString = dbContext.Database.GetConnectionString() ?? "Not available";
                var maskedConnectionString = MaskConnectionString(connectionString);
                
                // Get migrations information
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
                
                // Check if __EFMigrationsHistory table exists
                bool migrationTableExists = false;
                int tableCount = 0;
                List<string> tableNames = new List<string>();
                
                if (canConnect)
                {
                    try 
                    {
                        // Check if __EFMigrationsHistory table exists
                        migrationTableExists = await dbContext.Database
                            .SqlQueryRaw<bool>("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory')")
                            .FirstOrDefaultAsync();
                            
                        // Get table count
                        tableCount = await dbContext.Database
                            .SqlQueryRaw<int>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'")
                            .FirstOrDefaultAsync();
                            
                        // Get table names (limit to 20)
                        tableNames = await dbContext.Database
                            .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' LIMIT 20")
                            .ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting additional database information");
                    }
                }
                
                // Get environment information
                var jwtKeyConfigured = !string.IsNullOrEmpty(_configuration["JwtSettings:Key"]);
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";
                
                return Ok(new
                {
                    status = canConnect ? "Connected" : "Connection Failed",
                    canConnect,
                    provider,
                    connectionString = maskedConnectionString,
                    environment,
                    pendingMigrationsCount = pendingMigrations.Count(),
                    pendingMigrations = pendingMigrations.Take(20).ToList(), // Limit for brevity
                    appliedMigrationsCount = appliedMigrations.Count(),
                    appliedMigrations = appliedMigrations.Take(20).ToList(), // Limit for brevity
                    migrationTableExists,
                    tableCount,
                    tableNames,
                    jwtKeyConfigured,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database health");
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }
        
        [HttpGet("reset-migrations")]
        [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
        public async Task<IActionResult> ResetMigrations([FromServices] ApplicationDbContext dbContext)
        {
            // This endpoint should only be accessible in Development or Test
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment != "Development" && environment != "Test")
            {
                return StatusCode(403, new { error = "This endpoint is only available in Development or Test environment" });
            }
            
            try
            {
                // Check connection
                if (!await dbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { error = "Cannot connect to database" });
                }

                // Drop the migrations history table to reset migrations
                await dbContext.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
                
                return Ok(new
                {
                    message = "Migrations history table dropped successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting migrations");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        private string MaskConnectionString(string connectionString)
        {
            // Mask sensitive parts of the connection string
            if (string.IsNullOrEmpty(connectionString)) 
                return connectionString;
                
            // Create a safer version by masking passwords
            return connectionString
                .Replace(";Password=", ";Password=***")
                .Replace("password=", "password=***")
                .Replace("pwd=", "pwd=***");
        }
    }
}
