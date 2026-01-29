using System;
using MediatR;
using Domain;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands;

public class CreateEvent
{
    public class Command : IRequest<Event>
    {
        public required Event Event { get; set; }
    }
    
    public class Handler(AppDbContext context, ILogger<Handler> logger) : IRequestHandler<Command, Event>
    {
        public async Task<Event> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Assertions to ensure Event is not null and has a valid EventId
                if (request.Event == null)
                {
                    logger.LogWarning("Attempted to create event with null Event object");
                    throw new HttpRequestException("Event cannot be null", null, System.Net.HttpStatusCode.BadRequest);
                }

                // Generate a new EventId if not provided or empty
                if (request.Event.EventId == Guid.Empty)
                {
                    request.Event.EventId = Guid.NewGuid();
                    logger.LogInformation("Generated new EventId: {EventId}", request.Event.EventId);
                }

                // Generate a new GroupId if not provided or empty
                if (request.Event.GroupId == Guid.Empty)
                {
                    request.Event.GroupId = Guid.NewGuid();
                    logger.LogInformation("Generated new GroupId: {GroupId} for Event: {EventId}", request.Event.GroupId, request.Event.EventId);
                }

                // Add the new event to the database
                context.Events.Add(request.Event);
                await context.SaveChangesAsync(cancellationToken);
                
                logger.LogInformation("Event created successfully with ID {EventId}: {EventName}", request.Event.EventId, request.Event.EventName);
                return request.Event;
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                logger.LogWarning("Request timed out while creating event");
                throw new HttpRequestException("Request timed out", null, System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                logger.LogError(ex, "An error occurred while creating the event");
                throw new HttpRequestException("An error occurred while creating the event", ex, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}