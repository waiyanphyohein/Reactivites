using Application.Events.Commands;
using Tests.Application.TestHelpers;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Events.Commands;

public class EditEventHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<EditEvent.Handler> _logger;

    public EditEventHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _mapper = MapperFactory.CreateMapper();
        _logger = MockLoggerFactory.CreateLogger<EditEvent.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingEvent_UpdatesEvent()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        var updatedEvent = new Event
        {
            EventId = existingEvent.EventId,
            EventName = "Updated Event Name",
            EventDescription = "Updated Description",
            Location = "Updated Location",
            GroupId = existingEvent.GroupId,
            GroupName = existingEvent.GroupName,
            Organizers = existingEvent.Organizers
        };

        var command = new EditEvent.Command(updatedEvent);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventName.Should().Be("Updated Event Name");
        result.EventDescription.Should().Be("Updated Description");
        result.Location.Should().Be("Updated Location");
    }

    [Fact]
    public async Task Handle_NonExistentEvent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var eventEntity = EventTestData.CreateValidEvent(nonExistentId);
        var command = new EditEvent.Command(eventEntity);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Event not found*");
    }

    [Fact]
    public async Task Handle_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        existingEvent.EventName = "Original Name";
        existingEvent.EventDescription = "Original Description";
        existingEvent.Location = "Original Location";
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        // Create partial update with some null/empty fields (should not overwrite)
        // Pass existing organizers to avoid EF tracking conflicts with new Person instances
        var partialUpdate = EventTestData.CreatePartialUpdateEvent(existingEvent.EventId, existingEvent.Organizers);

        var command = new EditEvent.Command(partialUpdate);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.EventName.Should().Be("Updated Event Name"); // Should update
        result.GroupName.Should().Be("Updated Group Name"); // Should update
        result.EventDescription.Should().Be("Original Description"); // Should NOT update (null)
        result.Location.Should().Be("Original Location"); // Should NOT update (empty string)
    }

    [Fact]
    public async Task Handle_ValidUpdate_SavesChangesToDatabase()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        var updatedEvent = new Event
        {
            EventId = existingEvent.EventId,
            EventName = "Saved Name",
            EventDescription = existingEvent.EventDescription,
            Location = existingEvent.Location,
            GroupId = existingEvent.GroupId,
            GroupName = existingEvent.GroupName,
            Organizers = existingEvent.Organizers
        };

        var command = new EditEvent.Command(updatedEvent);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedEvent = await _context.Events.FindAsync(existingEvent.EventId);
        savedEvent.Should().NotBeNull();
        savedEvent!.EventName.Should().Be("Saved Name");
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedEvent()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        // Reuse same Organizers to avoid EF tracking conflicts with new Person instances
        var updatedEvent = new Event
        {
            EventId = existingEvent.EventId,
            EventName = "Returned Name",
            EventDescription = existingEvent.EventDescription,
            Location = existingEvent.Location,
            GroupId = existingEvent.GroupId,
            GroupName = existingEvent.GroupName,
            Organizers = existingEvent.Organizers
        };

        var command = new EditEvent.Command(updatedEvent);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(existingEvent.EventId);
        result.EventName.Should().Be("Returned Name");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsException()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        // Reuse same Organizers to avoid EF tracking conflicts
        var updatedEvent = new Event
        {
            EventId = existingEvent.EventId,
            EventName = "Cancelled Update",
            EventDescription = existingEvent.EventDescription,
            Location = existingEvent.Location,
            GroupId = existingEvent.GroupId,
            GroupName = existingEvent.GroupName,
            Organizers = existingEvent.Organizers
        };
        var command = new EditEvent.Command(updatedEvent);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert - SaveChangesAsync with cancelled token throws TaskCanceledException,
        // which is caught and rethrown as HttpRequestException(RequestTimeout)
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout);
    }

    [Fact]
    public async Task Handle_ValidUpdate_LogsSuccess()
    {
        // Arrange
        var existingEvent = EventTestData.CreateValidEvent();
        _context.Events.Add(existingEvent);
        await _context.SaveChangesAsync();

        // Reuse same Organizers to avoid EF tracking conflicts
        var updatedEvent = new Event
        {
            EventId = existingEvent.EventId,
            EventName = "Logged Update",
            EventDescription = existingEvent.EventDescription,
            Location = existingEvent.Location,
            GroupId = existingEvent.GroupId,
            GroupName = existingEvent.GroupName,
            Organizers = existingEvent.Organizers
        };

        var command = new EditEvent.Command(updatedEvent);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(existingEvent.EventId.ToString())),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_NonExistentEvent_LogsWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var eventEntity = EventTestData.CreateValidEvent(nonExistentId);
        var command = new EditEvent.Command(eventEntity);
        var handler = new EditEvent.Handler(_context, _mapper, _logger);

        // Act
        try { await handler.Handle(command, CancellationToken.None); }
        catch (KeyNotFoundException) { /* expected */ }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("not found for update")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
