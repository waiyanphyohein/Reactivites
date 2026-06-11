using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Application.TestHelpers;

namespace Tests.Persistence;

public class DbInitializerTests : IDisposable
{
    private readonly global::Persistence.AppDbContext _context;

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
    public async Task Initialize_WhenAnyTableAlreadyHasData_DoesNotSeedDuplicateData()
    {
        // Arrange
        var existingActivity = new Activity
        {
            Title = "Existing Activity",
            Date = DateTime.UtcNow,
            City = "London",
            Venue = "Existing Venue"
        };

        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        // Act
        await global::Persistence.DbInitializer.Initialize(_context);

        // Assert
        (await _context.Activities.CountAsync()).Should().Be(1);
        (await _context.People.CountAsync()).Should().Be(0);
        (await _context.Tags.CountAsync()).Should().Be(0);
        (await _context.Groups.CountAsync()).Should().Be(0);
        (await _context.Events.CountAsync()).Should().Be(0);
    }
}
