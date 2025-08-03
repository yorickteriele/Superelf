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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        string adminRoleId = Guid.NewGuid().ToString();
        string userRoleId = Guid.NewGuid().ToString();
        
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" }
        );

        string adminUserId = Guid.NewGuid().ToString();
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

        modelBuilder.Entity<FootballPlayer>().HasData(
            new FootballPlayer { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Unai Simón", Position = "Goalkeeper", Nationality = "SPANJE", Club = null },
            new FootballPlayer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Josko Gvardiol", Position = "Defender", Nationality = "KROATIE", Club = null },
            new FootballPlayer { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Virgil van Dijk", Position = "Defender", Nationality = "NEDERLAND", Club = null },
            new FootballPlayer { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Giovanni Di Lorenzo", Position = "Defender", Nationality = "ITALIE", Club = null },
            new FootballPlayer { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Andreas Christensen", Position = "Defender", Nationality = "DENEMARKEN", Club = null },
            new FootballPlayer { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Xherdan Shaqiri", Position = "Midfielder", Nationality = "ZWITSERLAND", Club = null },
            new FootballPlayer { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Florian Wirtz", Position = "Midfielder", Nationality = "DUITSLAND", Club = null },
            new FootballPlayer { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Kevin De Bruyne", Position = "Midfielder", Nationality = "BELGIE", Club = null },
            new FootballPlayer { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Cristiano Ronaldo", Position = "Forward", Nationality = "PORTUGAL", Club = null },
            new FootballPlayer { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Kylian Mbappé", Position = "Forward", Nationality = "FRANKRIJK", Club = null },
            new FootballPlayer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Harry Kane", Position = "Forward", Nationality = "ENGELAND", Club = null },
            new FootballPlayer { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Jan Oblak", Position = "Goalkeeper", Nationality = "SLOVENIE", Club = null },
            new FootballPlayer { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Nemanja Stojic", Position = "Defender", Nationality = "SERVIE", Club = null },
            new FootballPlayer { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Nicola Zalewski", Position = "Midfielder", Nationality = "POLEN", Club = null },
            new FootballPlayer { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Maximilian Entrup", Position = "Forward", Nationality = "OOSTENRIJK", Club = null }
        );
    }
}
