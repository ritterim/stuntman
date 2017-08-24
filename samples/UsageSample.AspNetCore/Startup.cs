using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RimDev.Stuntman.Core;

namespace UsageSample.AspNetCore
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();

            services.AddSingleton(
                new StuntmanOptions()
                    .EnableServer()
                    .AddUser(new StuntmanUser("user-1", "User 1")
                        .SetAccessToken("user-1-token")
                        .SetDescription("This is User 1.")
                        .AddClaim("given_name", "John")
                        .AddClaim("family_name", "Doe"))
                    .AddUser(new StuntmanUser("user-2", "User 2")
                        .AddClaim("given_name", "Jane")
                        .AddClaim("family_name", "Doe"))
                    .AddUser(new StuntmanUser("user-3", "User 3")
                        .AddClaim("given_name", "Sam")
                        .AddClaim("family_name", "Smith"))
                    .AddUsersFromJson(
                        "https://raw.githubusercontent.com/ritterim/stuntman/master/samples/UsageSample.AspNetCore/test-users-1.json")
                    .AddUsersFromJson(Path.Combine(Environment.ContentRootPath, "test-users-2.json")));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            var stuntmanOptions = app.ApplicationServices.GetService<StuntmanOptions>();

            if (env.IsDevelopment())
            {
                app.UseStuntman(stuntmanOptions);
            }

            app.Map("/secure", secure =>
            {
                AuthenticateAllRequests(secure, "StuntmanAuthentication");

                secure.Run(async context =>
                {
                    var userName = context.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(
                        $"Hello, {userName}. This is the /secure endpoint.");

                    if (env.IsDevelopment())
                    {
                        await context.Response.WriteAsync(
                            stuntmanOptions.UserPicker(context.User));
                    }
                });
            });

            app.Map("/secure-json", secure =>
            {
                AuthenticateAllRequests(secure, "StuntmanAuthentication");

                secure.Run(async context =>
                {
                    var userName = context.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        $@"{{""message"":""Hello, {userName}. This is the /secure-json endpoint.""}}");

                });
            });

            app.Map("/logout", logout =>
            {
                logout.Run(async context =>
                {
                    await context.Authentication.SignOutAsync("StuntmanAuthentication");
                });
            });

            app.Map("", nonSecure =>
            {
                nonSecure.Run(async context =>
                {
                    var userName = context.User.Identity.Name;

                    if (string.IsNullOrEmpty(userName))
                        userName = "Anonymous / Unknown";

                    context.Response.ContentType = "text/html";

                    await context.Response.WriteAsync(
@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"">
        <title>Stuntman - UsageSample</title>
    </head>
<body>");

                    await context.Response.WriteAsync(
                        $"Hello, {userName}.");

                    if (env.IsDevelopment())
                    {
                        await context.Response.WriteAsync(
                            stuntmanOptions.UserPicker(context.User));
                    }

                    await context.Response.WriteAsync(
@"</body>
</html>");
                });
            });
        }

        // http://stackoverflow.com/a/26265757
        private static void AuthenticateAllRequests(IApplicationBuilder app, string authenticationType)
        {
            app.Use(async (context, continuation) =>
            {
                if (context.User != null &&
                    context.User.Identity != null &&
                    context.User.Identity.IsAuthenticated)
                {
                    await continuation();
                }
                else
                {
                    await context.Authentication.ChallengeAsync(authenticationType);
                }
            });
        }
    }
}
