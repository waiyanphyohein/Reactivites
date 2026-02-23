using Application.Events.Queries;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.Application.Events.Queries;

public class GetEventListHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetEventList> _logger;

    public GetEventListHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetEventList>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithEvents_ReturnsAllEvents()
    {
        // Arrange
        var events = EventTestData.CreateEventList(5);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventList.Query();
        var handler = new GetEventList.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetEventList.Query();
        var handler = new GetEventList.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEvents_ReturnsCorrectEventNames()
    {
        // Arrange
        var events = EventTestData.CreateEventList(3);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventList.Query();
        var handler = new GetEventList.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Select(e => e.EventName).Should().BeEquivalentTo(events.Select(e => e.EventName));
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var events = EventTestData.CreateEventList(100);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventList.Query();
        var handler = new GetEventList.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert - In-memory database throws OperationCanceledException directly
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var query = new GetEventList.Query();
        var handler = new GetEventList.Handler(_context, _logger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Fetching event list")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
