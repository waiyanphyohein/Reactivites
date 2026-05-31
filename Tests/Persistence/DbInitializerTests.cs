using Domain;
using FluentAssertions;
using Persistence;
using Tests.Application.TestHelpers;
using Xunit;

namespace Tests.PersistenceLayer;

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
    public async Task Initialize_WhenAnyTableAlreadyHasData_SkipsSeeding()
    {
        // Arrange
        _context.Tags.Add(new Tag { TagName = "Existing tag" });
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.Initialize(_context);

        // Assert
        _context.Tags.Should().ContainSingle(tag => tag.TagName == "Existing tag");
        _context.Activities.Should().BeEmpty();
        _context.People.Should().BeEmpty();
        _context.Groups.Should().BeEmpty();
        _context.Events.Should().BeEmpty();
    }
}
