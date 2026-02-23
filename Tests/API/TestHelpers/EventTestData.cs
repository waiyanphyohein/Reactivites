using Domain;

namespace Tests.API.TestHelpers;

public static class EventTestData
{
    public static Event CreateValidEvent(Guid? eventId = null)
    {
        return new Event
        {
            EventId = eventId ?? Guid.NewGuid(),
            EventName = "Test Event",
            EventDescription = "Test Event Description",
            Location = "Test Location",
            GroupId = Guid.NewGuid(),
            GroupName = "Test Group",
            GroupDescription = "Test Group Description",
            Organizers = new List<Person>
            {
                new Person { FirstName = "John", LastName = "Doe" }
            }
        };
    }

    public static List<Event> CreateEventList(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Event
            {
                EventId = Guid.NewGuid(),
                EventName = $"Event {i}",
                EventDescription = $"Description {i}",
                Location = $"Location {i}",
                GroupId = Guid.NewGuid(),
                GroupName = $"Group {i}",
                GroupDescription = $"Group Description {i}",
                Organizers = new List<Person>
                {
                    new Person { FirstName = $"Organizer{i}", LastName = "Smith" }
                }
            })
            .ToList();
    }
}
