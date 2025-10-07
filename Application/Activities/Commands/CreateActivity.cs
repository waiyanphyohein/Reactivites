using System;
using MediatR;
using Domain;
using Persistence;

namespace Application.Activities.Queries;

public class CreateActivity
{
    public class Command : IRequest<Activity>
    {
        public required Activity Activity { get; set; }
    }
    
    public class Handler(AppDbContext context) : IRequestHandler<Command, Activity>
    {
        public async Task<Activity> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Assertions to ensure Activity is not null and has a valid Id
                if (request.Activity == null)
                {
                    throw new HttpRequestException("Activity cannot be null", null, System.Net.HttpStatusCode.BadRequest);
                }

                // Generate a new ID if not provided
                if (request.Activity.Id == Guid.Empty.ToString())
                {
                    request.Activity.Id = Guid.NewGuid().ToString();
                }

                // Add the new activity to the database
                context.Activities.Add(request.Activity);
                await context.SaveChangesAsync(cancellationToken);
                return request.Activity;
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                throw new HttpRequestException("Request timed out", null, System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new HttpRequestException("An error occurred while creating the activity", ex, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }

}
