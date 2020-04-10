namespace ProjectTest.Emulation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Octokit;
    using ruler.Features;
    using File = System.IO.File;

    public class GithubAdapterEmulator : IGithubAdapter
    {
        public Task<Func<Func<(string shaTree, string shaBranch), NewCommit>, Task<Commit>>> CreateCommitAsync(IDictionary<string, MemoryStream> files, CancellationToken cancellationToken = default) 
            => throw new NotImplementedException();

        public async Task<Func<Func<(string shaTree, string shaBranch), NewCommit>, Task<Commit>>> 
            CreateCommitAsync(string name, MemoryStream memory, CancellationToken cancellationToken = default)
        {
            var temp = Path.Combine(Path.GetTempPath(), "rune-test");

            var file = Path.Combine(temp, name);

            Directory.CreateDirectory(Path.GetDirectoryName(file));

            await File.WriteAllBytesAsync(file, memory.ToArray(), cancellationToken);

            static Task<Commit> functor(Func<(string shaTree, string shaBranch), NewCommit> func)
            {
                var result = func(("test1", "test2"));
                Console.WriteLine($"[CreateCommitAsync] {result.Message}");
                return Task.FromResult(new Commit());
            }

            return functor;
        }

        public async Task<byte[]> GetBinaryFileAsync(string path)
        {
            var files = await GetFilesAsync(path);
            var content = files.First().EncodedContent;
            return Convert.FromBase64String(content);
        }

        public Task<NewTreeItem> CreateTreeItem(string name, MemoryStream memory)
        {
            throw new NotImplementedException();
        }

        public Task<Func<Func<(string shaTree, string shaBranch), NewCommit>, Task<Commit>>> CreateCommitAsync(IEnumerable<NewTreeItem> files, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Reference> Push(Commit commit)
        {
            return Task.FromResult(new Reference());
        }

        public Task<BlobReference> CreateBlob(NewBlob blob)
        {
            throw new NotImplementedException();
        }

        public Func<Func<(string shaTree, string shaBranch), NewCommit>, Task<Commit>> CreateCommitAsync(string shaTree, string branchSha)
        {
            throw new NotImplementedException();
        }

        public Task<(string sha, IReadOnlyList<TreeItem> items)> FillTree(NewTree tree, IEnumerable<NewTreeItem> files)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<RepositoryContent>> GetFilesAsync(string path)
        {
            var temp = Path.Combine(Path.GetTempPath(), "rune-test");
            var file = Path.Combine(temp, path);
            await using var mem = new MemoryStream(await File.ReadAllBytesAsync(file));
            var base64 = Convert.ToBase64String(mem.ToArray());


            var list = new List<RepositoryContent>
            {
                new RepositoryContent(Path.GetFileName(path), path, "sha", (int) mem.Length, ContentType.File, "", "",
                    "", "", "base64", base64, "target", "")
            };

            return list.AsReadOnly();
        }

        public Task<NewTree> CreateNewTree()
        {
            throw new NotImplementedException();
        }

        public Task<Reference> GetMasterAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetLastCommitBy(string sha)
        {
            throw new NotImplementedException();
        }

        public ProductHeaderValue GetProductHeader()
        {
            throw new NotImplementedException();
        }

        public (string branch, string owner, string repo) GetConfig()
        {
            throw new NotImplementedException();
        }
    }
}