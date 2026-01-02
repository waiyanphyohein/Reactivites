using Application.Activities.Queries;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Activities.Queries;

public class GetActivityListHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetActivityList> _logger;

    public GetActivityListHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetActivityList>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithActivities_ReturnsAllActivities()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(5);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityList.Query();
        var handler = new GetActivityList.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().BeEquivalentTo(activities, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetActivityList.Query();
        var handler = new GetActivityList.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(100);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var query = new GetActivityList.Query();
        var handler = new GetActivityList.Handler(_context, _logger);
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
        var query = new GetActivityList.Query();
        var handler = new GetActivityList.Handler(_context, _logger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Fetching activity list")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
