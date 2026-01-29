using System;
using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class DbInitializer
{
    /// <summary>
    /// Clears all seeded data from the database
    /// </summary>
    public static async Task ClearAllData(AppDbContext context)
    {
        try
        {
            // Delete in reverse order to respect foreign key constraints
            // Delete Events first (they inherit from Groups)
            context.Events.RemoveRange(context.Events);
            await context.SaveChangesAsync();

            // Delete Groups (standalone groups)
            context.Groups.RemoveRange(context.Groups);
            await context.SaveChangesAsync();

            // Delete Activities
            context.Activities.RemoveRange(context.Activities);
            await context.SaveChangesAsync();

            // Delete People (many-to-many relationships will be handled automatically)
            context.People.RemoveRange(context.People);
            await context.SaveChangesAsync();

            // Delete Tags (many-to-many relationships will be handled automatically)
            context.Tags.RemoveRange(context.Tags);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error clearing database: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Initializes the database with seed data
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="clearExistingData">If true, clears all existing data before seeding</param>
    public static async Task Initialize(AppDbContext context, bool clearExistingData = false)
    {
        try
        {
            // Clear existing data if requested
            if (clearExistingData)
            {
                await ClearAllData(context);
            }
            else
            {
                // Check if database is empty
                var hasAnyData = await context.Activities.AnyAsync() &&
                               await context.Events.AnyAsync() &&
                               await context.People.AnyAsync() &&
                               await context.Tags.AnyAsync() &&
                               await context.Groups.AnyAsync();

                if (hasAnyData)
                {
                    // Database already has data, skip seeding
                    return;
                }
            }

            // Seed initial data - order matters due to relationships
            var tags = GetTags();
            var people = GetPeople();
            var groups = GetGroups(people, tags);
            var events = GetEvents(people, tags, groups);
            var activities = GetActivities();

            // Add entities in proper order to respect foreign key relationships
            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();

            await context.People.AddRangeAsync(people);
            await context.SaveChangesAsync();

            // Reload Tags and People from context to ensure they're tracked
            // This is necessary for many-to-many relationships to work correctly
            var trackedTags = await context.Tags.ToListAsync();
            var trackedPeople = await context.People.ToListAsync();
            
            // Update Groups to use tracked entities for Organizers and GroupTags
            foreach (var group in groups)
            {
                if (group.Organizers != null && group.Organizers.Count > 0)
                {
                    // Replace Organizers with tracked entities
                    var personIds = group.Organizers.Select(p => p.PersonId).ToList();
                    group.Organizers = trackedPeople.Where(p => personIds.Contains(p.PersonId)).ToList();
                }
                
                if (group.GroupTags != null && group.GroupTags.Count > 0)
                {
                    // Replace GroupTags with tracked entities
                    var tagIds = group.GroupTags.Select(t => t.TagId).ToList();
                    group.GroupTags = trackedTags.Where(t => tagIds.Contains(t.TagId)).ToList();
                }
            }

            // Note: Groups are added through Events since Event inherits from Group
            // We need to add standalone Groups first, then Events
            await context.Groups.AddRangeAsync(groups);
            await context.SaveChangesAsync();

            // Update Events to use tracked entities for Tags and Registration
            // Also update inherited Group properties (Organizers, GroupTags)
            foreach (var evt in events)
            {
                if (evt.Organizers != null && evt.Organizers.Count > 0)
                {
                    // Replace Organizers with tracked entities
                    var personIds = evt.Organizers.Select(p => p.PersonId).ToList();
                    evt.Organizers = trackedPeople.Where(p => personIds.Contains(p.PersonId)).ToList();
                }
                
                if (evt.GroupTags != null && evt.GroupTags.Count > 0)
                {
                    // Replace GroupTags with tracked entities
                    var tagIds = evt.GroupTags.Select(t => t.TagId).ToList();
                    evt.GroupTags = trackedTags.Where(t => tagIds.Contains(t.TagId)).ToList();
                }
                
                if (evt.Tags != null && evt.Tags.Count > 0)
                {
                    // Replace Tags with tracked entities
                    var tagIds = evt.Tags.Select(t => t.TagId).ToList();
                    evt.Tags = trackedTags.Where(t => tagIds.Contains(t.TagId)).ToList();
                }
                
                if (evt.Registration != null && evt.Registration.Count > 0)
                {
                    // Replace Registration with tracked entities
                    var personIds = evt.Registration.Select(p => p.PersonId).ToList();
                    evt.Registration = trackedPeople.Where(p => personIds.Contains(p.PersonId)).ToList();
                }
            }

            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            await context.Activities.AddRangeAsync(activities);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log and rethrow so Program.cs can handle it
            throw new InvalidOperationException($"Error seeding database: {ex.Message}", ex);
        }
    }

    private static List<Tag> GetTags()
    {
        return new List<Tag> {
            new Tag { TagName = "Adventure" },
            new Tag { TagName = "Dating" },
            new Tag { TagName = "Sports" },
            new Tag { TagName = "Outdoors" },
            new Tag { TagName = "Tech" },
            new Tag { TagName = "Networking" },
            new Tag { TagName = "Food & Drink" },
            new Tag { TagName = "Arts & Culture" },
            new Tag { TagName = "Health & Wellness" },
            new Tag { TagName = "Music" },
            new Tag { TagName = "Education" },
            new Tag { TagName = "Business" },
            new Tag { TagName = "Volunteer" },
            new Tag { TagName = "Social" },
            new Tag { TagName = "Fitness" },
            new Tag { TagName = "Travel" },
            new Tag { TagName = "Photography" },
            new Tag { TagName = "Gaming" }
        };
    }

    private static List<Person> GetPeople()
    {
        var now = DateTime.Now;
        return new List<Person>{
            new Person {
                FirstName = "Sarah",
                LastName = "Johnson",
                MiddleName = "Marie",
                Age = 28,
                DateOfBirth = new DateTime(now.Year - 28, 5, 15),
                Address = "123 Main St, San Francisco, CA 94102",
                Interests = "Technology, Networking, Hiking"
            },
            new Person {
                FirstName = "Michael",
                LastName = "Chen",
                Age = 32,
                DateOfBirth = new DateTime(now.Year - 32, 8, 22),
                Address = "456 Oak Avenue, Seattle, WA 98101",
                Interests = "Sports, Food, Travel"
            },
            new Person {
                FirstName = "Emily",
                LastName = "Rodriguez",
                MiddleName = "Grace",
                Age = 25,
                DateOfBirth = new DateTime(now.Year - 25, 3, 10),
                Address = "789 Pine Street, Austin, TX 78701",
                Interests = "Arts, Music, Photography"
            },
            new Person {
                FirstName = "David",
                LastName = "Thompson",
                Age = 35,
                DateOfBirth = new DateTime(now.Year - 35, 11, 5),
                Address = "321 Elm Drive, Boston, MA 02101",
                Interests = "Business, Education, Volunteer Work"
            },
            new Person {
                FirstName = "Jessica",
                LastName = "Williams",
                MiddleName = "Ann",
                Age = 29,
                DateOfBirth = new DateTime(now.Year - 29, 7, 18),
                Address = "654 Maple Lane, Portland, OR 97201",
                Interests = "Food & Drink, Social Events, Fitness"
            },
            new Person {
                FirstName = "James",
                LastName = "Anderson",
                Age = 31,
                DateOfBirth = new DateTime(now.Year - 31, 2, 28),
                Address = "987 Cedar Road, Chicago, IL 60601",
                Interests = "Sports, Gaming, Outdoors"
            },
            new Person {
                FirstName = "Amanda",
                LastName = "Martinez",
                MiddleName = "Rose",
                Age = 27,
                DateOfBirth = new DateTime(now.Year - 27, 9, 12),
                Address = "147 Birch Street, Los Angeles, CA 90001",
                Interests = "Health & Wellness, Yoga, Meditation"
            },
            new Person {
                FirstName = "Robert",
                LastName = "Taylor",
                Age = 33,
                DateOfBirth = new DateTime(now.Year - 33, 6, 25),
                Address = "258 Spruce Avenue, New York, NY 10001",
                Interests = "Tech, Startups, Networking"
            },
            new Person {
                FirstName = "Lisa",
                LastName = "Brown",
                MiddleName = "Jane",
                Age = 26,
                DateOfBirth = new DateTime(now.Year - 26, 4, 8),
                Address = "369 Willow Way, Denver, CO 80201",
                Interests = "Adventure, Travel, Photography"
            },
            new Person {
                FirstName = "Christopher",
                LastName = "Davis",
                Age = 30,
                DateOfBirth = new DateTime(now.Year - 30, 12, 3),
                Address = "741 Ash Boulevard, Miami, FL 33101",
                Interests = "Dating, Social Events, Music"
            },
            new Person {
                FirstName = "Michelle",
                LastName = "Wilson",
                MiddleName = "Lynn",
                Age = 34,
                DateOfBirth = new DateTime(now.Year - 34, 1, 20),
                Address = "852 Cherry Court, San Diego, CA 92101",
                Interests = "Volunteer Work, Community Service, Education"
            },
            new Person {
                FirstName = "Daniel",
                LastName = "Moore",
                Age = 28,
                DateOfBirth = new DateTime(now.Year - 28, 10, 14),
                Address = "963 Magnolia Drive, Atlanta, GA 30301",
                Interests = "Business, Finance, Networking"
            }
        };
    }

    private static List<Group> GetGroups(List<Person> people, List<Tag> tags)
    {
        return new List<Group>{
            new Group{
                GroupName = "Tech Innovators Network",
                GroupDescription = "A community of technology professionals and entrepreneurs sharing knowledge and opportunities.",
                Organizers = new List<Person> { people[0], people[7] },
                GroupTags = new List<Tag> { tags[4], tags[5], tags[11] }
            },
            new Group{
                GroupName = "Outdoor Adventure Club",
                GroupDescription = "Exploring nature through hiking, camping, and outdoor activities.",
                Organizers = new List<Person> { people[8], people[5] },
                GroupTags = new List<Tag> { tags[0], tags[3], tags[14] }
            },
            new Group{
                GroupName = "Food & Social Events",
                GroupDescription = "Connecting people through food, drinks, and social gatherings.",
                Organizers = new List<Person> { people[4], people[1] },
                GroupTags = new List<Tag> { tags[6], tags[13], tags[1] }
            },
            new Group{
                GroupName = "Arts & Culture Society",
                GroupDescription = "Promoting arts, music, and cultural events in the community.",
                Organizers = new List<Person> { people[2], people[6] },
                GroupTags = new List<Tag> { tags[8], tags[9], tags[16] }
            },
            new Group{
                GroupName = "Health & Wellness Community",
                GroupDescription = "Supporting healthy lifestyles through yoga, fitness, and wellness activities.",
                Organizers = new List<Person> { people[6], people[4] },
                GroupTags = new List<Tag> { tags[8], tags[14], tags[0] }
            },
            new Group{
                GroupName = "Business Networking Group",
                GroupDescription = "Professional networking and business development opportunities.",
                Organizers = new List<Person> { people[3], people[11] },
                GroupTags = new List<Tag> { tags[5], tags[11], tags[10] }
            },
            new Group{
                GroupName = "Volunteer & Community Service",
                GroupDescription = "Making a difference through volunteer work and community service projects.",
                Organizers = new List<Person> { people[10], people[3] },
                GroupTags = new List<Tag> { tags[12], tags[10], tags[13] }
            },
            new Group{
                GroupName = "Sports & Fitness Enthusiasts",
                GroupDescription = "Organizing sports events, fitness challenges, and athletic activities.",
                Organizers = new List<Person> { people[5], people[1] },
                GroupTags = new List<Tag> { tags[2], tags[14], tags[3] }
            }
        };
    }

    private static List<Event> GetEvents(List<Person> people, List<Tag> tags, List<Group> groups)
    {
        var now = DateTime.Now;
        return new List<Event>{
            new Event {
                GroupName = groups[0].GroupName,
                GroupDescription = groups[0].GroupDescription,
                Organizers = groups[0].Organizers,
                GroupTags = groups[0].GroupTags,
                EventName = "Tech Startup Pitch Night",
                EventDescription = "Join us for an evening of innovation! Local startups will pitch their ideas to investors and the community. Network with entrepreneurs, developers, and tech enthusiasts.",
                Location = "SoMa Startup Hub, San Francisco, CA",
                Tags = new List<Tag> { tags[4], tags[5], tags[11] },
                Registration = new List<Person> { people[0], people[7], people[11], people[3] }
            },
            new Event {
                GroupName = groups[1].GroupName,
                GroupDescription = groups[1].GroupDescription,
                Organizers = groups[1].Organizers,
                GroupTags = groups[1].GroupTags,
                EventName = "Mountain Hiking Adventure",
                EventDescription = "Experience breathtaking views on our guided mountain hike. Suitable for intermediate hikers. Bring water, snacks, and proper hiking gear.",
                Location = "Rocky Mountain National Park, CO",
                Tags = new List<Tag> { tags[0], tags[3], tags[14] },
                Registration = new List<Person> { people[8], people[5], people[1], people[9] }
            },
            new Event {
                GroupName = groups[2].GroupName,
                GroupDescription = groups[2].GroupDescription,
                Organizers = groups[2].Organizers,
                GroupTags = groups[2].GroupTags,
                EventName = "Food & Wine Tasting Evening",
                EventDescription = "Savor delicious cuisine and fine wines from local restaurants and wineries. Perfect for food lovers and social butterflies.",
                Location = "Downtown Culinary Center, Portland, OR",
                Tags = new List<Tag> { tags[6], tags[13], tags[1] },
                Registration = new List<Person> { people[4], people[1], people[9], people[2] }
            },
            new Event {
                GroupName = groups[3].GroupName,
                GroupDescription = groups[3].GroupDescription,
                Organizers = groups[3].Organizers,
                GroupTags = groups[3].GroupTags,
                EventName = "Live Music & Art Gallery Opening",
                EventDescription = "Celebrate local artists with live music performances and gallery exhibitions. Meet the artists and enjoy refreshments.",
                Location = "Contemporary Arts Center, Austin, TX",
                Tags = new List<Tag> { tags[8], tags[9], tags[16] },
                Registration = new List<Person> { people[2], people[6], people[8], people[4] }
            },
            new Event {
                GroupName = groups[4].GroupName,
                GroupDescription = groups[4].GroupDescription,
                Organizers = groups[4].Organizers,
                GroupTags = groups[4].GroupTags,
                EventName = "Sunrise Yoga & Meditation",
                EventDescription = "Start your day with peace and mindfulness. All levels welcome. Mats provided. Followed by healthy breakfast.",
                Location = "Beachfront Park, Los Angeles, CA",
                Tags = new List<Tag> { tags[8], tags[14] },
                Registration = new List<Person> { people[6], people[4], people[2], people[9] }
            },
            new Event {
                GroupName = groups[5].GroupName,
                GroupDescription = groups[5].GroupDescription,
                Organizers = groups[5].Organizers,
                GroupTags = groups[5].GroupTags,
                EventName = "Business Networking Breakfast",
                EventDescription = "Connect with local business professionals over breakfast. Share ideas, opportunities, and build meaningful professional relationships.",
                Location = "Grand Hotel Conference Center, Boston, MA",
                Tags = new List<Tag> { tags[5], tags[11], tags[10] },
                Registration = new List<Person> { people[3], people[11], people[0], people[7] }
            },
            new Event {
                GroupName = groups[6].GroupName,
                GroupDescription = groups[6].GroupDescription,
                Organizers = groups[6].Organizers,
                GroupTags = groups[6].GroupTags,
                EventName = "Community Garden Planting Day",
                EventDescription = "Help us plant vegetables and flowers in our community garden. Great way to give back and meet neighbors. Tools and refreshments provided.",
                Location = "Brooklyn Community Garden, New York, NY",
                Tags = new List<Tag> { tags[12], tags[10], tags[13] },
                Registration = new List<Person> { people[10], people[3], people[6], people[2] }
            },
            new Event {
                GroupName = groups[7].GroupName,
                GroupDescription = groups[7].GroupDescription,
                Organizers = groups[7].Organizers,
                GroupTags = groups[7].GroupTags,
                EventName = "Charity 5K Fun Run",
                EventDescription = "Run or walk to support local children's charities. All fitness levels welcome. T-shirts and medals for all participants.",
                Location = "Lakefront Trail, Chicago, IL",
                Tags = new List<Tag> { tags[2], tags[14], tags[12] },
                Registration = new List<Person> { people[5], people[1], people[8], people[4], people[6] }
            },
            new Event {
                GroupName = groups[0].GroupName,
                GroupDescription = groups[0].GroupDescription,
                Organizers = groups[0].Organizers,
                GroupTags = groups[0].GroupTags,
                EventName = "AI & Machine Learning Workshop",
                EventDescription = "Learn about the latest trends in AI and machine learning from industry experts. Hands-on coding session included.",
                Location = "Tech Innovation Lab, Seattle, WA",
                Tags = new List<Tag> { tags[4], tags[10], tags[5] },
                Registration = new List<Person> { people[0], people[7], people[11] }
            },
            new Event {
                GroupName = groups[2].GroupName,
                GroupDescription = groups[2].GroupDescription,
                Organizers = groups[2].Organizers,
                GroupTags = groups[2].GroupTags,
                EventName = "Speed Dating & Social Mixer",
                EventDescription = "Meet new people in a fun, relaxed environment. Multiple rounds of speed dating followed by open socializing.",
                Location = "Elegant Lounge, Miami, FL",
                Tags = new List<Tag> { tags[1], tags[13], tags[6] },
                Registration = new List<Person> { people[9], people[4], people[2], people[6], people[8] }
            }
        };
    }

    private static List<Activity> GetActivities()
    {
        return new List<Activity>
        {
            new Activity
            {
                Title = "Hiking in the Alps",
                Date = DateTime.Now.AddDays(7).Date.AddHours(8),
                Description = "Join us for a breathtaking hike through the Swiss Alps. We'll meet at 8:00 AM at the Grindelwald Trailhead. Please bring water, snacks, and appropriate hiking gear. The hike will last approximately 5 hours with a lunch break at Lake Bachalpsee.",
                Category = "Outdoors",
                City = "Interlaken",
                Venue = "Grindelwald Trailhead",
                Latitude = 46.6242,
                Longitude = 8.0414
            },
            new Activity
            {
                Title = "Downtown Food Festival",
                Date = DateTime.Now.AddDays(14).Date.AddHours(12),
                Description = "Sample dishes from over 30 of Portland's best restaurants and food trucks. The festival runs from noon to 8 PM at Waterfront Park. Enjoy live music, cooking demonstrations, and a kids' play area. Entry is free, but food and drinks are available for purchase.",
                Category = "Food & Drink",
                City = "Portland",
                Venue = "Waterfront Park",
                Latitude = 45.5152,
                Longitude = -122.6784
            },
            new Activity
            {
                Title = "Tech Innovators Meetup",
                Date = DateTime.Now.AddDays(21).Date.AddHours(18),
                Description = "Network with local tech professionals and hear talks from industry leaders at the SoMa Startup Hub. Doors open at 6:00 PM, with keynote at 7:00 PM. Complimentary pizza and drinks provided. Bring business cards for networking.",
                Category = "Networking",
                City = "San Francisco",
                Venue = "SoMa Startup Hub",
                Latitude = 37.7786,
                Longitude = -122.3893
            },
            new Activity
            {
                Title = "Art in the Park",
                Date = DateTime.Now.AddDays(28).Date.AddHours(10),
                Description = "A day of painting, sculpture, and crafts in Zilker Park. All ages and skill levels welcome. Materials provided for the first 100 participants. Event runs from 10 AM to 4 PM. Local artists will be giving live demonstrations throughout the day.",
                Category = "Arts & Culture",
                City = "Austin",
                Venue = "Zilker Park",
                Latitude = 30.2669,
                Longitude = -97.7725
            },
            new Activity
            {
                Title = "Charity 5K Run",
                Date = DateTime.Now.AddDays(35).Date.AddHours(9),
                Description = "Run or walk to support local children's charities. Registration opens at 8:00 AM, race starts at 9:00 AM on the Lakefront Trail. All finishers receive a medal and a free t-shirt. Water stations and first aid available along the route.",
                Category = "Sports",
                City = "Chicago",
                Venue = "Lakefront Trail",
                Latitude = 41.8826,
                Longitude = -87.6233
            },
            new Activity
            {
                Title = "Evening Yoga at the Beach",
                Date = DateTime.Now.AddDays(10).Date.AddHours(18),
                Description = "Unwind with a relaxing yoga session at Santa Monica Beach. All levels welcome. Please bring your own mat. The session will be led by certified instructor Maya Lin and will last 75 minutes, followed by a group meditation.",
                Category = "Health & Wellness",
                City = "Los Angeles",
                Venue = "Santa Monica Beach",
                Latitude = 34.0100,
                Longitude = -118.4962
            },
            new Activity
            {
                Title = "Board Game Night",
                Date = DateTime.Now.AddDays(17).Date.AddHours(19),
                Description = "Join us for a fun night of board games at The Game Room Café. Bring your favorite game or try something new from our collection. Snacks and drinks available for purchase. Event starts at 7:00 PM and goes until midnight.",
                Category = "Social",
                City = "Seattle",
                Venue = "The Game Room Café",
                Latitude = 47.6097,
                Longitude = -122.3331
            },
            new Activity
            {
                Title = "Photography Walk: City Lights",
                Date = DateTime.Now.AddDays(23).Date.AddHours(20),
                Description = "Capture the beauty of the city at night with fellow photography enthusiasts. Meet at Millennium Park at 8:00 PM. Bring your camera and tripod. We'll walk through downtown and share tips on night photography.",
                Category = "Hobbies",
                City = "Chicago",
                Venue = "Millennium Park",
                Latitude = 41.8827,
                Longitude = -87.6233
            },
            new Activity
            {
                Title = "Startup Pitch Night",
                Date = DateTime.Now.AddDays(30).Date.AddHours(18),
                Description = "Watch local startups pitch their ideas to a panel of investors at the Cambridge Innovation Center. Doors open at 6:00 PM, pitches start at 6:30 PM. Free pizza and drinks. RSVP required.",
                Category = "Business",
                City = "Boston",
                Venue = "Cambridge Innovation Center",
                Latitude = 42.3624,
                Longitude = -71.0846
            },
            new Activity
            {
                Title = "Community Garden Volunteer Day",
                Date = DateTime.Now.AddDays(40).Date.AddHours(9),
                Description = "Help us plant, weed, and harvest at the Brooklyn Community Garden. Tools and gloves provided. Coffee and pastries served at 9:00 AM. Great opportunity to meet neighbors and learn about urban gardening.",
                Category = "Volunteer",
                City = "New York",
                Venue = "Brooklyn Community Garden",
                Latitude = 40.6782,
                Longitude = -73.9442
            },
        };
    }
}
