using Superelf.Application.League;
using Superelf.Domain.Entities;

namespace Superelf.Tests.League;

[TestFixture]
public class LeagueServiceTests
{
    private Mock<ILeagueRepository> _mockRepository;
    private LeagueService _leagueService;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<ILeagueRepository>();
        _leagueService = new LeagueService(_mockRepository.Object);
    }

    [Test]
    public async Task GetAllLeaguesAsync_ReturnsAllLeagues()
    {
        // Arrange
        var expectedLeagues = new List<Domain.Entities.League>
        {
            new() { Id = Guid.NewGuid(), Name = "Premier League" },
            new() { Id = Guid.NewGuid(), Name = "La Liga" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expectedLeagues);

        // Act
        var result = await _leagueService.GetAllLeaguesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Premier League"));
        Assert.That(result[1].Name, Is.EqualTo("La Liga"));
    }

    [Test]
    public async Task GetLeagueByIdAsync_ExistingLeague_ReturnsLeague()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var expectedLeague = new Domain.Entities.League 
        { 
            Id = leagueId, 
            Name = "Premier League" 
        };

        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync(expectedLeague);

        // Act
        var result = await _leagueService.GetLeagueByIdAsync(leagueId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(leagueId));
        Assert.That(result.Name, Is.EqualTo("Premier League"));
    }

    [Test]
    public async Task GetLeagueByIdAsync_NonExistingLeague_ReturnsNull()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync((Domain.Entities.League?)null);

        // Act
        var result = await _leagueService.GetLeagueByIdAsync(leagueId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateLeagueAsync_ValidData_CreatesLeague()
    {
        // Arrange
        var leagueName = "New League";
        var shortName = "NL";
        var country = "England";
        var logoUrl = "http://example.com/logo.png";
        var calendarId = "calendar123";

        _mockRepository.Setup(r => r.ExistsAsync(leagueName))
            .ReturnsAsync(false);

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.League>()))
            .ReturnsAsync((Domain.Entities.League league) => league);

        // Act
        var result = await _leagueService.CreateLeagueAsync(leagueName, shortName, country, logoUrl, calendarId);

        // Assert
        Assert.That(result.Name, Is.EqualTo(leagueName));
        Assert.That(result.ShortName, Is.EqualTo(shortName));
        Assert.That(result.Country, Is.EqualTo(country));
        Assert.That(result.LogoUrl, Is.EqualTo(logoUrl));
        Assert.That(result.GoogleCalendarId, Is.EqualTo(calendarId));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void CreateLeagueAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var leagueName = "Existing League";
        _mockRepository.Setup(r => r.ExistsAsync(leagueName))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _leagueService.CreateLeagueAsync(leagueName));
        Assert.That(ex!.Message, Is.EqualTo($"League with name '{leagueName}' already exists"));
    }

    [Test]
    public async Task UpdateLeagueAsync_ValidData_UpdatesLeague()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var existingLeague = new Domain.Entities.League
        {
            Id = leagueId,
            Name = "Old Name"
        };

        var newName = "New Name";
        var shortName = "NN";
        var country = "Spain";
        var logoUrl = "http://example.com/new-logo.png";
        var calendarId = "newcalendar123";

        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync(existingLeague);
        _mockRepository.Setup(r => r.ExistsAsync(newName))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.League>()))
            .ReturnsAsync((Domain.Entities.League league) => league);

        // Act
        var result = await _leagueService.UpdateLeagueAsync(leagueId, newName, shortName, country, logoUrl, calendarId);

        // Assert
        Assert.That(result.Name, Is.EqualTo(newName));
        Assert.That(result.ShortName, Is.EqualTo(shortName));
        Assert.That(result.Country, Is.EqualTo(country));
        Assert.That(result.LogoUrl, Is.EqualTo(logoUrl));
        Assert.That(result.GoogleCalendarId, Is.EqualTo(calendarId));
    }

    [Test]
    public void UpdateLeagueAsync_NonExistingLeague_ThrowsException()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync((Domain.Entities.League?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _leagueService.UpdateLeagueAsync(leagueId, "New Name"));
        Assert.That(ex!.Message, Is.EqualTo($"League with ID {leagueId} not found"));
    }

    [Test]
    public void UpdateLeagueAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var existingLeague = new Domain.Entities.League
        {
            Id = leagueId,
            Name = "Old Name"
        };

        var newName = "Existing League";
        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync(existingLeague);
        _mockRepository.Setup(r => r.ExistsAsync(newName))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _leagueService.UpdateLeagueAsync(leagueId, newName));
        Assert.That(ex!.Message, Is.EqualTo($"League with name '{newName}' already exists"));
    }

    [Test]
    public async Task DeleteLeagueAsync_ExistingLeague_DeletesLeague()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var existingLeague = new Domain.Entities.League
        {
            Id = leagueId,
            Name = "League to Delete"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync(existingLeague);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _leagueService.DeleteLeagueAsync(leagueId));
        _mockRepository.Verify(r => r.DeleteAsync(leagueId), Times.Once);
    }

    [Test]
    public void DeleteLeagueAsync_NonExistingLeague_ThrowsException()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(leagueId))
            .ReturnsAsync((Domain.Entities.League?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _leagueService.DeleteLeagueAsync(leagueId));
        Assert.That(ex!.Message, Is.EqualTo($"League with ID {leagueId} not found"));
    }
}