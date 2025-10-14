using System;
using MediatR;
using Domain;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;

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
                var tempList = new List<Activity>();
                var activities = await context.Activities.ToListAsync(cancellationToken);
                foreach (var activity in activities)
                {                                    
                    // Simulate some work
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(1000, cancellationToken); // Simulate some async work

                    tempList.Add(activity);
                    var i = activities.IndexOf(activity);
                    
                    logger.LogInformation($"Processing {i+1}/{activities.Count} activities");
                }
                return tempList;
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
