using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace API.Controllers{

    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase{
        private ILogger<BaseApiController>? _logger;
        
        protected ILogger<BaseApiController> logger => _logger
            ??= HttpContext.RequestServices.GetService<ILogger<BaseApiController>>()
            ?? throw new InvalidOperationException("Logger service not found");

        private IMediator? _mediator;

        protected IMediator mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>() 
            ?? throw new InvalidOperationException("Mediator service not found");
        
    }
}