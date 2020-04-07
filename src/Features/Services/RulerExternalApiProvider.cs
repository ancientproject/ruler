namespace ruler.Features
{
    using System.Threading;
    using System.Threading.Tasks;
    using Flurl.Http;

    public class RulerExternalApiProvider : IRulerAPI
    {
        private const string origin = "https://cluster.ruler.runic.cloud/api";
        private readonly IFireStoreAdapter _adapter;
        private readonly IAuthProvider _authProvider;

        public RulerExternalApiProvider(IFireStoreAdapter adapter, IAuthProvider authProvider)
        {
            _adapter = adapter;
            _authProvider = authProvider;
        }



        public async Task<string> GetGithubToken(CancellationToken cancellationToken = default)
        {
            var token = await _authProvider.GetTokenAsync(cancellationToken);
            return await $"{origin}/@me/container/github"
                .WithHeader("User-Agent", "api.ruler/cloud")
                .WithOAuthBearerToken(token)
                .GetStringAsync(cancellationToken);
        }
    }

    public interface IRulerAPI
    {
        Task<string> GetGithubToken(CancellationToken cancellationToken = default);
    }
}