using MediatR;
using Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Application.Core.Helper;

namespace Application.Events.Queries;

public class GetEventListExcel
{
    public class Query : IRequest<byte[]> { }

    public class Handler : IRequestHandler<Query, byte[]>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetEventListExcel> _logger;
        private readonly ExcelExporter _excelExporter;

        public Handler(AppDbContext context, ILogger<GetEventListExcel> logger, ExcelExporter excelExporter)
        {
            _context = context;
            _logger = logger;
            _excelExporter = excelExporter;
        }

        public async Task<byte[]> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Exporting event list to Excel format");
                var eventCount = await _context.Events.CountAsync(cancellationToken);
                _logger.LogInformation("Found {EventCount} events to export to Excel", eventCount);
                
                var excelBytes = await Task.Run(() => _excelExporter.ExportEventsToExcelBytes(), cancellationToken);
                
                _logger.LogInformation("Excel export completed successfully for {EventCount} events", eventCount);
                return excelBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting events to Excel");
                throw;
            }
        }
    }
}

