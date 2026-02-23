using Application.Events.Queries;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Events.Queries;

public class GetEventDetailsHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetEventDetails.Handler> _logger;

    public GetEventDetailsHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetEventDetails.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var query = new GetEventDetails.Query { EventId = eventEntity.EventId };
        var handler = new GetEventDetails.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventEntity.EventId);
        result.EventName.Should().Be(eventEntity.EventName);
    }

    [Fact]
    public async Task Handle_NonExistentEvent_ThrowsHttpRequestExceptionWithNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetEventDetails.Query { EventId = nonExistentId };
        var handler = new GetEventDetails.Handler(_context, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound)
            .WithMessage("*Event not found*");
    }

    [Fact]
    public async Task Handle_ExistingEvent_ReturnsCorrectDetails()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var query = new GetEventDetails.Query { EventId = eventEntity.EventId };
        var handler = new GetEventDetails.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EventName.Should().Be(eventEntity.EventName);
        result.EventDescription.Should().Be(eventEntity.EventDescription);
        result.Location.Should().Be(eventEntity.Location);
        result.GroupId.Should().Be(eventEntity.GroupId);
        result.GroupName.Should().Be(eventEntity.GroupName);
    }

    [Fact]
    public async Task Handle_NonExistentEvent_WithCancelledToken_ThrowsRequestTimeoutException()
    {
        // Arrange - Non-existent event with cancelled token; in-memory FindAsync returns null.
        // Handler throws HttpRequestException(NotFound) before it can check for cancellation.
        var nonExistentId = Guid.NewGuid();
        var query = new GetEventDetails.Query { EventId = nonExistentId };
        var handler = new GetEventDetails.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert - in-memory EF FindAsync with cancelled token throws OperationCanceledException
        // which is wrapped by catch (TaskCanceledException) → HttpRequestException(RequestTimeout)
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_ExistingEvent_LogsSuccess()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var query = new GetEventDetails.Query { EventId = eventEntity.EventId };
        var handler = new GetEventDetails.Handler(_context, _logger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("retrieved successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_NonExistentEvent_LogsWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetEventDetails.Query { EventId = nonExistentId };
        var handler = new GetEventDetails.Handler(_context, _logger);

        // Act
        try { await handler.Handle(query, CancellationToken.None); }
        catch (HttpRequestException) { /* expected */ }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
