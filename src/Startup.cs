namespace ruler
{
    using Features;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IFireStoreAdapter, FireStoreAdapter>();
            services.AddScoped<IRulerAPI, RulerExternalApiProvider>();
            services.AddScoped<IAuthProvider, RulerAuthProvider>();
            services.AddScoped<IGithubAdapter, GithubAdapter>();
            services.AddScoped<IPackageSource, GithubRunePackageSource>();
            services.AddScoped<ITokenService, TokenService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) 
                app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => 
                endpoints.MapControllers());
        }
    }
}
