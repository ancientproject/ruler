namespace ProjectTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    public abstract class Context
    {
        public IServiceProvider Provider { get; private set; }
        private IServiceCollection collection { get; set; }

        [OneTimeSetUp]
        public void StartUp()
        {
            collection = new ServiceCollection();

            Configure(out var cfg);

            collection.AddLogging();
            collection.AddSingleton<IConfiguration>(x => new ConfigurationBuilder().AddInMemoryCollection(
                new List<KeyValuePair<string, string>>(cfg.ToArray())
            ).Build());
            Mount(collection);

            Provider = collection.BuildServiceProvider();
        }

        protected abstract void Mount(IServiceCollection serviceCollection);

        protected virtual void Configure(out Dictionary<string, string> config) =>
            config = new Dictionary<string, string>();
    }
}