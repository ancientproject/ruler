namespace ruler.Features
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class RulerAuthProvider : IAuthProvider
    {
        private const string AuthFileName = "rune.credentials";
        private const string Target = "api";
        private readonly IFireStoreAdapter _adapter;

        public RulerAuthProvider(IFireStoreAdapter adapter) => _adapter = adapter;


        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            var authFile = new FileInfo($"./{AuthFileName}");
            if (authFile.Exists)
            {
                if(authFile.CreationTimeUtc - DateTime.UtcNow > TimeSpan.FromHours(1))
                    authFile.Delete();
                else
                    return await File.ReadAllTextAsync($"./{AuthFileName}", cancellationToken);
            }
            var doc = _adapter.Cluster.Document("credentials");
            var snap = await doc.GetSnapshotAsync(cancellationToken);
            var token = snap.GetValue<string>(Target);

            await File.WriteAllTextAsync($"./{AuthFileName}", token, cancellationToken);
            return token;
        }
    }

    public interface IAuthProvider
    {
        Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}