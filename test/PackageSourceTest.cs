namespace ProjectTest
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Emulation;
    using Microsoft.Extensions.DependencyInjection;
    using NuGet.Versioning;
    using NUnit.Framework;
    using ruler.Features;
    public class PackageSourceTest : Context
    {
        public IPackageSource Source => this.Provider.GetRequiredService<IPackageSource>();
        public IGithubAdapter Github => this.Provider.GetRequiredService<IGithubAdapter>();

        public IFireStoreAdapter FireStore => this.Provider.GetRequiredService<IFireStoreAdapter>();
        [Test]
        public void InvokeTest()
        {
            _ = Source;
        }
        [Test]
        public async Task PushPackageTest()
        {
            var package = EmulatedPackageFactory.Create($"test-package-{Guid.NewGuid()}", new NuGetVersion("1.2.4-beta"));

            // publish new package
            await Source.New(package);

            // download raw package from git
            var resultBytes = await Github.GetBinaryFileAsync($"packages/{package.ID}/{package.Version}/target.rpkg");
            var targetBytes = package.Content.ToArray();
            Assert.AreEqual(targetBytes, resultBytes);

            // download metadata package
            var docRef = FireStore.Packages
                .Document(package.ID)
                .Collection("list")
                .Document($"{package.Version}");
            var snapshot = await docRef.GetSnapshotAsync();

            Assert.True(snapshot.Exists);

            Assert.True(snapshot.ContainsField("Status"));
            Assert.True(snapshot.ContainsField("DownloadCount"));
            Assert.AreEqual(MetadataStatusType.Listed, snapshot.GetValue<MetadataStatusType>("Status"));
        }

        protected override void Mount(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IGithubAdapter, GithubAdapterEmulator>();
            serviceCollection.AddScoped<IFireStoreAdapter, FirestoreEmulator>();
            serviceCollection.AddScoped<IPackageSource, GithubRunePackageSource>();
        }
    }

    
}