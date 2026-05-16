using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Application.TestHelpers;
using Xunit;

namespace Tests.Persistence;

public class DbInitializerTests : IDisposable
{
    private readonly AppDbContext _context;

    public DbInitializerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Initialize_WhenAnyDataAlreadyExists_DoesNotSeedDuplicates()
    {
        // Arrange
        _context.Activities.Add(new Activity
        {
            Title = "Existing Activity",
            Date = DateTime.UtcNow,
            City = "Existing City",
            Venue = "Existing Venue"
        });
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        (await _context.Activities.CountAsync()).Should().Be(1);
        (await _context.Events.CountAsync()).Should().Be(0);
        (await _context.People.CountAsync()).Should().Be(0);
        (await _context.Tags.CountAsync()).Should().Be(0);
        (await _context.Groups.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Initialize_SeedsEventsWithMatchingEventAndGroupIds()
    {
        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        var events = await _context.Events.AsNoTracking().ToListAsync();
        events.Should().NotBeEmpty();
        events.Should().OnlyContain(eventEntity => eventEntity.GroupId == eventEntity.EventId);
    }
}
