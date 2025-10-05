using System;
using MediatR;
using Domain;
using Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Application.Activities.Queries;

public class GetActivityList
{
    public class Query : IRequest<List<Activity>> { }

    public class Handler(AppDbContext context) : IRequestHandler<Query, List<Activity>>
    {
        public async Task<List<Activity>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Implementation to get the list of activities
            try
            {
                return await context.Activities.ToListAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                throw new HttpRequestException("Request timed out", null, HttpStatusCode.RequestTimeout);
            }
        }
    }
}
