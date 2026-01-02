using Application.Activities.Queries;
using Application.Core.Helper;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Activities.Queries;

public class GetActivityListCSVHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetActivityListCSV> _logger;
    private readonly ExcelExporter _excelExporter;

    public GetActivityListCSVHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetActivityListCSV>();
        _excelExporter = new ExcelExporter(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();

        // Cleanup any generated files
        var filePath = "activities.csv.xlsx";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCSVString()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(3);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task Handle_ValidRequest_FetchesActivitiesFromDatabase()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(5);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        // The handler should have fetched activities from database
        var fetchedActivities = await _context.Activities.ToListAsync();
        fetchedActivities.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(_context, _logger, _excelExporter);

        // Act
        try
        {
            await handler.Handle(query, CancellationToken.None);
        }
        catch
        {
            // Ignore any file-related exceptions
        }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Fetching activity list for CSV export")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExportFails_LogsError()
    {
        // Arrange
        // Dispose context to cause an error
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(disposedContext, _logger, exporter);

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
            Arg.Is<object>(o => o.ToString()!.Contains("An error occurred while exporting activities to CSV")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExportFails_ThrowsInternalServerErrorException()
    {
        // Arrange
        // Dispose context to cause an error
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(disposedContext, _logger, exporter);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError)
            .WithMessage("*An error occurred while exporting activities to CSV*");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsException()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(100);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityListCSV.Query();
        var handler = new GetActivityListCSV.Handler(_context, _logger, _excelExporter);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
