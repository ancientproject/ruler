namespace ruler.Features
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Ancient.ProjectSystem;
    using NuGet.Versioning;

    public abstract class PackageStorage : IPackageSource
    {
        public abstract Task New([DisallowNull] RunePackage package);
        public abstract Task Delete([DisallowNull] string ID, [DisallowNull] NuGetVersion version, [DisallowNull] string reason);
        public abstract Task<bool> IsExist(RunePackage package);
        public abstract Task<RunePackage> Get(string ID, NuGetVersion version = null,
            CancellationToken cancellationToken = default);
    }
}