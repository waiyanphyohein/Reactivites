using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Xunit;

namespace Tests.Application.Core;

public class MappingProfilesTests
{
    private readonly IMapper _mapper;

    public MappingProfilesTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());
        _mapper = config.CreateMapper();
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
            Id = Guid.NewGuid(),
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
        var destination = new Activity { Id = source.Id };

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
        var destination = new Activity { Id = Guid.NewGuid(), Title = "Keep This Title" };
        var source = new Activity { Id = destination.Id, Title = null! };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Title.Should().Be("Keep This Title");
    }

    [Fact]
    public void ActivityMap_EmptyString_PreservesDestinationValue()
    {
        // Arrange
        var destination = new Activity { Id = Guid.NewGuid(), Description = "Keep This Description" };
        var source = new Activity { Id = destination.Id, Description = "" };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Description.Should().Be("Keep This Description");
    }

    [Fact]
    public void ActivityMap_WhitespaceString_PreservesDestinationValue()
    {
        // Arrange
        var destination = new Activity { Id = Guid.NewGuid(), City = "Keep This City" };
        var source = new Activity { Id = destination.Id, City = "   " };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.City.Should().Be("Keep This City");
    }

    [Fact]
    public void ActivityMap_ZeroDouble_PreservesDestinationValue()
    {
        // Arrange
        var destination = new Activity { Id = Guid.NewGuid(), Latitude = 51.5 };
        var source = new Activity { Id = destination.Id, Latitude = 0 };

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
        var destination = new Activity { Id = Guid.NewGuid(), Date = originalDate };
        var source = new Activity { Id = destination.Id, Date = default };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Date.Should().Be(originalDate);
    }

    [Fact]
    public void ActivityMap_NonZeroDouble_UpdatesDestinationValue()
    {
        // Arrange
        var destination = new Activity { Id = Guid.NewGuid(), Latitude = 10.0 };
        var source = new Activity { Id = destination.Id, Latitude = 99.9 };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Latitude.Should().Be(99.9);
    }

    [Fact]
    public void ActivityMap_NonDefaultDateTime_UpdatesDestinationValue()
    {
        // Arrange
        var destination = new Activity { Id = Guid.NewGuid(), Date = new DateTime(2024, 1, 1) };
        var newDate = new DateTime(2025, 12, 31);
        var source = new Activity { Id = destination.Id, Date = newDate };

        // Act
        _mapper.Map(source, destination);

        // Assert
        destination.Date.Should().Be(newDate);
    }

    [Fact]
    public void ActivityMap_BooleanFalse_UpdatesDestinationValue()
    {
        // Arrange - bool is never ignored; IsCancelled = false should still update
        var destination = new Activity { Id = Guid.NewGuid(), IsCancelled = true };
        var source = new Activity { Id = destination.Id, IsCancelled = false };

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
        var destination = new Event
        {
            EventId = id,
            GroupId = id,
            EventName = "Old Name",
            EventDescription = "Old Desc",
            Location = "Old Location",
            GroupName = "Old Group",
            GroupDescription = "Old Group Desc",
            Organizers = new List<Person> { new Person { FirstName = "A", LastName = "B" } }
        };

        var source = new Event
        {
            EventId = id,
            GroupId = id,
            EventName = "New Name",
            EventDescription = "New Desc",
            Location = "New Location",
            GroupName = "New Group",
            GroupDescription = "New Group Desc",
            Organizers = new List<Person> { new Person { FirstName = "C", LastName = "D" } }
        };

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
        var destination = new Event
        {
            EventId = id, GroupId = id, EventName = "Keep This", GroupName = "G",
            Organizers = new List<Person>()
        };
        var source = new Event
        {
            EventId = id, GroupId = id, EventName = null!, GroupName = "G",
            Organizers = new List<Person>()
        };

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
        var destination = new Event
        {
            EventId = id, GroupId = id, Location = "Keep This Location", GroupName = "G",
            Organizers = new List<Person>()
        };
        var source = new Event
        {
            EventId = id, GroupId = id, Location = "", GroupName = "G",
            Organizers = new List<Person>()
        };

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
        var destination = new Event
        {
            EventId = id, GroupId = id, EventDescription = "Keep This Desc", GroupName = "G",
            Organizers = new List<Person>()
        };
        var source = new Event
        {
            EventId = id, GroupId = id, EventDescription = "   ", GroupName = "G",
            Organizers = new List<Person>()
        };

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
        var destination = new Event
        {
            EventId = id, GroupId = id, GroupName = "G", Organizers = new List<Person>()
        };
        var source = new Event
        {
            EventId = id, GroupId = Guid.Empty, GroupName = "G", Organizers = new List<Person>()
        };

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
        var destination = new Event
        {
            EventId = oldId, GroupId = oldId, GroupName = "G", Organizers = new List<Person>()
        };
        var source = new Event
        {
            EventId = oldId, GroupId = newGroupId, GroupName = "G", Organizers = new List<Person>()
        };

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
        var destination = new Event
        {
            EventId = id,
            GroupId = id,
            EventName = "Original Name",
            EventDescription = "Original Desc",
            Location = "Original Location",
            GroupName = "Original Group",
            Organizers = new List<Person>()
        };

        var source = new Event
        {
            EventId = id,
            GroupId = Guid.Empty,       // should not overwrite
            EventName = "Updated Name", // should overwrite
            EventDescription = null!,   // should not overwrite
            Location = "",              // should not overwrite
            GroupName = "Updated Group",
            Organizers = new List<Person>()
        };

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
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());

        // Assert - This throws if the configuration is invalid
        config.AssertConfigurationIsValid();
    }
}
