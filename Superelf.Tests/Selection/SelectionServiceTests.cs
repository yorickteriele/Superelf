using Superelf.Domain.Entities;
using Superelf.Application.Selection;

namespace Superelf.Tests.Selection;

[TestFixture]
public class SelectionServiceTests
{
    private Mock<ISelectionRepository> _mockRepository;
    private SelectionService _selectionService;
    private ApplicationUser _testUser;
    private Domain.Entities.Pool _testPool;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<ISelectionRepository>();
        _selectionService = new SelectionService(_mockRepository.Object);

        _testUser = new ApplicationUser
        {
            Id = "user1",
            UserName = "testuser",
            Email = "test@example.com"
        };

        _testPool = new Domain.Entities.Pool
        {
            Id = Guid.NewGuid(),
            Name = "Test Pool",
            Code = "TEST01",
            Owner = _testUser
        };
    }

    [Test]
    public async Task GetOrCreateLineupAsync_ExistingLineup_ReturnsLineup()
    {
        // Arrange
        var poolUser = new PoolParticipant
        {
            Id = Guid.NewGuid(),
            ApplicationUser = _testUser,
            Pool = _testPool
        };

        var existingLineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = poolUser
        };

        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync(existingLineup);

        // Act
        var result = await _selectionService.GetOrCreateLineupAsync(_testPool.Id, _testUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(existingLineup.Id));
        Assert.That(result.PoolUser, Is.EqualTo(poolUser));
        _mockRepository.Verify(r => r.AddLineupAsync(It.IsAny<Lineup>()), Times.Never);
    }

    [Test]
    public async Task GetOrCreateLineupAsync_NewLineup_CreatesAndReturnsLineup()
    {
        // Arrange
        var poolUser = new PoolParticipant
        {
            Id = Guid.NewGuid(),
            ApplicationUser = _testUser,
            Pool = _testPool
        };

        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync((Lineup?)null);
        _mockRepository.Setup(r => r.GetPoolParticipantAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync(poolUser);
        _mockRepository.Setup(r => r.AddLineupAsync(It.IsAny<Lineup>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _selectionService.GetOrCreateLineupAsync(_testPool.Id, _testUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PoolUser, Is.EqualTo(poolUser));
        _mockRepository.Verify(r => r.AddLineupAsync(It.IsAny<Lineup>()), Times.Once);
    }

    [Test]
    public void GetOrCreateLineupAsync_UserNotInPool_ThrowsException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync((Lineup?)null);
        _mockRepository.Setup(r => r.GetPoolParticipantAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync((PoolParticipant?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _selectionService.GetOrCreateLineupAsync(_testPool.Id, _testUser.Id));
        Assert.That(ex!.Message, Is.EqualTo($"User {_testUser.Id} is not a participant in pool {_testPool.Id}"));
    }

    [Test]
    public async Task SubmitSelectionAsync_ValidSelection_AddsLineupLine()
    {
        // Arrange
        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = new PoolParticipant { ApplicationUser = _testUser, Pool = _testPool }
        };
        var playerId = Guid.NewGuid();
        var selectedPlayers = new List<Guid> { playerId };
        var footballer = new FootballPlayer
        {
            Id = playerId,
            Name = "Virgil van Dijk",
            Position = "Goalkeeper",
            Nationality = "Dutch"
        };

        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync(lineup);
        _mockRepository.Setup(r => r.GetPlayersByIds(selectedPlayers))
            .Returns(new List<FootballPlayer> { footballer });

        // Act
        await _selectionService.SubmitSelectionAsync(_testPool.Id, _testUser.Id, "Goalkeeper", false, selectedPlayers);

        // Assert
        _mockRepository.Verify(r => r.AddLineupLinesAsync(It.Is<List<LineupLine>>(
            lines => lines.Count == 1 &&
                    lines[0].FootballPlayer == footballer &&
                    lines[0].SpecificPosition == 1 &&
                    !lines[0].IsReserve
        )), Times.Once);
    }

    [Test]
    public void SubmitSelectionAsync_DuplicatePlayer_ThrowsException()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var footballer = new FootballPlayer
        {
            Id = playerId,
            Name = "Virgil van Dijk",
            Position = "Goalkeeper",
            Nationality = "Dutch"
        };

        var existingLine = new LineupLine
        {
            FootballPlayer = footballer,
            IsReserve = false,
            SpecificPosition = 1
        };

        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = new PoolParticipant { ApplicationUser = _testUser, Pool = _testPool },
            LineupLines = new List<LineupLine> { existingLine }
        };

        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync(lineup);
        _mockRepository.Setup(r => r.GetPlayersByIds(It.Is<List<Guid>>(ids => ids.Contains(playerId))))
            .Returns(new List<FootballPlayer> { footballer });

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _selectionService.SubmitSelectionAsync(
                _testPool.Id, _testUser.Id, "Goalkeeper", false, new List<Guid> { playerId }));
        Assert.That(ex!.Message, Is.EqualTo("Speler staat al in de selectie."));
    }

    [Test]
    public async Task SetJokerAsync_ValidPlayer_SetsJoker()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var footballer = new FootballPlayer
        {
            Id = playerId,
            Name = "Virgil van Dijk",
            Position = "Forward",
            Nationality = "Dutch"
        };

        var lineupLine = new LineupLine
        {
            FootballPlayer = footballer,
            IsReserve = false,
            SpecificPosition = 9
        };

        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = new PoolParticipant { ApplicationUser = _testUser, Pool = _testPool },
            LineupLines = new List<LineupLine> { lineupLine }
        };

        _mockRepository.Setup(r => r.GetLineupWithPlayersAsync(_testPool.Id, _testUser.Id))
            .ReturnsAsync(lineup);
        _mockRepository.Setup(r => r.UpdateLineupAsync(lineup))
            .Returns(Task.CompletedTask);

        // Act
        await _selectionService.SetJokerAsync(_testPool.Id, _testUser.Id, playerId);

        // Assert
        Assert.That(lineupLine.IsJoker, Is.True);
        _mockRepository.Verify(r => r.UpdateLineupAsync(lineup), Times.Once);
    }

    [Test]
    public async Task GetLineupStatisticsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = new PoolParticipant { ApplicationUser = _testUser, Pool = _testPool },
            Complete = true,
            LineupLines = new List<LineupLine>
            {
                new() { FootballPlayer = new FootballPlayer { Name = "Virgil van Dijk", Position = "Goalkeeper", Nationality = "Dutch" }, IsJoker = true },
                new() { FootballPlayer = new FootballPlayer { Name = "Virgil van Dijk", Position = "Defender", Nationality = "English" } },
                new() { FootballPlayer = new FootballPlayer { Name = "Virgil van Dijk", Position = "Defender", Nationality = "French" } }
            }
        };

        _mockRepository.Setup(r => r.GetLineupByIdAsync(lineup.Id))
            .ReturnsAsync(lineup);

        // Act
        var stats = await _selectionService.GetLineupStatisticsAsync(lineup.Id);

        // Assert
        Assert.That(stats.TotalPlayers, Is.EqualTo(3));
        Assert.That(stats.UniqueNationalities, Is.EqualTo(3));
        Assert.That(stats.HasJoker, Is.True);
        Assert.That(stats.FormationComplete, Is.True);
    }

    [Test]
    public async Task GetLineupStatisticsAsync_NoJoker_ReturnsCorrectStats()
    {
        // Arrange
        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            PoolUser = new PoolParticipant { ApplicationUser = _testUser, Pool = _testPool },
            Complete = false,
            LineupLines = new List<LineupLine>
            {
                new() { FootballPlayer = new FootballPlayer { Name = "Virgil van Dijk", Position = "Goalkeeper", Nationality = "Dutch" } },
                new() { FootballPlayer = new FootballPlayer { Name = "Virgil van Dijk", Position = "Defender", Nationality = "Dutch" } }
            }
        };

        _mockRepository.Setup(r => r.GetLineupByIdAsync(lineup.Id))
            .ReturnsAsync(lineup);

        // Act
        var stats = await _selectionService.GetLineupStatisticsAsync(lineup.Id);

        // Assert
        Assert.That(stats.TotalPlayers, Is.EqualTo(2));
        Assert.That(stats.UniqueNationalities, Is.EqualTo(1)); // Both players are Dutch
        Assert.That(stats.HasJoker, Is.False);
        Assert.That(stats.JokerPlayerId, Is.EqualTo(Guid.Empty));
        Assert.That(stats.FormationComplete, Is.False);
    }
}