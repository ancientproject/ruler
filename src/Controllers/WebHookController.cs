namespace ruler.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("/api/@me/hook")]
    public class WebHookController : ControllerBase
    {

        private readonly ILogger<WebHookController> _logger;

        public WebHookController(ILogger<WebHookController> logger) => _logger = logger;

    }
}
