namespace ruler.Features
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Ancient.ProjectSystem;
    using Flurl.Http;
    using Microsoft.Extensions.Logging;
    using NuGet.Versioning;
    using Octokit;

    public class GithubRunePackageSource : PackageStorage
    {
        private readonly IGithubAdapter _adapter;
        private readonly IFireStoreAdapter _fireStore;
        private readonly ILogger<GithubRunePackageSource> _logger;

        public GithubRunePackageSource(IGithubAdapter adapter, IFireStoreAdapter fireStore, ILogger<GithubRunePackageSource> logger)
        {
            _adapter = adapter;
            _fireStore = fireStore;
            _logger = logger;
        }
        public override async Task<bool> IsExist(string ID, string version = null)
        {
            if (version != null)
                return await IsExist(ID, new NuGetVersion(version));
            var collection = await _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .GetSnapshotAsync();
            var exists = collection.Count > 0;
            _logger.LogInformation($"Snapshot by ~#/{ID}/ -> exist: {exists}");
            return exists;
        }
        public override async Task<bool> IsExist(string ID, NuGetVersion version)
        {
            var snapshot = await _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .Document($"{version}")
                .GetSnapshotAsync();
            _logger.LogInformation($"Snapshot by ~#/{ID}/list/{version}/ -> exist: {snapshot.Exists}");
            return snapshot.Exists;
        }

        public override Task<bool> IsExist(RunePackage package)
            => IsExist(package.ID, package.Version);

        public override async Task New(RunePackage package)
        {
            var formattedName = $"{package.ID} {package.Version}";
            var path = $"packages/{package.ID}/{package.Version}";

            var postCreate = 
                await _adapter.CreateCommitAsync($"{path}/target.rpkg", package.Content);
            var commit = await postCreate( x 
                => new NewCommit($"Upload new package, {formattedName}", x.shaTree, x.shaBranch));

            await _adapter.Push(commit);

            var metadata = _fireStore.Packages
                .Document(package.ID)
                .Collection("list")
                .Document($"{package.Version}");

            await metadata.SetAsync(new RunePackageMetadata {Status = MetadataStatusType.Listed});
        }

        public override async Task Delete(string ID, NuGetVersion version, string reason)
        {
            var formattedName = $"{ID} {version}";
            var path = $"packages/{ID}/{version}";


            await using var memoryStream = new MemoryStream();
            await using var writer = new StreamWriter(memoryStream);

            await writer.WriteLineAsync($"unlisted|{DateTimeOffset.UtcNow:R}|{reason}");

            var postCreate =
                await _adapter.CreateCommitAsync($"{path}/unlisted.tag", memoryStream);
            var commit = await postCreate(x
                => new NewCommit($"Unlist package {formattedName}", x.shaTree, x.shaBranch));
            await _adapter.Push(commit);

            var metadata = _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .Document($"{version}");

            var collection = new Dictionary<string, object>
            {
                {"Status", MetadataStatusType.Unlisted}
            };


            await metadata.UpdateAsync(collection);
        }

        public override async Task<RunePackage> Get(string ID, NuGetVersion version = null, CancellationToken cancellationToken = default)
        {
            if (version is null)
                version = await SelectLastVersionAsync(ID, cancellationToken);
            else
                version = await FindMathByVersion(ID, version, cancellationToken);



            var metadataRef = _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .Document($"{version}");
            var snapshot = await metadataRef.GetSnapshotAsync(cancellationToken);

            if (!snapshot.Exists)
                throw new NotFoundInRegistryException(ID, version);
            var metadata = snapshot.ConvertTo<RunePackageMetadata>();

            metadata.DownloadCount++;

            await metadataRef.UpdateAsync(
                    nameof(metadata.DownloadCount),
                    metadata.DownloadCount, 
                    cancellationToken: cancellationToken);

            var collection = 
                await _adapter.GetFilesAsync($"packages/{ID}/{version}/target.rpkg");
            var result = collection.First();


            var content = Convert.FromBase64String(result.EncodedContent);

            return new RunePackage
            {
                ID = ID,
                Version = version,
                Content = new MemoryStream(content)
            };
        }

        

        public async ValueTask<NuGetVersion> SelectLastVersionAsync(string ID, CancellationToken cancellationToken = default) =>
            await _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .ListDocumentsAsync()
                .Select(x => new NuGetVersion(x.Id))
                .OrderBy(x => x)
                .FirstAsync(cancellationToken);

        public async Task<NuGetVersion> FindMathByVersion(string ID, NuGetVersion target, CancellationToken cancellationToken = default) =>
            (await _fireStore.Packages
                .Document(ID)
                .Collection("list")
                .ListDocumentsAsync()
                .Select(x => new NuGetVersion(x.Id))
                .ToListAsync(cancellationToken))
            .FindBestMatch(new VersionRange(target), x => x);
    }

    public class NotFoundInRegistryException : Exception
    {
        public NotFoundInRegistryException(string ID, NuGetVersion version)
            : base($"Package [{ID}-{version}] not found in package registry.") { }
    }
}