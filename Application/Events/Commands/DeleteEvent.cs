using System;
using MediatR;
using Persistence;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands;

public class DeleteEvent
{
    public class Command : IRequest { 
        public required Guid EventId { get; set; }
    }

    public class Handler(AppDbContext context, ILogger<Handler> logger) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Deleting event with ID: {EventId}", request.EventId);
                var eventEntity = await context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);
                if (eventEntity == null)
                {
                    logger.LogWarning("Event with ID {EventId} not found for deletion", request.EventId);
                    throw new KeyNotFoundException("Event not found");
                }

                // Simulate some async work and check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken); // Simulate some async work

                context.Events.Remove(eventEntity);
                await context.SaveChangesAsync(cancellationToken);
                
                logger.LogInformation("Event with ID {EventId} deleted successfully: {EventName}", eventEntity.EventId, eventEntity.EventName);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                logger.LogWarning("Request timed out");
                throw new HttpRequestException("Request timed out", null, System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (KeyNotFoundException)
            {
                logger.LogWarning("Event not found");
                throw; // Rethrow to be handled by the controller
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                logger.LogError(ex, "An error occurred while deleting the event");
                throw new HttpRequestException("An error occurred while deleting the event", ex, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}

