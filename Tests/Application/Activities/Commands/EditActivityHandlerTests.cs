using Application.Activities.Queries;
using Tests.Application.TestHelpers;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Tests.Application.Activities.Commands;

public class EditActivityHandlerTests : IDisposable
{
    private readonly Persistence.AppDbContext _context;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<EditActivity.Handler> _logger;

    public EditActivityHandlerTests()
    {
        _context = DbContextMockHelper.CreateInMemoryContext();
        _mapper = MapperFactory.CreateMapper();
        _logger = MockLoggerFactory.CreateLogger<EditActivity.Handler>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingActivity_UpdatesActivity()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = new Activity
        {
            Id = existingActivity.Id,
            Title = "Updated Title",
            Date = DateTime.UtcNow.AddDays(10),
            Description = "Updated Description",
            Category = "culture",
            City = "Paris",
            Venue = "Updated Venue",
            Latitude = 48.8566,
            Longitude = 2.3522,
            IsCancelled = true
        };

        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.City.Should().Be("Paris");
        result.Venue.Should().Be("Updated Venue");
    }

    [Fact]
    public async Task Handle_NonExistentActivity_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var activity = ActivityTestData.CreateValidActivity(nonExistentId);
        var command = new EditActivity.Command(activity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Activity not found*");
    }

    [Fact]
    public async Task Handle_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        existingActivity.Title = "Original Title";
        existingActivity.Description = "Original Description";
        existingActivity.City = "London";
        existingActivity.Venue = "Original Venue";
        existingActivity.Date = DateTime.UtcNow.AddDays(5);

        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        // Create partial update with some null/empty fields
        var partialUpdate = ActivityTestData.CreatePartialUpdateActivity(existingActivity.Id);

        var command = new EditActivity.Command(partialUpdate);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title"); // Should update
        result.Description.Should().Be("Updated Description"); // Should update
        result.Venue.Should().Be("Updated Venue"); // Should update
        result.City.Should().Be("London"); // Should NOT update (empty string)
        result.Category.Should().Be("drinks"); // Should NOT update (null)
        result.Latitude.Should().Be(51.5074); // Should NOT update (0)
        result.Longitude.Should().Be(-0.1278); // Should NOT update (0)
    }

    [Fact]
    public async Task Handle_AutoMapperIntegration_MapsCorrectly()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = new Activity
        {
            Id = existingActivity.Id,
            Title = "Mapped Title",
            Date = existingActivity.Date,
            Description = existingActivity.Description,
            Category = existingActivity.Category,
            City = existingActivity.City,
            Venue = existingActivity.Venue,
            Latitude = existingActivity.Latitude,
            Longitude = existingActivity.Longitude,
            IsCancelled = existingActivity.IsCancelled
        };

        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Title.Should().Be("Mapped Title");
        result.Id.Should().Be(existingActivity.Id);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ThrowsCancellationException()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = ActivityTestData.CreateValidActivity(existingActivity.Id);
        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ValidUpdate_LogsInformation()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = ActivityTestData.CreateValidActivity(existingActivity.Id);
        updatedActivity.Title = "Updated for Logging";

        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(existingActivity.Id)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_ValidUpdate_SavesChangesToDatabase()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = new Activity
        {
            Id = existingActivity.Id,
            Title = "Saved Title",
            Date = existingActivity.Date,
            Description = existingActivity.Description,
            Category = existingActivity.Category,
            City = existingActivity.City,
            Venue = existingActivity.Venue,
            Latitude = existingActivity.Latitude,
            Longitude = existingActivity.Longitude,
            IsCancelled = existingActivity.IsCancelled
        };

        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedActivity = await _context.Activities.FindAsync(existingActivity.Id);
        savedActivity.Should().NotBeNull();
        savedActivity!.Title.Should().Be("Saved Title");
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedActivity()
    {
        // Arrange
        var existingActivity = ActivityTestData.CreateValidActivity();
        _context.Activities.Add(existingActivity);
        await _context.SaveChangesAsync();

        var updatedActivity = ActivityTestData.CreateValidActivity(existingActivity.Id);
        updatedActivity.Title = "Returned Title";

        var command = new EditActivity.Command(updatedActivity);
        var handler = new EditActivity.Handler(_context, _mapper, _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingActivity.Id);
        result.Title.Should().Be("Returned Title");
    }
}
