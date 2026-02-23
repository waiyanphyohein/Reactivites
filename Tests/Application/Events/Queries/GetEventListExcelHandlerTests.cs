using Application.Events.Queries;
using Application.Core.Helper;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.Application.Events.Queries;

public class GetEventListExcelHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetEventListExcel> _logger;
    private readonly ExcelExporter _excelExporter;

    public GetEventListExcelHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetEventListExcel>();
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
        var events = EventTestData.CreateEventList(3);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(_context, _logger, _excelExporter);

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
        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(_context, _logger, _excelExporter);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<byte[]>();
        result.Length.Should().BeGreaterThan(0); // Should have headers at minimum
    }

    [Fact]
    public async Task Handle_ValidRequest_LogsInformation()
    {
        // Arrange
        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(_context, _logger, _excelExporter);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Exporting event list to Excel format")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExcelExporterThrows_LogsError()
    {
        // Arrange
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(disposedContext, _logger, exporter);

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
            Arg.Is<object>(o => o.ToString()!.Contains("An error occurred while exporting events to Excel")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ExcelExporterThrows_RethrowsException()
    {
        // Arrange
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var exporter = new ExcelExporter(disposedContext);
        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(disposedContext, _logger, exporter);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_CancellationRequested_CancelsOperation()
    {
        // Arrange
        var events = EventTestData.CreateEventList(100);
        _context.Events.AddRange(events);
        await _context.SaveChangesAsync();

        var query = new GetEventListExcel.Query();
        var handler = new GetEventListExcel.Handler(_context, _logger, _excelExporter);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(query, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
