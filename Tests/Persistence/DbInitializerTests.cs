using Domain;
using FluentAssertions;
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
    public async Task Initialize_WhenAnyCoreTableHasData_DoesNotReseedDatabase()
    {
        // Arrange
        _context.Activities.Add(new Activity
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Existing Activity",
            Date = DateTime.UtcNow.AddDays(1),
            Description = "Already present",
            Category = "General",
            City = "Boston",
            Venue = "Existing Venue"
        });
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        _context.Activities.Should().ContainSingle();
        _context.Events.Should().BeEmpty();
        _context.People.Should().BeEmpty();
        _context.Tags.Should().BeEmpty();
        _context.Groups.Should().BeEmpty();
    }
}
