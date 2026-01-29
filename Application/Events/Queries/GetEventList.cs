using System;
using MediatR;
using Domain;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Application.Events.Queries;

public class GetEventList
{
    public class Query : IRequest<List<Event>> { }

    public class Handler(AppDbContext context, ILogger<GetEventList> logger) : IRequestHandler<Query, List<Event>>
    {
        public async Task<List<Event>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Implementation to get the list of events
            try
            {
                logger.LogInformation("Fetching event list");
                return await context.Events.ToListAsync(cancellationToken);              
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                logger.LogWarning("Request timed out");
                throw new HttpRequestException("Request timed out", null, HttpStatusCode.RequestTimeout);
            }
        }
    }
}

