using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using Application.Core.Helper;

// Assume ExcelExporter exists in Infrastructure or relevant namespace
// using Infrastructure.Services; // DO NOT import here. Assume injected properly.

namespace Application.Activities.Queries;

public class GetActivityListCSV
{
    public class Query : IRequest<string> { }

    public class Handler : IRequestHandler<Query, string>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetActivityListCSV> _logger;
        private readonly ExcelExporter _excelExporter;

        public Handler(AppDbContext context, ILogger<GetActivityListCSV> logger, ExcelExporter excelExporter)
        {
            _context = context;
            _logger = logger;
            _excelExporter = excelExporter;
        }

        public async Task<string> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching activity list for CSV export");
                var activities = await _context.Activities.ToListAsync(cancellationToken);
                var filePath = "activities.csv";
                
                // Use ExcelExporter service to return CSV data as string
                _excelExporter.ExportToExcel(filePath + ".xlsx");
                return await File.ReadAllTextAsync(filePath + ".xlsx", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting activities to CSV");
                throw new HttpRequestException("An error occurred while exporting activities to CSV", ex, HttpStatusCode.InternalServerError);
            }
        }
    }
}
