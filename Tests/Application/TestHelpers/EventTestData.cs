using Domain;

namespace Tests.Application.TestHelpers;

public static class EventTestData
{
    public static Event CreateValidEvent(Guid? eventId = null, Guid? groupId = null)
    {
        var id = eventId ?? Guid.NewGuid();
        var resolvedGroupId = groupId ?? CreateDistinctGroupId(id);

        return new Event
        {
            EventId = id,
            EventName = "Test Event",
            EventDescription = "Test Event Description",
            Location = "Test Location",
            GroupId = resolvedGroupId,
            GroupName = "Test Group",
            GroupDescription = "Test Group Description",
            Organizers = new List<Person>
            {
                new Person { FirstName = "John", LastName = "Doe" }
            }
        };
    }

    public static Event CreateEventWithEmptyIds()
    {
        var eventEntity = CreateValidEvent();
        eventEntity.EventId = Guid.Empty;
        eventEntity.GroupId = Guid.Empty;
        return eventEntity;
    }

    public static List<Event> CreateEventList(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i =>
            {
                var id = Guid.NewGuid();
                return new Event
                {
                    EventId = id,
                    EventName = $"Event {i}",
                    EventDescription = $"Description {i}",
                    Location = $"Location {i}",
                    GroupId = CreateDistinctGroupId(id),
                    GroupName = $"Group {i}",
                    GroupDescription = $"Group Description {i}",
                    Organizers = new List<Person>
                    {
                        new Person { FirstName = $"Organizer{i}", LastName = "Smith" }
                    }
                };
            })
            .ToList();
    }

    public static Event CreatePartialUpdateEvent(Guid eventId, List<Person>? organizers = null)
    {
        // GroupId == Guid.Empty so AutoMapper ignores it (preserves original GroupId)
        // Organizers reuses the original tracked list to avoid EF tracking conflicts
        return new Event
        {
            EventId = eventId,
            EventName = "Updated Event Name",
            EventDescription = null,   // null → AutoMapper ignores (preserves original)
            Location = "",             // empty string → AutoMapper ignores (preserves original)
            GroupId = Guid.Empty,      // Guid.Empty → AutoMapper ignores (preserves original)
            GroupName = "Updated Group Name",
            GroupDescription = null,   // null → AutoMapper ignores (preserves original)
            Organizers = organizers ?? new List<Person>()
        };
    }

    private static Guid CreateDistinctGroupId(Guid eventId)
    {
        var groupId = Guid.NewGuid();
        return groupId == eventId ? Guid.NewGuid() : groupId;
    }
}
