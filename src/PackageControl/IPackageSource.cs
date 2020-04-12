namespace ruler.Features
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Ancient.ProjectSystem;
    using NuGet.Versioning;

    public interface IPackageSource
    {
        Task New([DisallowNull] RunePackage package);
        Task Delete([DisallowNull] string ID, [DisallowNull] NuGetVersion version, [DisallowNull] string reason);
        Task<RunePackage> Get(string ID, NuGetVersion version = null, CancellationToken cancellationToken = default);
        Task<bool> IsExist(RunePackage package);
    }
}