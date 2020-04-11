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
        private readonly ITokenService _tokenService;
        private readonly CancellationToken _cancellationToken;

        public StorageProxy(IPackageSource adapter, ITokenService tokenService, CancellationToken cancellationToken = default)
        {
            _adapter = adapter;
            _tokenService = tokenService;
            _cancellationToken = cancellationToken;
        }

        [HttpPost("/api/storage")]
        public async Task<IActionResult> AddPackageAsync()
        {
            if (!Request.Headers.ContainsKey("X-Rune-Key"))
                return StatusCode(401, new {message = "You must use an authorization key 'X-Rune-Key' in request header." });
            var key = Request.Headers["X-Rune-Key"];
            if (!await _tokenService.IsValidToken(key))
                return StatusCode(401, new {message = "Authorization key is invalid." });
            if (Request.Form.Files.Count == 0)
                return StatusCode(406);
            if (Request.Form.Files.Count != 1)
                return StatusCode(406, new { message = "One package file was expected." });

            var files = Request.Form.Files;
            var file = files.First();
            var memory = new MemoryStream();
            await file.CopyToAsync(memory, _cancellationToken);

            await _adapter.New(new RunePackage
            {
                Content = memory, 
                ID = id, 
                Version = new NuGetVersion(version)
            });

            return StatusCode(200);
        }
    }
}