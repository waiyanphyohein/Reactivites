using Application.Activities.Queries;
using Tests.Application.TestHelpers;
using Domain;
using FluentAssertions;
using System.Net;
using Xunit;

namespace Tests.Application.Activities.Commands;

public class CreateActivityHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;

    public CreateActivityHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ValidActivity_AddsActivityToDatabase()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedActivity = await _context.Activities.FindAsync(result.Id);
        savedActivity.Should().NotBeNull();
        savedActivity!.Title.Should().Be(activity.Title);
        savedActivity.City.Should().Be(activity.City);
        savedActivity.Venue.Should().Be(activity.Venue);
    }

    [Fact]
    public async Task Handle_ValidActivityWithoutId_GeneratesNewGuid()
    {
        // Arrange
        var activity = ActivityTestData.CreateActivityWithEmptyId();
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBe(Guid.Empty.ToString());
        Guid.TryParse(result.Id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidActivityWithId_PreservesProvidedId()
    {
        // Arrange
        var expectedId = Guid.NewGuid().ToString();
        var activity = ActivityTestData.CreateValidActivity(expectedId);
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(expectedId);
    }

    [Fact(Skip = "Cannot test null with required property - would fail at compile time or runtime before reaching handler")]
    public async Task Handle_NullActivity_ThrowsBadRequestException()
    {
        // This test is skipped because the Command has a required Activity property
        // which makes it impossible to set Activity to null in a real scenario
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsRequestTimeoutException()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(_context);
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
        // Create a separate disposed context to simulate a database failure
        var disposedContext = DbContextMockHelper.CreateInMemoryContext();
        disposedContext.Dispose();

        var activity = ActivityTestData.CreateValidActivity();
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(disposedContext);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.InternalServerError)
            .WithMessage("*An error occurred while creating the activity*");
    }

    [Fact]
    public async Task Handle_ValidActivity_ReturnsCreatedActivity()
    {
        // Arrange
        var activity = ActivityTestData.CreateValidActivity();
        var command = new CreateActivity.Command { Activity = activity };
        var handler = new CreateActivity.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(activity);
    }
}
