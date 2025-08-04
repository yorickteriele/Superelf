using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Superelf.Domain.Entities;

namespace Superelf.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<PoolParticipant> PoolParticipants { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Pool> Pools { get; set; }
    public DbSet<Lineup> Lineups { get; set; }
    public DbSet<LineupLine> LineupLines { get; set; }
    public DbSet<FootballPlayer> FootballPlayers { get; set; }
    public DbSet<Club> Clubs { get; set; }
    public DbSet<League> Leagues { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<PlayerPerformance> PlayerPerformances { get; set; }
    public DbSet<UserScore> UserScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FootballPlayer-Club relationship
        modelBuilder.Entity<FootballPlayer>()
            .HasOne(p => p.ClubEntity)
            .WithMany(c => c.Players)
            .HasForeignKey(p => p.ClubId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure PlayerPerformance-Match relationship
        modelBuilder.Entity<PlayerPerformance>()
            .HasOne(pp => pp.Match)
            .WithMany(m => m.PlayerPerformances)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure PlayerPerformance-Player relationship
        modelBuilder.Entity<PlayerPerformance>()
            .HasOne(pp => pp.Player)
            .WithMany(p => p.Performances)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure League-Club relationship
        modelBuilder.Entity<Club>()
            .HasOne(c => c.League)
            .WithMany(l => l.Clubs)
            .HasForeignKey(c => c.LeagueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure League-Match relationship
        modelBuilder.Entity<Match>()
            .HasOne(m => m.League)
            .WithMany(l => l.Matches)
            .HasForeignKey(m => m.LeagueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Use fixed GUIDs to avoid regenerating seed data on each migration
        // Commented out seed data to avoid conflicts with existing data
        
        string adminRoleId = "550e8400-e29b-41d4-a716-446655440000";
        string userRoleId = "550e8400-e29b-41d4-a716-446655440001";
        
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" }
        );
         string adminUserId = "550e8400-e29b-41d4-a716-446655440002";
         var hasher = new PasswordHasher<ApplicationUser>();

         var adminUser = new ApplicationUser
         {
             Id = adminUserId,
             UserName = "admin@superelf.com",
             NormalizedUserName = "ADMIN@SUPERELF.COM",
             Email = "admin@superelf.com",
             NormalizedEmail = "ADMIN@SUPERELF.COM",
             EmailConfirmed = true,
             PasswordHash = hasher.HashPassword(new ApplicationUser(), "Admin123!")
         };

         modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

         modelBuilder.Entity<IdentityUserRole<string>>().HasData(
             new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId }
         );

        // Add Eredivisie league data
        var eredivisieId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<League>().HasData(
            new League 
            { 
                Id = eredivisieId,
                Name = "Eredivisie", 
                ShortName = "ERE",
                Country = "Netherlands",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Add Eredivisie clubs
        var clubData = new[]
        {
            new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Ajax", ShortName = "AJX", LeagueId = eredivisieId },
            new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "PSV", ShortName = "PSV", LeagueId = eredivisieId },
            new { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Feyenoord", ShortName = "FEY", LeagueId = eredivisieId },
            new { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "AZ", ShortName = "AZ", LeagueId = eredivisieId },
            new { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "FC Utrecht", ShortName = "UTR", LeagueId = eredivisieId },
            new { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "FC Twente", ShortName = "TWE", LeagueId = eredivisieId },
            new { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Vitesse", ShortName = "VIT", LeagueId = eredivisieId },
            new { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "SC Heerenveen", ShortName = "HEE", LeagueId = eredivisieId },
            new { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "FC Groningen", ShortName = "GRO", LeagueId = eredivisieId },
            new { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Willem II", ShortName = "WIL", LeagueId = eredivisieId },
            new { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "NEC", ShortName = "NEC", LeagueId = eredivisieId },
            new { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Fortuna Sittard", ShortName = "FOR", LeagueId = eredivisieId },
            new { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Go Ahead Eagles", ShortName = "GAE", LeagueId = eredivisieId },
            new { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Heracles Almelo", ShortName = "HER", LeagueId = eredivisieId },
            new { Id = Guid.Parse("10101010-1010-1010-1010-101010101010"), Name = "PEC Zwolle", ShortName = "PEC", LeagueId = eredivisieId },
            new { Id = Guid.Parse("20202020-2020-2020-2020-202020202020"), Name = "RKC Waalwijk", ShortName = "RKC", LeagueId = eredivisieId },
            new { Id = Guid.Parse("30303030-3030-3030-3030-303030303030"), Name = "Sparta Rotterdam", ShortName = "SPA", LeagueId = eredivisieId },
            new { Id = Guid.Parse("40404040-4040-4040-4040-404040404040"), Name = "Almere City", ShortName = "ALM", LeagueId = eredivisieId }
        };

        foreach (var club in clubData)
        {
            modelBuilder.Entity<Club>().HasData(
                new Club 
                { 
                    Id = club.Id, 
                    Name = club.Name, 
                    ShortName = club.ShortName,
                    Country = "Netherlands",
                    LeagueId = club.LeagueId,
                    CreatedAt = DateTime.UtcNow 
                }
            );
        }

        // Temporarily commented out to avoid seed data conflicts during migration
        /*
        modelBuilder.Entity<FootballPlayer>().HasData(
            new FootballPlayer { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Unai Simón", Position = "Goalkeeper", Nationality = "SPANJE", Club = null, JerseyNumber = 1, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Josko Gvardiol", Position = "Defender", Nationality = "KROATIE", Club = null, JerseyNumber = 4, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Virgil van Dijk", Position = "Defender", Nationality = "NEDERLAND", Club = null, JerseyNumber = 4, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Giovanni Di Lorenzo", Position = "Defender", Nationality = "ITALIE", Club = null, JerseyNumber = 2, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Andreas Christensen", Position = "Defender", Nationality = "DENEMARKEN", Club = null, JerseyNumber = 6, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Xherdan Shaqiri", Position = "Midfielder", Nationality = "ZWITSERLAND", Club = null, JerseyNumber = 23, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Florian Wirtz", Position = "Midfielder", Nationality = "DUITSLAND", Club = null, JerseyNumber = 10, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Kevin De Bruyne", Position = "Midfielder", Nationality = "BELGIE", Club = null, JerseyNumber = 17, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Cristiano Ronaldo", Position = "Forward", Nationality = "PORTUGAL", Club = null, JerseyNumber = 7, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Kylian Mbappé", Position = "Forward", Nationality = "FRANKRIJK", Club = null, JerseyNumber = 10, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Harry Kane", Position = "Forward", Nationality = "ENGELAND", Club = null, JerseyNumber = 9, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Jan Oblak", Position = "Goalkeeper", Nationality = "SLOVENIE", Club = null, JerseyNumber = 1, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Nemanja Stojic", Position = "Defender", Nationality = "SERVIE", Club = null, JerseyNumber = 5, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Nicola Zalewski", Position = "Midfielder", Nationality = "POLEN", Club = null, JerseyNumber = 21, CreatedAt = DateTime.UtcNow },
            new FootballPlayer { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Maximilian Entrup", Position = "Forward", Nationality = "OOSTENRIJK", Club = null, JerseyNumber = 9, CreatedAt = DateTime.UtcNow }
        );
        */
    }
}
