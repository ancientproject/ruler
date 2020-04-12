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
    using Microsoft.AspNetCore.Mvc.Routing;
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
        [AcceptVerbs("PROPFIND")]
        [Route("/api/storage")]
        public async Task<IActionResult> IsExistPackage([FromQuery] string id, [FromQuery] string version = null)
        {
            if (id is null)
                return StatusCode(400, new {message = "Incorrect search params"});
            if (version != null && !NuGetVersion.TryParse(version, out _))
                return StatusCode(400, new { message = "Incorrect version format." });

            if (await _adapter.IsExist(id, version))
                return Ok();
            return NotFound();
        }
        [HttpGet]
        [Route("/api/storage")]
        public async Task<IActionResult> Fetch([FromQuery] string id, [FromQuery] string version = null, [FromServices] CancellationToken cancellationToken = default)
        {
            if (!await _adapter.IsExist(id))
                return StatusCode(404, new { message = $"Package with ID {id}{version} not found in registry." });
            var result = await _adapter.Get(id, version is null ? null : new NuGetVersion(version), cancellationToken);

            return File(result.Content.ToArray(), "application/rpkg+zip");
        }
        [HttpPut]
        [Route("/api/storage")]
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