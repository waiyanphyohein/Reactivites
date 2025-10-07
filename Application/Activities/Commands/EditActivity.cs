using System;
using MediatR;
using Domain;
using Persistence;

namespace Application.Activities.Queries;

public class EditActivity
{
    public class Command(Guid id, Activity Activity) : IRequest<Activity>
    {
        public Guid Id { get; } = id;
        public Activity Activity { get; } = Activity;
    }

    public class Handler : IRequestHandler<Command, Activity>
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Activity> Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await _context.Activities.FindAsync(new object[] { request.Id }, cancellationToken);

            if (activity == null)
            {
                throw new KeyNotFoundException("Activity not found");
            }

            // Update only the fields that are provided (not null)
            if (!string.IsNullOrEmpty(request.Activity.Title))
                activity.Title = request.Activity.Title;

            if (request.Activity.Date != default)
                activity.Date = request.Activity.Date;

            if (!string.IsNullOrEmpty(request.Activity.Description))
                activity.Description = request.Activity.Description;

            if (!string.IsNullOrEmpty(request.Activity.Category))
                activity.Category = request.Activity.Category;

            if (!string.IsNullOrEmpty(request.Activity.City))
                activity.City = request.Activity.City;

            if (!string.IsNullOrEmpty(request.Activity.Venue))
                activity.Venue = request.Activity.Venue;

            if (request.Activity.Latitude != 0)
                activity.Latitude = request.Activity.Latitude;

            if (request.Activity.Longitude != 0)
                activity.Longitude = request.Activity.Longitude;

            await _context.SaveChangesAsync(cancellationToken);

            return activity;
        }
    }   
}
