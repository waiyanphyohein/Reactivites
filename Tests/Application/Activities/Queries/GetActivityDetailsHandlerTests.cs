using Application.Activities.Queries;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Activities.Queries;

public class GetActivityDetailsHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<GetActivityDetails.Handler> _logger;

    public GetActivityDetailsHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<GetActivityDetails.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingActivity_ReturnsActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var query = new GetActivityDetails.Query { Id = activity.Id };
        var handler = new GetActivityDetails.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(activity.Id);
        result.Title.Should().Be(activity.Title);
        result.City.Should().Be(activity.City);
    }

    [Fact]
    public async Task Handle_NonExistentActivity_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new GetActivityDetails.Query { Id = nonExistentId };
        var handler = new GetActivityDetails.Handler(_context, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.NotFound)
            .WithMessage("*Activity not found*");
    }

    [Fact]
    public async Task Handle_NonExistentActivity_LogsWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new GetActivityDetails.Query { Id = nonExistentId };
        var handler = new GetActivityDetails.Handler(_context, _logger);

        // Act
        try
        {
            await handler.Handle(query, CancellationToken.None);
        }
        catch (HttpRequestException)
        {
            // Expected exception
        }

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Activity not found")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(Skip = "In-memory database doesn't throw TaskCanceledException on immediate cancellation")]
    public async Task Handle_CancellationRequested_ThrowsRequestTimeoutException()
    {
        // This test is skipped because the in-memory database completes FindAsync too quickly
        // to respect the cancellation token, so no exception is thrown
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Handle_ValidId_UsesCorrectId()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(3);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var targetActivity = activities[1];
        var query = new GetActivityDetails.Query { Id = targetActivity.Id };
        var handler = new GetActivityDetails.Handler(_context, _logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(targetActivity.Id);
        result.Title.Should().Be(targetActivity.Title);
    }
}
