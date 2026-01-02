using Application.Activities.Queries;
using Application.Core.Helper;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.Application.Activities.Queries;

public class GetActivityListExcelHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetActivityListExcel> _logger;
    private readonly ExcelExporter _excelExporter;

    public GetActivityListExcelHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetActivityListExcel>();
        _excelExporter = new ExcelExporter(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsExcelBytes()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(3);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsExcelBytesWithHeadersOnly()
    {
        // Arrange
        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        result.Length.Should().BeGreaterThan(0); // Should have headers
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(_context, _logger, _excelExporter);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Exporting activity list to Excel format")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExcelExporterThrows_LogsError()
    {
        // Arrange
        // Dispose the context to cause an error when ExcelExporter tries to query
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(disposedContext, _logger, exporter);

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
            Arg.Is<object>(o => o.ToString()!.Contains("An error occurred while exporting activities to Excel")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExcelExporterThrows_RethrowsException()
    {
        // Arrange
        // Dispose the context to cause an error
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(disposedContext, _logger, exporter);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_CancellationRequested_CancelsOperation()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(100);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityListExcel.Query();
        var handler = new GetActivityListExcel.Handler(_context, _logger, _excelExporter);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
