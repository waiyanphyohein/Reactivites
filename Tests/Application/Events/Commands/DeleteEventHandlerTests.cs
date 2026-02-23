using Application.Events.Commands;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Events.Commands;

public class DeleteEventHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<DeleteEvent.Handler> _logger;

    public DeleteEventHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<DeleteEvent.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingEvent_DeletesEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { EventId = eventEntity.EventId };
        var handler = new DeleteEvent.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedEvent = await _context.Events.FindAsync(eventEntity.EventId);
        deletedEvent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteEvent.Command { EventId = nonExistentId };
        var handler = new DeleteEvent.Handler(_context, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Event not found*");
    }

    [Fact]
    public async Task Handle_CancellationDuringDelay_ThrowsRequestTimeoutException()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { EventId = eventEntity.EventId };
        var handler = new DeleteEvent.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancel during the Task.Delay(100)

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
            .WithMessage("*Request timed out*");
    }

    [Fact]
    public async Task Handle_CancellationAtStart_ThrowsInternalServerErrorException()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { EventId = eventEntity.EventId };
        var handler = new DeleteEvent.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately before Task.Delay

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert - OperationCanceledException gets wrapped by the generic Exception handler
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Handle_KeyNotFoundException_LogsWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteEvent.Command { EventId = nonExistentId };
        var handler = new DeleteEvent.Handler(_context, _logger);

        // Act
        try { await handler.Handle(command, CancellationToken.None); }
        catch (KeyNotFoundException) { /* expected */ }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("not found for deletion")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ValidDeletion_RemovesFromDatabase()
    {
        // Arrange
        var events = EventTestData.CreateEventList(5);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var eventToDelete = events[2];
        var command = new DeleteEvent.Command { EventId = eventToDelete.EventId };
        var handler = new DeleteEvent.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingEvents = await _context.Events.ToListAsync();
        remainingEvents.Should().HaveCount(4);
        remainingEvents.Should().NotContain(e => e.EventId == eventToDelete.EventId);
    }

    [Fact]
    public async Task Handle_ValidDeletion_LogsSuccess()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { EventId = eventEntity.EventId };
        var handler = new DeleteEvent.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("deleted successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
