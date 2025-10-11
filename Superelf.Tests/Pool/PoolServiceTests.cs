using Moq;
using NUnit.Framework;
using Superelf.Application.Pool;
using Superelf.Application.Selection;
using Superelf.Domain.Entities;

namespace Superelf.Tests;

public class PoolServiceTests
{
    private Mock<IPoolRepository> _mockPoolRepository;
    private Mock<ISelectionService> _mockSelectionService;
    private PoolService _poolService;
    private ApplicationUser _testUser;

    [SetUp]
    public void Setup()
    {
        _mockPoolRepository = new Mock<IPoolRepository>();
        _mockSelectionService = new Mock<ISelectionService>();
        _poolService = new PoolService(_mockPoolRepository.Object, _mockSelectionService.Object);
        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser"
        };
    }

    [Test]
    public async Task CreatePoolAsync_Success()
    {
        // Arrange
        var pool = new Pool
        {
            Name = "Test Pool",
            Code = "INITIAL", // Will be overwritten by the service
            Owner = _testUser // Will be overwritten by the service
        };
        _mockPoolRepository.Setup(repo => repo.CreatePoolAsync(It.IsAny<Pool>())).ReturnsAsync(true);
        _mockPoolRepository.Setup(repo => repo.AddParticipantToPoolAsync(It.IsAny<Pool>(), It.IsAny<ApplicationUser>())).ReturnsAsync(true);
        _mockPoolRepository.Setup(repo => repo.GetPoolByCodeAsync(It.IsAny<string>())).ReturnsAsync(null as Pool);

        // Act
        var result = await _poolService.CreatePoolAsync(pool, _testUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Code, Has.Length.EqualTo(6));
        Assert.That(result.Owner, Is.EqualTo(_testUser));
        _mockPoolRepository.Verify(repo => repo.CreatePoolAsync(It.Is<Pool>(p => p.Name == "Test Pool")), Times.Once);
        _mockPoolRepository.Verify(repo => repo.AddParticipantToPoolAsync(It.IsAny<Pool>(), _testUser), Times.Once);
    }

    [Test]
    public async Task JoinPoolAsync_Success()
    {
        // Arrange
        var pool = new Pool
        {
            Code = "ABC123",
            Name = "Test Pool",
            Owner = _testUser
        };
        _mockPoolRepository.Setup(repo => repo.GetPoolByCodeAsync("ABC123")).ReturnsAsync(pool);
        _mockPoolRepository.Setup(repo => repo.IsUserInPoolAsync(pool, _testUser)).ReturnsAsync(false);
        _mockPoolRepository.Setup(repo => repo.AddParticipantToPoolAsync(pool, _testUser)).ReturnsAsync(true);

        // Act
        var result = await _poolService.JoinPoolAsync("ABC123", _testUser);

        // Assert
        Assert.That(result, Is.True);
        _mockPoolRepository.Verify(repo => repo.AddParticipantToPoolAsync(pool, _testUser), Times.Once);
    }

    [Test]
    public async Task JoinPoolAsync_AlreadyMember_ReturnsFalse()
    {
        // Arrange
        var pool = new Pool
        {
            Code = "ABC123",
            Name = "Test Pool",
            Owner = _testUser
        };
        _mockPoolRepository.Setup(repo => repo.GetPoolByCodeAsync("ABC123")).ReturnsAsync(pool);
        _mockPoolRepository.Setup(repo => repo.IsUserInPoolAsync(pool, _testUser)).ReturnsAsync(true);

        // Act
        var result = await _poolService.JoinPoolAsync("ABC123", _testUser);

        // Assert
        Assert.That(result, Is.False);
        _mockPoolRepository.Verify(repo => repo.AddParticipantToPoolAsync(It.IsAny<Pool>(), It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task GetPoolWithParticipantsAsync_UserInPool_ReturnsPoolAndParticipants()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool
        {
            Id = poolId,
            Name = "Test Pool",
            Owner = _testUser,
            Code = "TEST01"
        };
        var participants = new List<PoolParticipant>
        {
            new() { ApplicationUser = _testUser, Pool = pool }
        };

        _mockPoolRepository.Setup(repo => repo.GetPoolByIdAsync(poolId)).ReturnsAsync(pool);
        _mockPoolRepository.Setup(repo => repo.IsUserInPoolAsync(pool, _testUser)).ReturnsAsync(true);
        _mockPoolRepository.Setup(repo => repo.GetPoolParticipantsAsync(poolId)).ReturnsAsync(participants);

        // Act
        var result = await _poolService.GetPoolWithParticipantsAsync(poolId, _testUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.Pool, Is.EqualTo(pool));
        Assert.That(result.Value.Participants, Is.EqualTo(participants));
    }

    [Test]
    public async Task GetPoolParticipantsWithSelectionStatusAsync_Success()
    {
        // Arrange
        var testPool = new Pool
        {
            Id = Guid.NewGuid(),
            Name = "Test Pool",
            Owner = _testUser,
            Code = "TEST02"
        };
        var participant = new PoolParticipant { ApplicationUser = _testUser, Pool = testPool };
        var participants = new List<PoolParticipant> { participant };
        var lineup = new Lineup
        {
            Id = Guid.NewGuid(),
            Complete = true,
            PoolUser = participant
        };
        var lineupStats = new LineupStatistics
        {
            TotalPlayers = 11,
            HasJoker = true
        };

        _mockPoolRepository.Setup(repo => repo.GetPoolParticipantsAsync(testPool.Id)).ReturnsAsync(participants);
        _mockSelectionService.Setup(service => service.GetOrCreateLineupAsync(testPool.Id, _testUser.Id)).ReturnsAsync(lineup);
        _mockSelectionService.Setup(service => service.GetLineupStatisticsAsync(lineup.Id)).ReturnsAsync(lineupStats);

        // Act
        var result = await _poolService.GetPoolParticipantsWithSelectionStatusAsync(testPool.Id);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var participantStatus = result[0];
        Assert.That(participantStatus.Participant, Is.EqualTo(participants[0]));
        Assert.That(participantStatus.SelectionComplete, Is.True);
        Assert.That(participantStatus.SelectedPlayers, Is.EqualTo(11));
        Assert.That(participantStatus.HasJoker, Is.True);
    }

    [Test]
    public async Task EditPoolNameAsync_OwnerRequest_Success()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool
        {
            Id = poolId,
            Name = "Old Name",
            Owner = _testUser,
            Code = "TEST03"
        };
        var newName = "New Name";

        _mockPoolRepository.Setup(repo => repo.GetPoolByIdAsync(poolId)).ReturnsAsync(pool);
        _mockPoolRepository.Setup(repo => repo.EditPoolNameAsync(It.IsAny<Pool>())).ReturnsAsync(true);

        // Act
        var result = await _poolService.EditPoolNameAsync(poolId, newName, _testUser);

        // Assert
        Assert.That(result, Is.True);
        _mockPoolRepository.Verify(repo => repo.EditPoolNameAsync(It.Is<Pool>(p => p.Name == newName)), Times.Once);
    }
}