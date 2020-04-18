namespace ruler.Controllers
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;

    public class RulerController : ControllerBase
    {
        public ObjectResult StatusCode(int statusCode, string message) 
            => base.StatusCode(statusCode, new
            {
                code = statusCode,
                message, 
                traceId = this.HttpContext.TraceIdentifier
            });
        public ObjectResult StatusCode(HttpStatusCode statusCode, string message)
            => base.StatusCode((int)statusCode, new
            {
                code = (int)statusCode, 
                message, 
                traceId = this.HttpContext.TraceIdentifier
            });
    }
}