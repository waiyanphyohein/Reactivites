using System;
using MediatR;
using Persistence;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.Activities.Queries;

public class DeleteActivity
{
    public class Command : IRequest { 
        public required string Id { get; set; }
    }

    public class Handler(AppDbContext context, ILogger<Handler> logger) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var activity = await context.Activities.FindAsync(new object[] { request.Id }, cancellationToken);
                if (activity == null)
                {
                    throw new KeyNotFoundException("Activity not found");
                }

                // Simulate some async work and check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken); // Simulate some async work

                context.Activities.Remove(activity);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                logger.LogWarning("Request timed out");
                throw new HttpRequestException("Request timed out", null, System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (KeyNotFoundException)
            {
                logger.LogWarning("Activity not found");
                throw; // Rethrow to be handled by the controller
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                logger.LogError(ex, "An error occurred while deleting the activity");
                throw new HttpRequestException("An error occurred while deleting the activity", ex, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}