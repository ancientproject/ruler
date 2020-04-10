namespace ruler.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    public class StorageProxy : ControllerBase
    {
        private readonly GithubAdapter _adapter;
        private readonly CancellationToken _cancellationToken;

        public StorageProxy(GithubAdapter adapter, CancellationToken cancellationToken = default)
        {
            _adapter = adapter;
            _cancellationToken = cancellationToken;
        }

        [HttpPost("/api/storage")]
        public async Task<IActionResult> AddPackageAsync()
        {
            var files = Request.Form.Files;
            var file = files.First();
            await using var memory = new MemoryStream();
            await file.CopyToAsync(memory, _cancellationToken);
            
            return StatusCode(200);
        }
    }
}