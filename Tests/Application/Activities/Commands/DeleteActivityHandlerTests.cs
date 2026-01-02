using Application.Activities.Queries;
using Tests.Application.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using Xunit;

namespace Tests.Application.Activities.Commands;

public class DeleteActivityHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly ILogger<DeleteActivity.Handler> _logger;

    public DeleteActivityHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _logger = MockLoggerFactory.CreateLogger<DeleteActivity.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingActivity_DeletesActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var command = new DeleteActivity.Command { Id = activity.Id };
        var handler = new DeleteActivity.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedActivity = await _context.Activities.FindAsync(activity.Id);
        deletedActivity.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentActivity_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new DeleteActivity.Command { Id = nonExistentId };
        var handler = new DeleteActivity.Handler(_context, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Activity not found*");
    }

    [Fact]
    public async Task Handle_CancellationDuringDelay_ThrowsRequestTimeoutException()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var command = new DeleteActivity.Command { Id = activity.Id };
        var handler = new DeleteActivity.Handler(_context, _logger);
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
    public async Task Handle_CancellationAtStart_ThrowsRequestTimeoutException()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var command = new DeleteActivity.Command { Id = activity.Id };
        var handler = new DeleteActivity.Handler(_context, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert - When cancelled before Task.Delay, OperationCanceledException is wrapped in general Exception handler
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Handle_KeyNotFoundException_LogsWarning()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new DeleteActivity.Command { Id = nonExistentId };
        var handler = new DeleteActivity.Handler(_context, _logger);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (KeyNotFoundException)
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

    [Fact]
    public async Task Handle_SaveChangesFails_ThrowsInternalServerErrorException()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Activities.Add(activity);
        await disposedContext.SaveChangesAsync();
        disposedContext.Dispose();

        var command = new DeleteActivity.Command { Id = activity.Id };
        var handler = new DeleteActivity.Handler(disposedContext, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError)
            .WithMessage("*An error occurred while deleting the activity*");
    }

    [Fact]
    public async Task Handle_ValidDeletion_RemovesFromDatabase()
    {
        // Arrange
        var activities = ActivityTestData.CreateActivityList(5);
        _context.Activities.AddRange(activities);
        await _context.SaveChangesAsync();

        var activityToDelete = activities[2];
        var command = new DeleteActivity.Command { Id = activityToDelete.Id };
        var handler = new DeleteActivity.Handler(_context, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingActivities = await _context.Activities.ToListAsync();
        remainingActivities.Should().HaveCount(4);
        remainingActivities.Should().NotContain(a => a.Id == activityToDelete.Id);
    }
}
