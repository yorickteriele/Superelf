using Microsoft.EntityFrameworkCore;
using Superelf.Infrastructure.Data;
using Superelf.Domain.Entities;

namespace Superelf.Infrastructure.Data
{
    public static class EredivisieSeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if we already have Eredivisie data
            var eredivisieLeague = await context.Leagues
                .FirstOrDefaultAsync(l => l.Name == "Eredivisie");

            if (eredivisieLeague == null)
            {
                // Create Eredivisie league
                eredivisieLeague = new League
                {
                    Id = Guid.NewGuid(),
                    Name = "Eredivisie",
                    ShortName = "ERE",
                    Country = "Netherlands",
                    LogoUrl = "https://logos-world.net/wp-content/uploads/2020/06/Eredivisie-Logo.png",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Leagues.AddAsync(eredivisieLeague);
                await context.SaveChangesAsync();
            }

            // Check if we already have Eredivisie clubs
            var existingClubsCount = await context.Clubs
                .CountAsync(c => c.LeagueId == eredivisieLeague.Id);

            if (existingClubsCount == 0)
            {
                var eredivisieClubs = new List<Club>
                {
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Ajax",
                        ShortName = "AJX",
                        LogoUrl = "https://logos-world.net/wp-content/uploads/2020/06/Ajax-Logo.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "PSV",
                        ShortName = "PSV",
                        LogoUrl = "https://logos-world.net/wp-content/uploads/2020/06/PSV-Eindhoven-Logo.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Feyenoord",
                        ShortName = "FEY",
                        LogoUrl = "https://logos-world.net/wp-content/uploads/2020/06/Feyenoord-Logo.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "AZ Alkmaar",
                        ShortName = "AZ",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/3/32/AZ_Alkmaar_logo.svg/1200px-AZ_Alkmaar_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "FC Twente",
                        ShortName = "TWE",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/e/e3/FC_Twente.svg/1200px-FC_Twente.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "FC Utrecht",
                        ShortName = "UTR",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/c/c8/FC_Utrecht.svg/1200px-FC_Utrecht.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Vitesse",
                        ShortName = "VIT",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/f/f3/SBV_Vitesse_logo.svg/1200px-SBV_Vitesse_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "sc Heerenveen",
                        ShortName = "HEE",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/e/ed/SC_Heerenveen_logo.svg/1200px-SC_Heerenveen_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Willem II",
                        ShortName = "WIL",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/f/f4/Willem_II_Tilburg_logo.svg/1200px-Willem_II_Tilburg_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Go Ahead Eagles",
                        ShortName = "GAE",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/f/f3/Go_Ahead_Eagles_logo.svg/1200px-Go_Ahead_Eagles_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Heracles Almelo",
                        ShortName = "HER",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/4/43/Heracles_Almelo_logo.svg/1200px-Heracles_Almelo_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Sparta Rotterdam",
                        ShortName = "SPA",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/4/47/Sparta_Rotterdam_logo.svg/1200px-Sparta_Rotterdam_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "PEC Zwolle",
                        ShortName = "PEC",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/c/c1/PEC_Zwolle_logo.svg/1200px-PEC_Zwolle_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "RKC Waalwijk",
                        ShortName = "RKC",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/7/7f/RKC_Waalwijk_logo.svg/1200px-RKC_Waalwijk_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Fortuna Sittard",
                        ShortName = "FOR",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/8/82/Fortuna_Sittard_logo.svg/1200px-Fortuna_Sittard_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "FC Groningen",
                        ShortName = "GRO",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/3/32/FC_Groningen_logo.svg/1200px-FC_Groningen_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "N.E.C.",
                        ShortName = "NEC",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/4/4b/NEC_Nijmegen_logo.svg/1200px-NEC_Nijmegen_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = "Almere City FC",
                        ShortName = "ALM",
                        LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/4/42/Almere_City_FC_logo.svg/1200px-Almere_City_FC_logo.svg.png",
                        Country = "Netherlands",
                        LeagueId = eredivisieLeague.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Clubs.AddRangeAsync(eredivisieClubs);
                await context.SaveChangesAsync();
            }
        }
    }
}
