namespace ruler.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Ancient.ProjectSystem;
    using ByteSizeLib;
    using Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using NuGet.Versioning;

    [ApiController]
    public class StorageProxy : RulerController
    {
        private readonly IPackageSource _adapter;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StorageProxy> _logger;
        private readonly CancellationToken _cancellationToken;

        public StorageProxy(IPackageSource adapter, ITokenService tokenService, ILogger<StorageProxy> logger, CancellationToken cancellationToken = default)
        {
            _adapter = adapter;
            _tokenService = tokenService;
            _logger = logger;
            _cancellationToken = cancellationToken;
        }
        [AcceptVerbs("PROPFIND")]
        [Route("/api/registry/@/{id}/{version?}")]
        public async Task<IActionResult> IsExistPackage(string id, string version = null)
        {
            if (id is null)
                return StatusCode(400, "Incorrect search params");
            if (version != null && !NuGetVersion.TryParse(version, out _))
                return StatusCode(400, "Incorrect version format.");

            _logger.LogInformation($"PROPFIND /api/storage with id '{id}', version: '{version}'");

            if (await _adapter.IsExist(id, version))
                return Ok();
            return NotFound();
        }
        [HttpGet]
        [Route("/api/registry/@/{id}/{version?}")]
        public async Task<IActionResult> Fetch(string id, string version = null)
        {
            if (!await _adapter.IsExist(id))
                return StatusCode(404, $"Package with ID {id}{version} not found in registry.");
            var result = await _adapter.Get(id, version is null ? null : new NuGetVersion(version), _cancellationToken);

            return File(result.Content, "application/rpkg+zip", $"{result.ID}-{result.Version}.rpkg");
        }
        [HttpPut]
        [Route("/api/registry/@/")]
        public async Task<IActionResult> AddPackageAsync()
        {
            if (!Request.Headers.ContainsKey("X-Rune-Key"))
                return StatusCode(401, "You must use an authorization key 'X-Rune-Key' in request header.");
            var key = Request.Headers["X-Rune-Key"];
            if (!await _tokenService.IsValidToken(key))
                return StatusCode(401, "Authorization key is invalid.");
            if (Request.Form.Files.Count != 1)
                return StatusCode(406, "One package file was expected.");

            var files = Request.Form.Files;
            var file = files.First();

            if (ByteSize.FromBytes(file.Length).MegaBytes > 10)
                return StatusCode(413, "Package too large. [limit 10Mb]");

            var memory = new MemoryStream();
            await file.CopyToAsync(memory, _cancellationToken);
            var package = default(RunePackage);
            try
            {
                package = await RunePackage.Unwrap(memory.ToArray(), _cancellationToken);
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }

            if (await _adapter.IsExist(package))
                return StatusCode(422, $"Package {package.ID} {package.Version} has already exist in registry.");

            await _adapter.New(package);

            await package.DisposeAsync();

            return StatusCode(200);
        }

        [HttpGet]
        [AcceptVerbs("VIEW")]
        [Route("/api/registry/@/")]
        public async Task<IActionResult> GetFeedAsync([FromServices] IFireStoreAdapter adapter, [FromQuery] int page = 0, CancellationToken cancellationToken = default)
        {
            var result = await adapter.Packages
                .ListDocumentsAsync()
                .Skip(page * 40)
                .Take(40)
                .SelectMany(x => x
                    .Collection("list")
                    .ListDocumentsAsync()
                    .SelectAwait(async w => new
                    {
                        id = x.Id,
                        version = w.Id,
                        status = (await w.GetSnapshotAsync(cancellationToken)).GetValue<MetadataStatusType>("Status"),
                        downloads = (await w.GetSnapshotAsync(cancellationToken)).GetValue<long>("DownloadCount")
                    })
                )
                .ToListAsync(cancellationToken);
            return Ok(result);
        }
    }
}