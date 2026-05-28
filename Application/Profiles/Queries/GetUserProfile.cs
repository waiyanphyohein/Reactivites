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
            var username = request.Username.Trim();
            var allActivities = await context.Activities
                .OrderBy(a => a.Date)
                .ToListAsync(cancellationToken);

            var userActivities = allActivities
                .Where(activity =>
                    !string.IsNullOrWhiteSpace(activity.CreatorDisplayName) &&
                    string.Equals(activity.CreatorDisplayName, username, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var futureActivities = userActivities
                .Where(activity => activity.Date.ToUniversalTime() >= now)
                .Select(MapActivity)
                .ToList();

            var pastActivities = userActivities
                .Where(activity => activity.Date.ToUniversalTime() < now)
                .Select(MapActivity)
                .ToList();

            return new UserProfileDto
            {
                Username = username.ToLowerInvariant(),
                DisplayName = ToDisplayName(username),
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
                Venue = activity.Venue,
                CreatorDisplayName = activity.CreatorDisplayName
            };
        }

        private static string ToDisplayName(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return "User";

            var trimmed = username.Trim();
            return char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
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
    public string? CreatorDisplayName { get; init; }
}
