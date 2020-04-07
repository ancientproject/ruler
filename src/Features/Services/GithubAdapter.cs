namespace ruler.Features
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Octokit;

    public class GithubAdapter
    {
        private readonly IRulerAPI _api;
        private readonly IConfiguration _configuration;

        private GitHubClient client
        {
            get
            {
                if (_handle is null)
                    return (_handle = InstallAsync(factoryToken).Result);
                return _handle;
            }
        }
        private readonly CancellationToken factoryToken;
        private GitHubClient _handle;
        public GithubAdapter(IRulerAPI api, IConfiguration configuration, CancellationToken cancellationToken = default)
        {
            _api = api;
            _configuration = configuration;
            factoryToken = cancellationToken;
        }


        private async Task<GitHubClient> InstallAsync(CancellationToken cancellationToken = default)
        {
            var token = await _api.GetGithubToken(cancellationToken);
            return new GitHubClient(GetProductHeader())
            {
                Credentials = new Credentials(token, AuthenticationType.Bearer)
            };
        }

        public async Task<Commit> CreateCommitAsync(IDictionary<string, MemoryStream> files,
            CancellationToken cancellationToken = default)
        {
            var list = new List<NewTreeItem>();
            foreach (var (name, memory) in files) 
                list.Add(await CreateTreeItem(name, memory));
            return await CreateCommitAsync(list, cancellationToken);
        }
        public async Task<Commit> CreateCommitAsync(string name, MemoryStream memory,
            CancellationToken cancellationToken = default)
        {
            var list = new List<NewTreeItem>
            {
                await CreateTreeItem(name, memory)
            };
            return await CreateCommitAsync(list, cancellationToken);
        }


        public async Task<NewTreeItem> CreateTreeItem(string name, MemoryStream memory)
        {
            var array = memory.ToArray();
            var base64String = Convert.ToBase64String(array);
            var blob = new NewBlob
            {
                Content = base64String,
                Encoding = EncodingType.Base64
            };
            var result = await this.CreateBlob(blob);
            return new NewTreeItem
            {
                Type = TreeType.Blob,
                Mode = Octokit.FileMode.File,
                Path = name,
                Sha = result.Sha
            };
        }


            
        public async Task<Commit> CreateCommitAsync(IEnumerable<NewTreeItem> files, CancellationToken cancellationToken = default)
        {
            var branch = await GetMasterAsync();
            var newTree = await CreateNewTree();
            var (treeSha, _) = await FillTree(newTree, files);
            return await CreateCommitAsync(treeSha, branch.Object.Sha);
        }

        public async Task<Reference> Push(Commit commit)
        {
            var (branch, owner, repo) = GetConfig();
            return await client.Git.Reference.Update(owner, repo, branch, new ReferenceUpdate(commit.Sha));
        }

        public async Task<BlobReference> CreateBlob(NewBlob blob)
        {
            var (_, owner, repo) = GetConfig();
            return await client.Git.Blob.Create(owner, repo, blob);
        }

        public async Task<Commit> CreateCommitAsync(string shaTree, string branchSha)
        {
            var (_, owner, repo) = GetConfig();
            var newCommit = new NewCommit("Publish new package.", shaTree, branchSha);
            return await client.Git.Commit.Create(owner, repo, newCommit);
        }

        public async Task<(string sha, IReadOnlyList<TreeItem> items)> FillTree(NewTree tree, IEnumerable<NewTreeItem> files)
        {
            var (_, owner, repo) = GetConfig();
            foreach (var item in files)
                tree.Tree.Add(item);
            var result = await client.Git.Tree.Create(owner, repo, tree);
            return (result.Sha, result.Tree);
        }


        public async Task<NewTree> CreateNewTree()
        {
            var masterReference = await GetMasterAsync();
            var latestCommit = await GetLastCommitBy(masterReference.Object.Sha);
            return new NewTree
            {
                BaseTree = latestCommit.Tree.Sha
            };
        }

        public Task<Reference> GetMasterAsync()
        {
            var (branch, owner, repo) = GetConfig();

            return client.Git.Reference.Get(owner, repo, branch);
        }

        public Task<Commit> GetLastCommitBy(string sha)
        {
            var (_, owner, repo) = GetConfig();
            return client.Git.Commit.Get(owner, repo, sha);
        }

        public ProductHeaderValue GetProductHeader() 
            => new ProductHeaderValue("ancient-rune-ruler");


        public (string branch, string owner, string repo) GetConfig()
        {
            var branch = _configuration["Github:Branch"];
            var owner = _configuration["Github:Owner"];
            var repo = _configuration["Github:Repository"];

            return (branch, owner, repo);
        }
    }
}