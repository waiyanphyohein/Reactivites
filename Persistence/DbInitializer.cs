using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        if (await context.Activities.AnyAsync()) return;

        var activities = new List<Activity>
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

        await context.Activities.AddRangeAsync(activities);
        await context.SaveChangesAsync();
    }
}