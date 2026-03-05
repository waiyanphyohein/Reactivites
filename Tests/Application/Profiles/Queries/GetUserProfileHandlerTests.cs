using Application.Profiles.Queries;
using Domain;
using FluentAssertions;
using Tests.Application.TestHelpers;
using Xunit;

namespace Tests.Application.Profiles.Queries;

public class GetUserProfileHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;

    public GetUserProfileHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithMatchingCreator_ReturnsOnlyUsersActivities()
    {
        // Arrange
        _context.Activities.AddRange(
            new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Jeff Future Event",
                Date = DateTime.UtcNow.AddDays(3),
                Description = "Created by Jeff",
                Category = "Tech",
                City = "Boston",
                Venue = "Hub",
                CreatorDisplayName = "Jeff"
            },
            new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Sarah Future Event",
                Date = DateTime.UtcNow.AddDays(5),
                Description = "Created by Sarah",
                Category = "Social",
                City = "Austin",
                Venue = "Center",
                CreatorDisplayName = "Sarah"
            });
        await _context.SaveChangesAsync();

        var handler = new GetUserProfile.Handler(_context);

        // Act
        var result = await handler.Handle(new GetUserProfile.Query { Username = "jeff" }, CancellationToken.None);

        // Assert
        result.FutureEvents.Should().ContainSingle();
        result.FutureEvents[0].Title.Should().Be("Jeff Future Event");
    }

    [Fact]
    public async Task Handle_WhenNoCreatorMatch_FallsBackToAllActivities()
    {
        // Arrange
        _context.Activities.AddRange(
            new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Future Event",
                Date = DateTime.UtcNow.AddDays(2),
                Description = "Future",
                Category = "General",
                City = "Boston",
                Venue = "Venue A"
            },
            new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Past Event",
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Past",
                Category = "General",
                City = "Chicago",
                Venue = "Venue B"
            });
        await _context.SaveChangesAsync();

        var handler = new GetUserProfile.Handler(_context);

        // Act
        var result = await handler.Handle(new GetUserProfile.Query { Username = "unknown" }, CancellationToken.None);

        // Assert
        result.FutureEvents.Should().ContainSingle();
        result.PastEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenNoPastEvents_CreatesSyntheticPastEntries()
    {
        // Arrange
        _context.Activities.Add(new Activity
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Future Only Event",
            Date = DateTime.UtcNow.AddDays(7),
            Description = "Future only",
            Category = "General",
            City = "Seattle",
            Venue = "Venue"
        });
        await _context.SaveChangesAsync();

        var handler = new GetUserProfile.Handler(_context);

        // Act
        var result = await handler.Handle(new GetUserProfile.Query { Username = "jeff" }, CancellationToken.None);

        // Assert
        result.PastEvents.Should().NotBeEmpty();
        result.PastEvents[0].Id.Should().Contain("-past-");
    }
}
