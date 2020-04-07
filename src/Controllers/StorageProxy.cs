namespace ruler.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Features;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Octokit;

    [ApiController]
    public class StorageProxy : ControllerBase
    {
        private readonly GithubAdapter _adapter;

        public StorageProxy(GithubAdapter adapter) => _adapter = adapter;

        [HttpPost("/api/storage")]
        public async Task<IActionResult> AddPackageAsync([FromBody] IFormFile file, CancellationToken cancellationToken = default)
        {
            await using var memory = new MemoryStream();
            await file.CopyToAsync(memory, cancellationToken);
            

            var commit = await _adapter.CreateCommitAsync("packages/test.nuget", memory, cancellationToken);

            await _adapter.Push(commit);

            return StatusCode(200);
        }
    }
}