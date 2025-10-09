using System;
using MediatR;
using Domain;
using Persistence;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Activities.Queries;

public class EditActivity
{
    public class Command(Activity Activity) : IRequest<Activity>
    {
        public Activity Activity { get; } = Activity;
    }

    public class Handler(AppDbContext context, IMapper mapper, ILogger<Handler> logger) : IRequestHandler<Command, Activity>
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<Handler> _logger = logger;

        public async Task<Activity> Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await _context.Activities.FindAsync(new object[] { request.Activity.Id }, cancellationToken) ?? throw new KeyNotFoundException("Activity not found");

            // Update only the fields that are provided (not null)
            _mapper.Map(request.Activity, activity);

            _logger.LogInformation("Activity with ID {ActivityId} updated successfully: {Activity}", activity.Id, activity);
            // await _context.SaveChangesAsync(cancellationToken);

            // if (request.Activity.Date != default)
            //     activity.Date = request.Activity.Date;

            // if (!string.IsNullOrEmpty(request.Activity.Description))
            //     activity.Description = request.Activity.Description;

            // if (!string.IsNullOrEmpty(request.Activity.Category))
            //     activity.Category = request.Activity.Category;

            // if (!string.IsNullOrEmpty(request.Activity.City))
            //     activity.City = request.Activity.City;

            // if (!string.IsNullOrEmpty(request.Activity.Venue))
            //     activity.Venue = request.Activity.Venue;

            // if (request.Activity.Latitude != 0)
            //     activity.Latitude = request.Activity.Latitude;

            // if (request.Activity.Longitude != 0)
            //     activity.Longitude = request.Activity.Longitude;

            await _context.SaveChangesAsync(cancellationToken);

            return activity;
        }
    }
}
