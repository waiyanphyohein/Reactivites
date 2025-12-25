using MediatR;
using Persistence;
using Microsoft.Extensions.Logging;
using Application.Core.Helper;

namespace Application.Activities.Queries;

public class GetActivityListExcel
{
    public class Query : IRequest<byte[]> { }

    public class Handler : IRequestHandler<Query, byte[]>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetActivityListExcel> _logger;
        private readonly ExcelExporter _excelExporter;

        public Handler(AppDbContext context, ILogger<GetActivityListExcel> logger, ExcelExporter excelExporter)
        {
            _context = context;
            _logger = logger;
            _excelExporter = excelExporter;
        }

        public async Task<byte[]> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Exporting activity list to Excel format");
                return await Task.Run(() => _excelExporter.ExportToExcelBytes(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting activities to Excel");
                throw;
            }
        }
    }
}

