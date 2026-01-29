using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using Application.Core.Helper;
using System.Text;

namespace Application.Events.Queries;

public class GetEventListCSV
{
    public class Query : IRequest<string> { }

    public class Handler : IRequestHandler<Query, string>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetEventListCSV> _logger;
        private readonly ExcelExporter _excelExporter;

        public Handler(AppDbContext context, ILogger<GetEventListCSV> logger, ExcelExporter excelExporter)
        {
            _context = context;
            _logger = logger;
            _excelExporter = excelExporter;
        }

        public async Task<string> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching event list for CSV export");
                var events = await _context.Events.ToListAsync(cancellationToken);
                
                _logger.LogInformation("Found {EventCount} events for CSV export", events.Count);
                
                // Generate CSV content
                var csv = new StringBuilder();
                csv.AppendLine("EventId,EventName,EventDescription,Location,GroupId,GroupName,GroupDescription");
                
                foreach (var eventEntity in events)
                {
                    csv.AppendLine($"{eventEntity.EventId},{EscapeCsvField(eventEntity.EventName)},{EscapeCsvField(eventEntity.EventDescription)},{EscapeCsvField(eventEntity.Location)},{eventEntity.GroupId},{EscapeCsvField(eventEntity.GroupName)},{EscapeCsvField(eventEntity.GroupDescription)}");
                }
                
                _logger.LogInformation("CSV export completed successfully for {EventCount} events", events.Count);
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting events to CSV");
                throw new HttpRequestException("An error occurred while exporting events to CSV", ex, HttpStatusCode.InternalServerError);
            }
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;
            
            // If field contains comma, quote, or newline, wrap in quotes and escape quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}

