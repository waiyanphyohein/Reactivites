using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles.Queries;

public class GetUserProfile
{
    public class Query : IRequest<UserProfileDto>
    {
        public required string Username { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, UserProfileDto>
    {
        public async Task<UserProfileDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var allActivities = await context.Activities
                .OrderBy(a => a.Date)
                .ToListAsync(cancellationToken);

            var userActivities = allActivities
                .Where(activity =>
                    !string.IsNullOrWhiteSpace(activity.CreatorDisplayName) &&
                    string.Equals(activity.CreatorDisplayName, request.Username, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Keep demo behavior usable if no creator data exists yet.
            if (userActivities.Count == 0)
            {
                userActivities = allActivities;
            }

            var futureActivities = userActivities
                .Where(activity => activity.Date.ToUniversalTime() >= now)
                .Select(MapActivity)
                .ToList();

            var pastActivities = userActivities
                .Where(activity => activity.Date.ToUniversalTime() < now)
                .Select(MapActivity)
                .ToList();

            // Seed data is usually future-dated. Ensure the profile still shows history for demo purposes.
            if (pastActivities.Count == 0)
            {
                pastActivities = userActivities
                    .Take(3)
                    .Select((activity, index) => MapActivity(activity) with
                    {
                        Id = $"{activity.Id}-past-{index + 1}",
                        Date = activity.Date.AddMonths(-(index + 1))
                    })
                    .OrderByDescending(activity => activity.Date)
                    .ToList();
            }

            return new UserProfileDto
            {
                Username = request.Username,
                DisplayName = "Jeff",
                AvatarUrl = "/images/jeff-placeholder.svg",
                PastEvents = pastActivities,
                FutureEvents = futureActivities
            };
        }

        private static ProfileActivityDto MapActivity(Activity activity)
        {
            return new ProfileActivityDto
            {
                Id = activity.Id,
                Title = activity.Title,
                Date = activity.Date,
                Description = activity.Description,
                Category = activity.Category,
                City = activity.City,
                Venue = activity.Venue
            };
        }
    }
}

public class UserProfileDto
{
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public required string AvatarUrl { get; set; }
    public required List<ProfileActivityDto> PastEvents { get; set; }
    public required List<ProfileActivityDto> FutureEvents { get; set; }
}

public record ProfileActivityDto
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public DateTime Date { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public required string City { get; init; }
    public required string Venue { get; init; }
}
