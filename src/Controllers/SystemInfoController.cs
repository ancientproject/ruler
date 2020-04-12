namespace ruler.Controllers
{
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class SystemInfoController : ControllerBase
    {
        [HttpGet("/api/version")]
        public IActionResult GetVersion() => Ok(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        [HttpGet("/")]
        public IActionResult Index() => Redirect("https://cluster.ruler.runic.cloud/");
    }
}