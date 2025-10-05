using System;  
using Domain;  
using MediatR;  
using System.Net;  
using Persistence;
using System.Security.Cryptography.X509Certificates;
namespace Application.Activities.Queries;  

public class GetActivityDetails  
{  
    public class Query : IRequest<Activity>  
    {  
        public required string Id { get; set; }  
    }  

    public class Handler(AppDbContext context) : IRequestHandler<Query, Activity>  
    {  
        public async Task<Activity> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var activity = await context.Activities.FindAsync(new object[] { request.Id }, cancellationToken);

                if (activity == null)
                {
                    throw new HttpRequestException("Activity not found", null, HttpStatusCode.NotFound);
                }

                return activity;
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation if needed
                throw new HttpRequestException("Request timed out", null, HttpStatusCode.RequestTimeout);
            }
        }  
    }  
}