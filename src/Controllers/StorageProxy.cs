namespace ruler.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ancient.ProjectSystem;
    using Features;
    using Microsoft.AspNetCore.Mvc;

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
            var package = default(RunePackage);
            try
            {
                package = await RunePackage.Unwrap(memory, _cancellationToken);
            }
            catch (Exception e)
            {
                return StatusCode(400, new {message = e.Message});
            }

            if (await _adapter.IsExist(package))
                return StatusCode(422, new { message = $"Package {package.ID} {package.Version} has already exist in registry." });

            await _adapter.New(package);

            await package.DisposeAsync();

            return StatusCode(200);
        }
    }
}