using System;
using MediatR;
using Domain;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Application.Activities.Queries;

public class GetActivityList
{
    public class Query : IRequest<List<Activity>> { }

    public class Handler(AppDbContext context, ILogger<GetActivityList> logger) : IRequestHandler<Query, List<Activity>>
    {
        public async Task<List<Activity>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Implementation to get the list of activities
            try
            {
                logger.LogInformation("Fetching activity list");
                return await context.Activities.ToListAsync(cancellationToken);              
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
