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
    public async Task Initialize_WhenDatabaseHasAnyExistingData_DoesNotSeedDuplicates()
    {
        // Arrange
        var existingActivity = new Activity
        {
            Title = "Existing Activity",
            Date = DateTime.UtcNow,
            City = "London",
            Venue = "Existing Venue"
        };
        await _context.Activities.AddAsync(existingActivity);
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        var activityCount = await _context.Activities.CountAsync();
        var seededTagCount = await _context.Tags.CountAsync();

        activityCount.Should().Be(1);
        seededTagCount.Should().Be(0);
    }
}

