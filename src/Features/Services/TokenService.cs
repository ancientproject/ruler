namespace ruler.Features
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class TokenService : ITokenService
    {
        private readonly IFireStoreAdapter _fireStoreAdapter;

        public TokenService(IFireStoreAdapter fireStoreAdapter) => _fireStoreAdapter = fireStoreAdapter;

        public ValueTask<bool> IsValidToken(string token)
            => _fireStoreAdapter
                .Tokens
                .ListDocumentsAsync()
                .Where(x => x.Id.Contains(token))
                .SelectAwait(async x => await x.GetSnapshotAsync())
                .Where(x => x.Exists)
                .Select(x => x.GetValue<DateTimeOffset>("expiredAt"))
                .AnyAsync(x => x > DateTimeOffset.UtcNow);
    }
}