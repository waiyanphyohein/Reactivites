using Application.Events.Commands;
using Application.Events.Queries;
using Tests.Application.TestHelpers;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Events.Commands;

public class AddEventHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<CreateEvent.Handler> _logger;

    public AddEventHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<CreateEvent.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidEvent_AddsEventToDatabase()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Event PK is GroupId (Event inherits from Group via TPH)
        var savedEvent = await _context.Events.FindAsync(result.GroupId);
        savedEvent.Should().NotBeNull();
        savedEvent!.EventName.Should().Be(eventEntity.EventName);
        savedEvent.Location.Should().Be(eventEntity.Location);
    }

    [Fact]
    public async Task Handle_ValidEventWithEmptyEventId_GeneratesNewEventId()
    {
        // Arrange
        var eventEntity = EventTestData.CreateEventWithEmptyIds();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidEventWithEmptyGroupId_GeneratesNewGroupId()
    {
        // Arrange
        var eventEntity = EventTestData.CreateEventWithEmptyIds();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.GroupId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidEventWithProvidedIds_SynchronizesGroupIdToEventId()
    {
        // Arrange
        var expectedEventId = Guid.NewGuid();
        var eventEntity = EventTestData.CreateValidEvent(expectedEventId);
        eventEntity.GroupId = Guid.NewGuid();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.EventId.Should().Be(expectedEventId);
        result.GroupId.Should().Be(expectedEventId);
    }

    [Fact]
    public async Task Handle_EventWithDefaultGeneratedIds_CanBeFetchedByEventId()
    {
        // Arrange
        var eventEntity = new Event
        {
            EventName = "Default ID Event",
            GroupName = "Default ID Group",
            Organizers = new List<Person>
            {
                new() { FirstName = "Jane", LastName = "Doe" }
            }
        };
        eventEntity.GroupId.Should().NotBe(eventEntity.EventId);

        var createCommand = new CreateEvent.Command { Event = eventEntity };
        var createHandler = new CreateEvent.Handler(_context, _logger);
        var detailsLogger = MockLoggerFactory.CreateLogger<GetEventDetails.Handler>();
        var detailsHandler = new GetEventDetails.Handler(_context, detailsLogger);

        // Act
        var created = await createHandler.Handle(createCommand, CancellationToken.None);
        var fetched = await detailsHandler.Handle(
            new GetEventDetails.Query { EventId = created.EventId },
            CancellationToken.None);

        // Assert
        fetched.EventId.Should().Be(created.EventId);
        fetched.GroupId.Should().Be(created.EventId);
    }

    [Fact]
    public async Task Handle_ValidEvent_ReturnsCreatedEvent()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EventName.Should().Be(eventEntity.EventName);
        result.EventDescription.Should().Be(eventEntity.EventDescription);
        result.Location.Should().Be(eventEntity.Location);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsRequestTimeoutException()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
            .WithMessage("*Request timed out*");
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ThrowsInternalServerErrorException()
    {
        // Arrange
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var eventEntity = EventTestData.CreateValidEvent();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(disposedContext, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError)
            .WithMessage("*An error occurred while creating the event*");
    }

    [Fact]
    public async Task Handle_ValidEvent_LogsSuccess()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        var command = new CreateEvent.Command { Event = eventEntity };
        var handler = new CreateEvent.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Event created successfully")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
