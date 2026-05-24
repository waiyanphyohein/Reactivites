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
    public async Task Initialize_WithExistingActivityAndEmptyRelatedTables_DoesNotSeedDuplicates()
    {
        // Arrange
        _context.Activities.Add(new Activity
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Existing Activity",
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Persisted before startup",
            Category = "General",
            City = "Boston",
            Venue = "Existing Venue"
        });
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        var activities = await _context.Activities.ToListAsync();
        activities.Should().ContainSingle();
        activities[0].Title.Should().Be("Existing Activity");
        (await _context.Tags.CountAsync()).Should().Be(0);
    }
}
