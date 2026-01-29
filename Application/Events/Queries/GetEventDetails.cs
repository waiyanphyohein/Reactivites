using System;  
using Domain;  
using MediatR;  
using System.Net;  
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Events.Queries;  

public class GetEventDetails  
{  
    public class Query : IRequest<Event>  
    {  
        public required Guid EventId { get; set; }  
    }  

    public class Handler(AppDbContext context, ILogger<Handler> logger) : IRequestHandler<Query, Event>  
    {  
        public async Task<Event> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Fetching event details for EventId: {EventId}", request.EventId);
                var eventEntity = await context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);

                if (eventEntity == null)
                {
                    logger.LogWarning("Event with ID {EventId} not found", request.EventId);
                    throw new HttpRequestException("Event not found", null, HttpStatusCode.NotFound);
                }

                logger.LogInformation("Event with ID {EventId} retrieved successfully: {EventName}", eventEntity.EventId, eventEntity.EventName);
                return eventEntity;
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                logger.LogWarning("Request timed out while fetching event details for EventId: {EventId}", request.EventId);
                throw new HttpRequestException("Request timed out", null, HttpStatusCode.RequestTimeout);
            }
        }  
    }  
}

