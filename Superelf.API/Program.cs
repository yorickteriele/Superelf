using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Superelf.Application.Authentication;
using Superelf.Application.Pool;
using Superelf.Application.Selection;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;
using Superelf.Infrastructure.Repositories;
using System.Text;

bool isMigrationOnly = args.Contains("--migrate-only");

var builder = WebApplication.CreateBuilder(args);

// Load correct appsettings.{ENV}.json
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

Console.WriteLine($"Using environment: {env}");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    if (!builder.Environment.IsProduction())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT setup
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
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
});

builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPoolRepository, PoolRepository>();
builder.Services.AddScoped<PoolService>();
builder.Services.AddScoped<ISelectionRepository, SelectionRepository>();
builder.Services.AddScoped<SelectionService>();

// Register CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (isMigrationOnly)
        Environment.Exit(0);
}

// Use CORS before authentication/authorization
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
