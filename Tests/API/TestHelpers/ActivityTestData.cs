using Domain;

namespace Tests.API.TestHelpers;

public static class ActivityTestData
{
    public static Activity CreateValidActivity(string? id = null)
    {
        return new Activity
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Title = "Test Activity",
            Date = DateTime.UtcNow.AddDays(7),
            Description = "Test Description",
            Category = "drinks",
            City = "London",
            Venue = "Test Venue",
            Latitude = 51.5074,
            Longitude = -0.1278,
            IsCancelled = false
        };
    }

    public static List<Activity> CreateActivityList(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Activity {i}",
                Date = DateTime.UtcNow.AddDays(i),
                Description = $"Description {i}",
                Category = "drinks",
                City = "London",
                Venue = $"Venue {i}",
                Latitude = 51.5074,
                Longitude = -0.1278,
                IsCancelled = false
            })
            .ToList();
    }
}
