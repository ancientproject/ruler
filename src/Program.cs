namespace ruler
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args) 
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(Configure);


        public static void Configure(IWebHostBuilder webBuilder)
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            var sentryDns = Environment.GetEnvironmentVariable("SENTRY_DNS") ??
                            throw new Exception($"Not configured");
            webBuilder
                .UseStartup<Startup>()
                .UseSentry(sentryDns)
                .UseUrls($"http://0.0.0.0:{port}");
        }
    }
}
