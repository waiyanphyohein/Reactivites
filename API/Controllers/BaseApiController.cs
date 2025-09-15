using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace API.Controllers{

    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase{
        private readonly ILogger<BaseApiController> _logger;

        public BaseApiController(ILogger<BaseApiController> logger){
            _logger = logger;
        }
    }
}