using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RimDev.Stuntman.Core;

namespace UsageSampleMvc.AspNetCore
{
    public class Startup
    {
        public static readonly StuntmanOptions StuntmanOptions = new StuntmanOptions();

        public Startup(IConfiguration configuration)
        {
            StuntmanOptions
                .AddUser(new StuntmanUser("user-1", "User 1")
                    .AddClaim("given_name", "John")
                    .AddClaim("family_name", "Doe"))
                .AddUser(new StuntmanUser("user-2", "User 2")
                    .AddClaim("given_name", "Jane")
                    .AddClaim("family_name", "Doe"));

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStuntman(StuntmanOptions);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStuntman(StuntmanOptions);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
