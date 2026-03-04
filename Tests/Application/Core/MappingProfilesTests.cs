using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Tests.Application.TestHelpers;
using Xunit;

namespace Tests.Application.Core;

public class MappingProfilesTests
{
    private readonly IMapper _mapper;

    public MappingProfilesTests()
    {
        _mapper = MapperFactory.CreateMapper();
    }

    // ─────────────────────────────────────────────
    // Activity → Activity mapping
    // ─────────────────────────────────────────────

    [Fact]
    public void ActivityMap_ValidValues_MapsAllFields()
    {
        // Arrange
        var source = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            Title = "New Title",
            Date = new DateTime(2024, 6, 15, 12, 0, 0),
            Description = "New Description",
            Category = "music",
            City = "London",
            Venue = "Arena",
            IsCancelled = true,
            Latitude = 51.5,
            Longitude = -0.1
        };
        var destination = CreateActivity(source.Id);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Title.Should().Be("New Title");
        destination.Date.Should().Be(source.Date);
        destination.Description.Should().Be("New Description");
        destination.Category.Should().Be("music");
        destination.City.Should().Be("London");
        destination.Venue.Should().Be("Arena");
        destination.Latitude.Should().Be(51.5);
        destination.Longitude.Should().Be(-0.1);
    }

    [Fact]
    public void ActivityMap_NullString_PreservesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(title: "Keep This Title");
        var source = CreateActivity(destination.Id);
        source.Title = null!;

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Title.Should().Be("Keep This Title");
    }

    [Fact]
    public void ActivityMap_EmptyString_PreservesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(description: "Keep This Description");
        var source = CreateActivity(destination.Id, description: "");

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Description.Should().Be("Keep This Description");
    }

    [Fact]
    public void ActivityMap_WhitespaceString_PreservesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(city: "Keep This City");
        var source = CreateActivity(destination.Id, city: "   ");

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.City.Should().Be("Keep This City");
    }

    [Fact]
    public void ActivityMap_ZeroDouble_PreservesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(latitude: 51.5);
        var source = CreateActivity(destination.Id, latitude: 0);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Latitude.Should().Be(51.5);
    }

    [Fact]
    public void ActivityMap_DefaultDateTime_PreservesDestinationValue()
    {
        // Arrange
        var originalDate = new DateTime(2024, 1, 1);
        var destination = CreateActivity(date: originalDate);
        var source = CreateActivity(destination.Id, date: default(DateTime));

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Date.Should().Be(originalDate);
    }

    [Fact]
    public void ActivityMap_NonZeroDouble_UpdatesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(latitude: 10.0);
        var source = CreateActivity(destination.Id, latitude: 99.9);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Latitude.Should().Be(99.9);
    }

    [Fact]
    public void ActivityMap_NonDefaultDateTime_UpdatesDestinationValue()
    {
        // Arrange
        var destination = CreateActivity(date: new DateTime(2024, 1, 1));
        var newDate = new DateTime(2025, 12, 31);
        var source = CreateActivity(destination.Id, date: newDate);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Date.Should().Be(newDate);
    }

    [Fact]
    public void ActivityMap_BooleanFalse_UpdatesDestinationValue()
    {
        // Arrange - bool is never ignored; IsCancelled = false should still update
        var destination = CreateActivity(isCancelled: true);
        var source = CreateActivity(destination.Id, isCancelled: false);

        // Act
        _mapper.Map(source, destination);

        // Assert - false is not null / not empty string / not 0-numeric, so it maps
        // Note: bool has no special ignore rule in MappingProfiles
        destination.IsCancelled.Should().BeFalse();
    }

    // ─────────────────────────────────────────────
    // Event → Event mapping
    // ─────────────────────────────────────────────

    [Fact]
    public void EventMap_ValidValues_MapsAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, "Old Name", "Old Desc", "Old Location", "Old Group", "Old Group Desc");
        var source = CreateEvent(id, "New Name", "New Desc", "New Location", "New Group", "New Group Desc");

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.EventName.Should().Be("New Name");
        destination.EventDescription.Should().Be("New Desc");
        destination.Location.Should().Be("New Location");
        destination.GroupName.Should().Be("New Group");
        destination.GroupDescription.Should().Be("New Group Desc");
    }

    [Fact]
    public void EventMap_NullString_PreservesDestinationValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, "Keep This", groupName: "G");
        var source = CreateEvent(id, "Placeholder", groupName: "G");
        source.EventName = null!;

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.EventName.Should().Be("Keep This");
    }

    [Fact]
    public void EventMap_EmptyString_PreservesDestinationValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, location: "Keep This Location", groupName: "G");
        var source = CreateEvent(id, location: "", groupName: "G");

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Location.Should().Be("Keep This Location");
    }

    [Fact]
    public void EventMap_WhitespaceString_PreservesDestinationValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, eventDescription: "Keep This Desc", groupName: "G");
        var source = CreateEvent(id, eventDescription: "   ", groupName: "G");

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.EventDescription.Should().Be("Keep This Desc");
    }

    [Fact]
    public void EventMap_EmptyGuid_PreservesDestinationValue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, groupName: "G");
        var source = CreateEvent(id, groupName: "G", groupId: Guid.Empty);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.GroupId.Should().Be(id);
    }

    [Fact]
    public void EventMap_NonEmptyGuid_UpdatesDestinationValue()
    {
        // Arrange
        var oldId = Guid.NewGuid();
        var newGroupId = Guid.NewGuid();
        var destination = CreateEvent(oldId, groupName: "G", groupId: oldId);
        var source = CreateEvent(oldId, groupName: "G", groupId: newGroupId);

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.GroupId.Should().Be(newGroupId);
    }

    [Fact]
    public void EventMap_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var destination = CreateEvent(id, "Original Name", "Original Desc", "Original Location", "Original Group");
        var source = CreateEvent(
            id,
            eventName: "Updated Name",
            eventDescription: null!,
            location: "",
            groupName: "Updated Group",
            groupId: Guid.Empty); // should not overwrite

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.EventName.Should().Be("Updated Name");
        destination.GroupName.Should().Be("Updated Group");
        destination.EventDescription.Should().Be("Original Desc");
        destination.Location.Should().Be("Original Location");
        destination.GroupId.Should().Be(id);
    }

    [Fact]
    public void MappingConfiguration_IsValid()
    {
        // Arrange & Act
        var config = MapperFactory.CreateMapper().ConfigurationProvider;

        // Assert - This throws if the configuration is invalid
        config.AssertConfigurationIsValid();
    }

    private static Activity CreateActivity(
        string? id = null,
        string title = "Default Title",
        DateTime? date = null,
        string? description = "Default Description",
        string? category = "Default Category",
        string city = "Default City",
        string venue = "Default Venue",
        bool isCancelled = false,
        double latitude = 1.0,
        double longitude = 1.0)
    {
        return new Activity
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Title = title,
            Date = date ?? DateTime.UtcNow,
            Description = description,
            Category = category,
            City = city,
            Venue = venue,
            IsCancelled = isCancelled,
            Latitude = latitude,
            Longitude = longitude
        };
    }

    private static Event CreateEvent(
        Guid? eventId = null,
        string eventName = "Default Event Name",
        string? eventDescription = "Default Event Description",
        string? location = "Default Location",
        string groupName = "Default Group Name",
        string? groupDescription = "Default Group Description",
        Guid? groupId = null,
        List<Person>? organizers = null)
    {
        var id = eventId ?? Guid.NewGuid();
        return new Event
        {
            EventId = id,
            EventName = eventName,
            EventDescription = eventDescription,
            Location = location,
            GroupId = groupId ?? id,
            GroupName = groupName,
            GroupDescription = groupDescription,
            Organizers = organizers ?? new List<Person>()
        };
    }
}
