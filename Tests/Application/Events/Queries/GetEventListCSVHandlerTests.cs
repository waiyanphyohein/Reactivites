using Application.Events.Queries;
using Application.Core.Helper;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Events.Queries;

public class GetEventListCSVHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetEventListCSV> _logger;
    private readonly ExcelExporter _excelExporter;

    public GetEventListCSVHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetEventListCSV>();
        _excelExporter = new ExcelExporter(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCSVString()
    {
        // Arrange
        var events = EventTestData.CreateEventList(3);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task Handle_ValidRequest_ContainsCSVHeaders()
    {
        // Arrange
        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Contain("EventId");
        result.Should().Contain("EventName");
        result.Should().Contain("EventDescription");
        result.Should().Contain("Location");
        result.Should().Contain("GroupId");
        result.Should().Contain("GroupName");
        result.Should().Contain("GroupDescription");
    }

    [Fact]
    public async Task Handle_WithEvents_ContainsEventData()
    {
        // Arrange
        var eventEntity = EventTestData.CreateValidEvent();
        eventEntity.EventName = "CSV Test Event";
        eventEntity.Location = "CSV Location";
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Contain("CSV Test Event");
        result.Should().Contain("CSV Location");
    }

    [Fact]
    public async Task Handle_ValidRequest_FetchesEventsFromDatabase()
    {
        // Arrange
        var events = EventTestData.CreateEventList(5);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var fetchedEvents = await _context.Events.ToListAsync();
        fetchedEvents.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Fetching event list for CSV export")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExportFails_LogsError()
    {
        // Arrange
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(disposedContext, _logger, exporter);

        // Act
        try
        {
            await handler.Handle(query, CancellationToken.None);
        }
        catch
        {
            // Expected exception
        }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("An error occurred while exporting events to CSV")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExportFails_ThrowsInternalServerErrorException()
    {
        // Arrange
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(disposedContext, _logger, exporter);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError)
            .WithMessage("*An error occurred while exporting events to CSV*");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsException()
    {
        // Arrange
        var events = EventTestData.CreateEventList(100);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventListCSV.Query();
        var handler = new GetEventListCSV.Handler(_context, _logger, _excelExporter);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
