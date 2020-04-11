namespace ruler.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Microsoft.AspNetCore.Mvc;
    using NuGet.Versioning;

    [ApiController]
    public class StorageProxy : ControllerBase
    {
        private readonly IPackageSource _adapter;
        private readonly CancellationToken _cancellationToken;

        public StorageProxy(IPackageSource adapter, CancellationToken cancellationToken = default)
        {
            _adapter = adapter;
            _cancellationToken = cancellationToken;
        }

        [HttpPost("/api/storage")]
        public async Task<IActionResult> AddPackageAsync([FromQuery]string id, [FromQuery]string version)
        {
            var files = Request.Form.Files;
            var file = files.First();
            var memory = new MemoryStream();
            await file.CopyToAsync(memory, _cancellationToken);

            await _adapter.New(new RunePackage {Content = memory, ID = id, Version = new NuGetVersion(version)});



            return StatusCode(200);
        }
    }
}